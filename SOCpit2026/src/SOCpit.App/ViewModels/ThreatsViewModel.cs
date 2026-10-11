// SOCpit - Security Operations Center
// Threats view model for WPF application
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
    /// <summary>Threats view model.</summary>
    public sealed class ThreatsViewModel : INotifyPropertyChanged
    {
        private readonly ThreatService _threatService;
        private readonly HostService _hostService;
        private readonly EvidenceService _evidenceService;
        private readonly ILogger<ThreatsViewModel> _logger;

        private ObservableCollection<Threat> _threats = new();
        private Threat? _selectedThreat;
        private ThreatSeverity _selectedSeverityFilter = ThreatSeverity.Critical;
        private ThreatStatus _selectedStatusFilter = ThreatStatus.New;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;

        public ThreatsViewModel(
            ThreatService threatService,
            HostService hostService,
            EvidenceService evidenceService,
            ILogger<ThreatsViewModel> logger)
        {
            _threatService = threatService;
            _hostService = hostService;
            _evidenceService = evidenceService;
            _logger = logger;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEditOrDelete);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, CanEditOrDelete);
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => _isEditMode);
            CancelCommand = new RelayCommand(Cancel, () => _isEditMode);
            FilterCommand = new AsyncRelayCommand(FilterAsync);

            SeverityFilters = new ObservableCollection<ThreatSeverity>(Enum.GetValues(typeof(ThreatSeverity)).Cast<ThreatSeverity>());
            StatusFilters = new ObservableCollection<ThreatStatus>(Enum.GetValues(typeof(ThreatStatus)).Cast<ThreatStatus>());

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Threat> Threats
        {
            get => _threats;
            set
            {
                if (_threats != value)
                {
                    _threats = value;
                    OnPropertyChanged();
                }
            }
        }

        public Threat? SelectedThreat
        {
            get => _selectedThreat;
            set
            {
                if (_selectedThreat != value)
                {
                    _selectedThreat = value;
                    OnPropertyChanged();
                }
            }
        }

        public ThreatSeverity SelectedSeverityFilter
        {
            get => _selectedSeverityFilter;
            set
            {
                if (_selectedSeverityFilter != value)
                {
                    _selectedSeverityFilter = value;
                    OnPropertyChanged();
                }
            }
        }

        public ThreatStatus SelectedStatusFilter
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

        public ObservableCollection<ThreatSeverity> SeverityFilters { get; }
        public ObservableCollection<ThreatStatus> StatusFilters { get; }

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
                var threats = await _threatService.GetAllAsync();
                Threats = new ObservableCollection<Threat>(threats.OrderByDescending(t => t.DetectedAt));
                _logger.LogInformation("Loaded {Count} threats", threats.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading threats");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Add()
        {
            SelectedThreat = Threat.Create(
                id: Guid.NewGuid().ToString(),
                name: "New Threat",
                description: string.Empty,
                severity: ThreatSeverity.Medium,
                status: ThreatStatus.New,
                targetId: "host-neo-pc",
                detectedAt: DateTimeOffset.UtcNow,
                source: "Manual",
                tags: new List<string>(),
                mitigationSteps: null,
                relatedEvidenceIds: null);
            IsEditMode = true;
        }

        private void Edit()
        {
            if (SelectedThreat == null)
                return;
            IsEditMode = true;
        }

        private bool CanEditOrDelete() => SelectedThreat != null;

        private async Task DeleteAsync()
        {
            if (SelectedThreat == null)
                return;

            try
            {
                IsLoading = true;
                var result = await _threatService.DeleteAsync(SelectedThreat.Id, "admin");
                if (result)
                {
                    await LoadAsync();
                    SelectedThreat = null;
                    _logger.LogInformation("Deleted threat {ThreatId}", SelectedThreat?.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting threat");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedThreat == null)
                return;

            try
            {
                IsLoading = true;
                var updated = await _threatService.UpdateAsync(
                    SelectedThreat.Id,
                    SelectedThreat.Name,
                    SelectedThreat.Description,
                    SelectedThreat.Severity,
                    SelectedThreat.Status,
                    SelectedThreat.TargetId,
                    SelectedThreat.Source,
                    SelectedThreat.Tags,
                    SelectedThreat.MitigationSteps,
                    SelectedThreat.RelatedEvidenceIds,
                    "admin");

                await LoadAsync();
                SelectedThreat = Threats.FirstOrDefault(t => t.Id == updated.Id);
                IsEditMode = false;
                _logger.LogInformation("Updated threat {ThreatId}", updated.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving threat");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            IsEditMode = false;
            SelectedThreat = null;
        }

        private async Task FilterAsync()
        {
            try
            {
                IsLoading = true;

                var allThreats = await _threatService.GetAllAsync();

                var filtered = allThreats.AsEnumerable();

                if (SelectedSeverityFilter != ThreatSeverity.Critical)
                {
                    filtered = filtered.Where(t => t.Severity == SelectedSeverityFilter);
                }

                if (SelectedStatusFilter != ThreatStatus.New)
                {
                    filtered = filtered.Where(t => t.Status == SelectedStatusFilter);
                }

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLowerInvariant();
                    filtered = filtered.Where(t =>
                        t.Name.ToLowerInvariant().Contains(searchLower) ||
                        t.Description.ToLowerInvariant().Contains(searchLower) ||
                        t.Source.ToLowerInvariant().Contains(searchLower));
                }

                Threats = new ObservableCollection<Threat>(filtered.OrderByDescending(t => t.DetectedAt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering threats");
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
