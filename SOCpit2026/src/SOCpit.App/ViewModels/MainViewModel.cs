// SOCpit - Security Operations Center
// Main view model for WPF application
// Based on MATRIX V3 with MVVM pattern

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SOCpit.App.Views;
using SOCpit.Application.Services;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SOCpit.App.ViewModels
{
    /// <summary>Main view model for the application.</summary>
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly ThreatService _threatService;
        private readonly VulnerabilityService _vulnerabilityService;
        private readonly IncidentService _incidentService;
        private readonly EvidenceService _evidenceService;
        private readonly HostService _hostService;
        private readonly NetworkService _networkService;
        private readonly UserService _userService;
        private readonly AuditService _auditService;
        private readonly ILogger<MainViewModel> _logger;

        private object? _currentView;
        private string _currentViewName = "Dashboard";

        public MainViewModel(
            ThreatService threatService,
            VulnerabilityService vulnerabilityService,
            IncidentService incidentService,
            EvidenceService evidenceService,
            HostService hostService,
            NetworkService networkService,
            UserService userService,
            AuditService auditService,
            ILogger<MainViewModel> logger)
        {
            _threatService = threatService;
            _vulnerabilityService = vulnerabilityService;
            _incidentService = incidentService;
            _evidenceService = evidenceService;
            _hostService = hostService;
            _networkService = networkService;
            _userService = userService;
            _auditService = auditService;
            _logger = logger;

            NavigateCommand = new RelayCommand<string>(Navigate);
            LoadedCommand = new RelayCommand(OnLoaded);

            // Initialize with Dashboard view
            Navigate("Dashboard");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentViewName
        {
            get => _currentViewName;
            set
            {
                if (_currentViewName != value)
                {
                    _currentViewName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NavigateCommand { get; }
        public ICommand LoadedCommand { get; }

        private void Navigate(string viewName)
        {
            CurrentViewName = viewName;

            CurrentView = viewName switch
            {
                "Dashboard" => new DashboardView(new DashboardViewModel(_threatService, _vulnerabilityService, _incidentService, _hostService, NullLogger<DashboardViewModel>.Instance)),
                "Threats" => new ThreatsView(new ThreatsViewModel(_threatService, _hostService, _evidenceService, NullLogger<ThreatsViewModel>.Instance)),
                "Vulnerabilities" => new VulnerabilitiesView(new VulnerabilitiesViewModel(_vulnerabilityService, _hostService, NullLogger<VulnerabilitiesViewModel>.Instance)),
                "Incidents" => new IncidentsView(new IncidentsViewModel(_incidentService, _threatService, _vulnerabilityService, NullLogger<IncidentsViewModel>.Instance)),
                "Evidence" => new EvidenceView(new EvidenceViewModel(_evidenceService, _incidentService, _threatService, NullLogger<EvidenceViewModel>.Instance)),
                "Hosts" => new HostsView(new HostsViewModel(_hostService, _networkService, NullLogger<HostsViewModel>.Instance)),
                "Networks" => new NetworksView(new NetworksViewModel(_networkService, _hostService, NullLogger<NetworksViewModel>.Instance)),
                "Users" => new UsersView(new UsersViewModel(_userService, NullLogger<UsersViewModel>.Instance)),
                "Reports" => new ReportsView(new ReportsViewModel(_threatService, _vulnerabilityService, _incidentService, _evidenceService, _hostService, NullLogger<ReportsViewModel>.Instance)),
                "Settings" => new SettingsView(new SettingsViewModel(_auditService, NullLogger<SettingsViewModel>.Instance)),
                _ => new DashboardView(new DashboardViewModel(_threatService, _vulnerabilityService, _incidentService, _hostService, NullLogger<DashboardViewModel>.Instance))
            };

            _logger.LogInformation("Navigated to {ViewName}", viewName);
        }

        private void OnLoaded()
        {
            _logger.LogInformation("Application loaded");
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>Relay command for MVVM.</summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
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
            return _canExecute == null || _canExecute();
        }

        public void Execute(object? parameter)
        {
            _execute();
        }
    }

    /// <summary>Relay command with parameter for MVVM.</summary>
    /// <typeparam name="T">The parameter type.</typeparam>
    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
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
            return parameter is T t && (_canExecute == null || _canExecute(t));
        }

        public void Execute(object? parameter)
        {
            if (parameter is T t)
                _execute(t);
        }
    }
}
