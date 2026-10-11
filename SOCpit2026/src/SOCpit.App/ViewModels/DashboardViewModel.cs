// SOCpit - Security Operations Center
// Dashboard view model for WPF application
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
    /// <summary>Dashboard view model.</summary>
    public sealed class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly ThreatService _threatService;
        private readonly VulnerabilityService _vulnerabilityService;
        private readonly IncidentService _incidentService;
        private readonly HostService _hostService;
        private readonly ILogger<DashboardViewModel> _logger;

        private int _totalThreats;
        private int _criticalThreats;
        private int _highThreats;
        private int _mediumThreats;
        private int _lowThreats;
        private int _totalVulnerabilities;
        private int _criticalVulnerabilities;
        private int _highVulnerabilities;
        private int _totalIncidents;
        private int _openIncidents;
        private int _totalHosts;
        private int _activeHosts;
        private ObservableCollection<Threat> _recentThreats = new();
        private ObservableCollection<Vulnerability> _recentVulnerabilities = new();
        private ObservableCollection<SecurityIncident> _recentIncidents = new();
        private bool _isLoading;

        public DashboardViewModel(
            ThreatService threatService,
            VulnerabilityService vulnerabilityService,
            IncidentService incidentService,
            HostService hostService,
            ILogger<DashboardViewModel> logger)
        {
            _threatService = threatService;
            _vulnerabilityService = vulnerabilityService;
            _incidentService = incidentService;
            _hostService = hostService;
            _logger = logger;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int TotalThreats
        {
            get => _totalThreats;
            set
            {
                if (_totalThreats != value)
                {
                    _totalThreats = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CriticalThreats
        {
            get => _criticalThreats;
            set
            {
                if (_criticalThreats != value)
                {
                    _criticalThreats = value;
                    OnPropertyChanged();
                }
            }
        }

        public int HighThreats
        {
            get => _highThreats;
            set
            {
                if (_highThreats != value)
                {
                    _highThreats = value;
                    OnPropertyChanged();
                }
            }
        }

        public int MediumThreats
        {
            get => _mediumThreats;
            set
            {
                if (_mediumThreats != value)
                {
                    _mediumThreats = value;
                    OnPropertyChanged();
                }
            }
        }

        public int LowThreats
        {
            get => _lowThreats;
            set
            {
                if (_lowThreats != value)
                {
                    _lowThreats = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalVulnerabilities
        {
            get => _totalVulnerabilities;
            set
            {
                if (_totalVulnerabilities != value)
                {
                    _totalVulnerabilities = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CriticalVulnerabilities
        {
            get => _criticalVulnerabilities;
            set
            {
                if (_criticalVulnerabilities != value)
                {
                    _criticalVulnerabilities = value;
                    OnPropertyChanged();
                }
            }
        }

        public int HighVulnerabilities
        {
            get => _highVulnerabilities;
            set
            {
                if (_highVulnerabilities != value)
                {
                    _highVulnerabilities = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalIncidents
        {
            get => _totalIncidents;
            set
            {
                if (_totalIncidents != value)
                {
                    _totalIncidents = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResolvedIncidents));
                }
            }
        }

        public int OpenIncidents
        {
            get => _openIncidents;
            set
            {
                if (_openIncidents != value)
                {
                    _openIncidents = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ResolvedIncidents));
                }
            }
        }

        public int ResolvedIncidents => Math.Max(0, TotalIncidents - OpenIncidents);

        public int TotalHosts
        {
            get => _totalHosts;
            set
            {
                if (_totalHosts != value)
                {
                    _totalHosts = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(InactiveHosts));
                }
            }
        }

        public int ActiveHosts
        {
            get => _activeHosts;
            set
            {
                if (_activeHosts != value)
                {
                    _activeHosts = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(InactiveHosts));
                }
            }
        }

        public int InactiveHosts => Math.Max(0, TotalHosts - ActiveHosts);

        public ObservableCollection<Threat> RecentThreats
        {
            get => _recentThreats;
            set
            {
                if (_recentThreats != value)
                {
                    _recentThreats = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Vulnerability> RecentVulnerabilities
        {
            get => _recentVulnerabilities;
            set
            {
                if (_recentVulnerabilities != value)
                {
                    _recentVulnerabilities = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<SecurityIncident> RecentIncidents
        {
            get => _recentIncidents;
            set
            {
                if (_recentIncidents != value)
                {
                    _recentIncidents = value;
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

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;

                // Load threats
                var threats = await _threatService.GetAllAsync();
                TotalThreats = threats.Count;
                CriticalThreats = threats.Count(t => t.Severity == ThreatSeverity.Critical);
                HighThreats = threats.Count(t => t.Severity == ThreatSeverity.High);
                MediumThreats = threats.Count(t => t.Severity == ThreatSeverity.Medium);
                LowThreats = threats.Count(t => t.Severity == ThreatSeverity.Low);
                RecentThreats = new ObservableCollection<Threat>(threats.OrderByDescending(t => t.DetectedAt).Take(5));

                // Load vulnerabilities
                var vulnerabilities = await _vulnerabilityService.GetAllAsync();
                TotalVulnerabilities = vulnerabilities.Count;
                CriticalVulnerabilities = vulnerabilities.Count(v => v.Severity == VulnerabilitySeverity.Critical);
                HighVulnerabilities = vulnerabilities.Count(v => v.Severity == VulnerabilitySeverity.High);
                RecentVulnerabilities = new ObservableCollection<Vulnerability>(vulnerabilities.OrderByDescending(v => v.PublishedAt).Take(5));

                // Load incidents
                var incidents = await _incidentService.GetAllAsync();
                TotalIncidents = incidents.Count;
                OpenIncidents = incidents.Count(i => i.Status != IncidentStatus.Closed && i.Status != IncidentStatus.Resolved);
                RecentIncidents = new ObservableCollection<SecurityIncident>(incidents.OrderByDescending(i => i.CreatedAt).Take(5));

                // Load hosts
                var hosts = await _hostService.GetAllAsync();
                TotalHosts = hosts.Count;
                ActiveHosts = hosts.Count(h => h.Status == AssetStatus.Active);

                _logger.LogInformation("Dashboard loaded: {Threats} threats, {Vulnerabilities} vulnerabilities, {Incidents} incidents, {Hosts} hosts",
                    TotalThreats, TotalVulnerabilities, TotalIncidents, TotalHosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
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

    /// <summary>Async relay command for MVVM.</summary>
    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute());
        }

        public async void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    CommandManager.InvalidateRequerySuggested();
                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
    }
}
