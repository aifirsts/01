// SOCpit - Security Operations Center
// Incidents view model for WPF application
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
    /// <summary>Incidents view model.</summary>
    public sealed class IncidentsViewModel : INotifyPropertyChanged
    {
        private readonly IncidentService _incidentService;
        private readonly ThreatService _threatService;
        private readonly VulnerabilityService _vulnerabilityService;
        private readonly ILogger<IncidentsViewModel> _logger;

        private ObservableCollection<SecurityIncident> _incidents = new();
        private SecurityIncident? _selectedIncident;
        private IncidentSeverity _selectedSeverityFilter = IncidentSeverity.Critical;
        private IncidentStatus _selectedStatusFilter = IncidentStatus.New;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;

        public IncidentsViewModel(
            IncidentService incidentService,
            ThreatService threatService,
            VulnerabilityService vulnerabilityService,
            ILogger<IncidentsViewModel> logger)
        {
            _incidentService = incidentService;
            _threatService = threatService;
            _vulnerabilityService = vulnerabilityService;
            _logger = logger;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEditOrDelete);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, CanEditOrDelete);
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => _isEditMode);
            CancelCommand = new RelayCommand(Cancel, () => _isEditMode);
            FilterCommand = new AsyncRelayCommand(FilterAsync);
            ResolveCommand = new AsyncRelayCommand(ResolveAsync, CanEditOrDelete);

            SeverityFilters = new ObservableCollection<IncidentSeverity>(Enum.GetValues(typeof(IncidentSeverity)).Cast<IncidentSeverity>());
            StatusFilters = new ObservableCollection<IncidentStatus>(Enum.GetValues(typeof(IncidentStatus)).Cast<IncidentStatus>());

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SecurityIncident> Incidents
        {
            get => _incidents;
            set
            {
                if (_incidents != value)
                {
                    _incidents = value;
                    OnPropertyChanged();
                }
            }
        }

        public SecurityIncident? SelectedIncident
        {
            get => _selectedIncident;
            set
            {
                if (_selectedIncident != value)
                {
                    _selectedIncident = value;
                    OnPropertyChanged();
                }
            }
        }

        public IncidentSeverity SelectedSeverityFilter
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

        public IncidentStatus SelectedStatusFilter
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

        public ObservableCollection<IncidentSeverity> SeverityFilters { get; }
        public ObservableCollection<IncidentStatus> StatusFilters { get; }

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ResolveCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var incidents = await _incidentService.GetAllAsync();
                Incidents = new ObservableCollection<SecurityIncident>(incidents.OrderByDescending(i => i.CreatedAt));
                _logger.LogInformation("Loaded {Count} incidents", incidents.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading incidents");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Add()
        {
            SelectedIncident = SecurityIncident.Create(
                id: Guid.NewGuid().ToString(),
                title: "New Incident",
                description: string.Empty,
                severity: IncidentSeverity.Medium,
                status: IncidentStatus.New,
                createdAt: DateTimeOffset.UtcNow,
                assignedTo: null,
                relatedThreatIds: new List<string>(),
                relatedVulnerabilityIds: new List<string>(),
                impactAnalysis: null,
                responseActions: null,
                lessonsLearned: null);
            IsEditMode = true;
        }

        private void Edit()
        {
            if (SelectedIncident == null)
                return;
            IsEditMode = true;
        }

        private bool CanEditOrDelete() => SelectedIncident != null;

        private async Task DeleteAsync()
        {
            if (SelectedIncident == null)
                return;

            try
            {
                IsLoading = true;
                var result = await _incidentService.DeleteAsync(SelectedIncident.Id, "admin");
                if (result)
                {
                    await LoadAsync();
                    SelectedIncident = null;
                    _logger.LogInformation("Deleted incident {IncidentId}", SelectedIncident?.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting incident");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedIncident == null)
                return;

            try
            {
                IsLoading = true;
                var updated = await _incidentService.UpdateAsync(
                    SelectedIncident.Id,
                    SelectedIncident.Title,
                    SelectedIncident.Description,
                    SelectedIncident.Severity,
                    SelectedIncident.Status,
                    SelectedIncident.AssignedTo,
                    SelectedIncident.RelatedThreatIds,
                    SelectedIncident.RelatedVulnerabilityIds,
                    SelectedIncident.ImpactAnalysis,
                    SelectedIncident.ResponseActions,
                    SelectedIncident.LessonsLearned,
                    "admin");

                await LoadAsync();
                SelectedIncident = Incidents.FirstOrDefault(i => i.Id == updated.Id);
                IsEditMode = false;
                _logger.LogInformation("Updated incident {IncidentId}", updated.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving incident");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            IsEditMode = false;
            SelectedIncident = null;
        }

        private async Task ResolveAsync()
        {
            if (SelectedIncident == null)
                return;

            try
            {
                IsLoading = true;
                var resolved = await _incidentService.ResolveAsync(
                    SelectedIncident.Id,
                    SelectedIncident.LessonsLearned,
                    "admin");

                await LoadAsync();
                SelectedIncident = Incidents.FirstOrDefault(i => i.Id == resolved.Id);
                _logger.LogInformation("Resolved incident {IncidentId}", resolved.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving incident");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FilterAsync()
        {
            try
            {
                IsLoading = true;

                var allIncidents = await _incidentService.GetAllAsync();

                var filtered = allIncidents.AsEnumerable();

                if (SelectedSeverityFilter != IncidentSeverity.Critical)
                {
                    filtered = filtered.Where(i => i.Severity == SelectedSeverityFilter);
                }

                if (SelectedStatusFilter != IncidentStatus.New)
                {
                    filtered = filtered.Where(i => i.Status == SelectedStatusFilter);
                }

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLowerInvariant();
                    filtered = filtered.Where(i =>
                        i.Title.ToLowerInvariant().Contains(searchLower) ||
                        i.Description.ToLowerInvariant().Contains(searchLower));
                }

                Incidents = new ObservableCollection<SecurityIncident>(filtered.OrderByDescending(i => i.CreatedAt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering incidents");
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
