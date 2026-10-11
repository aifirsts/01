// SOCpit - Security Operations Center
// Evidence view model for WPF application
// Based on MATRIX V3 with MVVM pattern

using Microsoft.Extensions.Logging;
using SOCpit.Application.Services;
using SOCpit.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SOCpit.App.ViewModels
{
    /// <summary>Evidence view model.</summary>
    public sealed class EvidenceViewModel : INotifyPropertyChanged
    {
        private readonly EvidenceService _evidenceService;
        private readonly IncidentService _incidentService;
        private readonly ThreatService _threatService;
        private readonly ILogger<EvidenceViewModel> _logger;

        private ObservableCollection<Evidence> _evidence = new();
        private Evidence? _selectedEvidence;
        private EvidenceType _selectedTypeFilter = EvidenceType.Log;
        private EvidenceStatus _selectedStatusFilter = EvidenceStatus.New;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;

        public EvidenceViewModel(
            EvidenceService evidenceService,
            IncidentService incidentService,
            ThreatService threatService,
            ILogger<EvidenceViewModel> logger)
        {
            _evidenceService = evidenceService;
            _incidentService = incidentService;
            _threatService = threatService;
            _logger = logger;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEditOrDelete);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, CanEditOrDelete);
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => _isEditMode);
            CancelCommand = new RelayCommand(Cancel, () => _isEditMode);
            FilterCommand = new AsyncRelayCommand(FilterAsync);

            TypeFilters = new ObservableCollection<EvidenceType>(Enum.GetValues(typeof(EvidenceType)).Cast<EvidenceType>());
            StatusFilters = new ObservableCollection<EvidenceStatus>(Enum.GetValues(typeof(EvidenceStatus)).Cast<EvidenceStatus>());

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Evidence> Evidence
        {
            get => _evidence;
            set
            {
                if (_evidence != value)
                {
                    _evidence = value;
                    OnPropertyChanged();
                }
            }
        }

        public Evidence? SelectedEvidence
        {
            get => _selectedEvidence;
            set
            {
                if (_selectedEvidence != value)
                {
                    _selectedEvidence = value;
                    OnPropertyChanged();
                }
            }
        }

        public EvidenceType SelectedTypeFilter
        {
            get => _selectedTypeFilter;
            set
            {
                if (_selectedTypeFilter != value)
                {
                    _selectedTypeFilter = value;
                    OnPropertyChanged();
                }
            }
        }

        public EvidenceStatus SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (_selectedStatusFilter != value)
                {
                    _selectedStatusFilter = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<EvidenceType> TypeFilters { get; }
        public ObservableCollection<EvidenceStatus> StatusFilters { get; }

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand FilterCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var evidence = await _evidenceService.GetAllAsync();
                Evidence = new ObservableCollection<Evidence>(evidence.OrderByDescending(e => e.CollectedAt));
                _logger.LogInformation("Loaded {Count} evidence items", evidence.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading evidence");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Add()
        {
            SelectedEvidence = Evidence.Create(
                id: Guid.NewGuid().ToString(),
                name: "New Evidence",
                type: EvidenceType.Log,
                description: string.Empty,
                status: EvidenceStatus.New,
                classification: EvidenceClassification.Internal,
                source: "Manual",
                collectedAt: DateTimeOffset.UtcNow,
                sizeBytes: 0,
                hash: string.Empty,
                filePath: string.Empty,
                tags: new List<string>(),
                relatedIncidentIds: null,
                relatedThreatIds: null,
                relatedVulnerabilityIds: null,
                analysisNotes: null,
                collectedBy: null);
            IsEditMode = true;
        }

        private void Edit()
        {
            if (SelectedEvidence == null)
                return;
            IsEditMode = true;
        }

        private bool CanEditOrDelete() => SelectedEvidence != null;

        private async Task DeleteAsync()
        {
            if (SelectedEvidence == null)
                return;

            try
            {
                IsLoading = true;
                var result = await _evidenceService.DeleteAsync(SelectedEvidence.Id, "admin");
                if (result)
                {
                    await LoadAsync();
                    SelectedEvidence = null;
                    _logger.LogInformation("Deleted evidence {EvidenceId}", SelectedEvidence?.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting evidence");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedEvidence == null)
                return;

            try
            {
                IsLoading = true;
                var updated = await _evidenceService.UpdateAsync(
                    SelectedEvidence.Id,
                    SelectedEvidence.Name,
                    SelectedEvidence.Type,
                    SelectedEvidence.Description,
                    SelectedEvidence.Status,
                    SelectedEvidence.Classification,
                    SelectedEvidence.Source,
                    SelectedEvidence.FilePath,
                    SelectedEvidence.Tags,
                    SelectedEvidence.RelatedIncidentIds,
                    SelectedEvidence.RelatedThreatIds,
                    SelectedEvidence.RelatedVulnerabilityIds,
                    SelectedEvidence.AnalysisNotes,
                    "admin");

                await LoadAsync();
                SelectedEvidence = Evidence.FirstOrDefault(e => e.Id == updated.Id);
                IsEditMode = false;
                _logger.LogInformation("Updated evidence {EvidenceId}", updated.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving evidence");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            IsEditMode = false;
            SelectedEvidence = null;
        }

        private async Task FilterAsync()
        {
            try
            {
                IsLoading = true;

                var allEvidence = await _evidenceService.GetAllAsync();

                var filtered = allEvidence.AsEnumerable();

                if (SelectedTypeFilter != EvidenceType.Log)
                {
                    filtered = filtered.Where(e => e.Type == SelectedTypeFilter);
                }

                if (SelectedStatusFilter != EvidenceStatus.New)
                {
                    filtered = filtered.Where(e => e.Status == SelectedStatusFilter);
                }

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLowerInvariant();
                    filtered = filtered.Where(e =>
                        e.Name.ToLowerInvariant().Contains(searchLower) ||
                        e.Description.ToLowerInvariant().Contains(searchLower) ||
                        e.Source.ToLowerInvariant().Contains(searchLower));
                }

                Evidence = new ObservableCollection<Evidence>(filtered.OrderByDescending(e => e.CollectedAt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering evidence");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
