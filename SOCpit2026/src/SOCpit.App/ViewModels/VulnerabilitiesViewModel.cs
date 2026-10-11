// SOCpit - Security Operations Center
// Vulnerabilities view model for WPF application
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
    /// <summary>Vulnerabilities view model.</summary>
    public sealed class VulnerabilitiesViewModel : INotifyPropertyChanged
    {
        private readonly VulnerabilityService _vulnerabilityService;
        private readonly HostService _hostService;
        private readonly ILogger<VulnerabilitiesViewModel> _logger;

        private ObservableCollection<Vulnerability> _vulnerabilities = new();
        private Vulnerability? _selectedVulnerability;
        private VulnerabilitySeverity _selectedSeverityFilter = VulnerabilitySeverity.Critical;
        private VulnerabilityStatus _selectedStatusFilter = VulnerabilityStatus.Open;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;

        public VulnerabilitiesViewModel(
            VulnerabilityService vulnerabilityService,
            HostService hostService,
            ILogger<VulnerabilitiesViewModel> logger)
        {
            _vulnerabilityService = vulnerabilityService;
            _hostService = hostService;
            _logger = logger;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEditOrDelete);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, CanEditOrDelete);
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => _isEditMode);
            CancelCommand = new RelayCommand(Cancel, () => _isEditMode);
            FilterCommand = new AsyncRelayCommand(FilterAsync);

            SeverityFilters = new ObservableCollection<VulnerabilitySeverity>(Enum.GetValues(typeof(VulnerabilitySeverity)).Cast<VulnerabilitySeverity>());
            StatusFilters = new ObservableCollection<VulnerabilityStatus>(Enum.GetValues(typeof(VulnerabilityStatus)).Cast<VulnerabilityStatus>());

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Vulnerability> Vulnerabilities
        {
            get => _vulnerabilities;
            set
            {
                if (_vulnerabilities != value)
                {
                    _vulnerabilities = value;
                    OnPropertyChanged();
                }
            }
        }

        public Vulnerability? SelectedVulnerability
        {
            get => _selectedVulnerability;
            set
            {
                if (_selectedVulnerability != value)
                {
                    _selectedVulnerability = value;
                    OnPropertyChanged();
                }
            }
        }

        public VulnerabilitySeverity SelectedSeverityFilter
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

        public VulnerabilityStatus SelectedStatusFilter
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

        public ObservableCollection<VulnerabilitySeverity> SeverityFilters { get; }
        public ObservableCollection<VulnerabilityStatus> StatusFilters { get; }

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
                var vulnerabilities = await _vulnerabilityService.GetAllAsync();
                Vulnerabilities = new ObservableCollection<Vulnerability>(vulnerabilities.OrderByDescending(v => v.PublishedAt));
                _logger.LogInformation("Loaded {Count} vulnerabilities", vulnerabilities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vulnerabilities");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Add()
        {
            SelectedVulnerability = Vulnerability.Create(
                id: Guid.NewGuid().ToString(),
                cveId: "CVE-New",
                name: "New Vulnerability",
                description: string.Empty,
                severity: VulnerabilitySeverity.Medium,
                status: VulnerabilityStatus.Open,
                targetId: "host-neo-pc",
                publishedAt: DateTimeOffset.UtcNow,
                fixAvailable: null,
                cvssScore: null,
                references: new List<string>());
            IsEditMode = true;
        }

        private void Edit()
        {
            if (SelectedVulnerability == null)
                return;
            IsEditMode = true;
        }

        private bool CanEditOrDelete() => SelectedVulnerability != null;

        private async Task DeleteAsync()
        {
            if (SelectedVulnerability == null)
                return;

            try
            {
                IsLoading = true;
                var result = await _vulnerabilityService.DeleteAsync(SelectedVulnerability.Id, "admin");
                if (result)
                {
                    await LoadAsync();
                    SelectedVulnerability = null;
                    _logger.LogInformation("Deleted vulnerability {VulnerabilityId}", SelectedVulnerability?.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vulnerability");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedVulnerability == null)
                return;

            try
            {
                IsLoading = true;
                var updated = await _vulnerabilityService.UpdateAsync(
                    SelectedVulnerability.Id,
                    SelectedVulnerability.CveId,
                    SelectedVulnerability.Name,
                    SelectedVulnerability.Description,
                    SelectedVulnerability.Severity,
                    SelectedVulnerability.Status,
                    SelectedVulnerability.TargetId,
                    SelectedVulnerability.FixAvailable,
                    SelectedVulnerability.CvssScore,
                    SelectedVulnerability.References,
                    "admin");

                await LoadAsync();
                SelectedVulnerability = Vulnerabilities.FirstOrDefault(v => v.Id == updated.Id);
                IsEditMode = false;
                _logger.LogInformation("Updated vulnerability {VulnerabilityId}", updated.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving vulnerability");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            IsEditMode = false;
            SelectedVulnerability = null;
        }

        private async Task FilterAsync()
        {
            try
            {
                IsLoading = true;

                var allVulnerabilities = await _vulnerabilityService.GetAllAsync();

                var filtered = allVulnerabilities.AsEnumerable();

                if (SelectedSeverityFilter != VulnerabilitySeverity.Critical)
                {
                    filtered = filtered.Where(v => v.Severity == SelectedSeverityFilter);
                }

                if (SelectedStatusFilter != VulnerabilityStatus.Open)
                {
                    filtered = filtered.Where(v => v.Status == SelectedStatusFilter);
                }

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLowerInvariant();
                    filtered = filtered.Where(v =>
                        v.Name.ToLowerInvariant().Contains(searchLower) ||
                        v.Description.ToLowerInvariant().Contains(searchLower) ||
                        v.CveId.ToLowerInvariant().Contains(searchLower));
                }

                Vulnerabilities = new ObservableCollection<Vulnerability>(filtered.OrderByDescending(v => v.PublishedAt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering vulnerabilities");
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
