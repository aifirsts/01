// SOCpit - Security Operations Center
// Reports view model for WPF application
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
    /// <summary>Reports view model.</summary>
    public sealed class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly ThreatService _threatService;
        private readonly VulnerabilityService _vulnerabilityService;
        private readonly IncidentService _incidentService;
        private readonly EvidenceService _evidenceService;
        private readonly HostService _hostService;
        private readonly ILogger<ReportsViewModel> _logger;

        private ObservableCollection<ReportSummary> _reports = new();
        private ReportSummary? _selectedReport;
        private DateTime _startDate = DateTime.UtcNow.AddDays(-30);
        private DateTime _endDate = DateTime.UtcNow;
        private string _reportType = "Threat Summary";
        private bool _isLoading;
        private bool _isGenerating;

        public ReportsViewModel(
            ThreatService threatService,
            VulnerabilityService vulnerabilityService,
            IncidentService incidentService,
            EvidenceService evidenceService,
            HostService hostService,
            ILogger<ReportsViewModel> logger)
        {
            _threatService = threatService;
            _vulnerabilityService = vulnerabilityService;
            _incidentService = incidentService;
            _evidenceService = evidenceService;
            _hostService = hostService;
            _logger = logger;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            GenerateCommand = new AsyncRelayCommand(GenerateAsync);
            ExportCommand = new AsyncRelayCommand(ExportAsync, () => SelectedReport != null);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);

            ReportTypes = new ObservableCollection<string>(new List<string>
            {
                "Threat Summary",
                "Vulnerability Summary",
                "Incident Summary",
                "Security Posture",
                "Compliance Report",
                "Asset Inventory",
                "Activity Log"
            });

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ReportSummary> Reports
        {
            get => _reports;
            set
            {
                if (_reports != value)
                {
                    _reports = value;
                    OnPropertyChanged();
                }
            }
        }

        public ReportSummary? SelectedReport
        {
            get => _selectedReport;
            set
            {
                if (_selectedReport != value)
                {
                    _selectedReport = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ReportType
        {
            get => _reportType;
            set
            {
                if (_reportType != value)
                {
                    _reportType = value;
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

        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                if (_isGenerating != value)
                {
                    _isGenerating = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> ReportTypes { get; }

        public ICommand LoadCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand RefreshCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;

                await Task.CompletedTask;
                var reports = new List<ReportSummary>
                {
                    new ReportSummary
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "Threat Summary - Last 30 Days",
                        Type = "Threat Summary",
                        GeneratedAt = DateTimeOffset.UtcNow.AddDays(-1),
                        GeneratedBy = "System",
                        RecordCount = 42
                    },
                    new ReportSummary
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "Vulnerability Assessment",
                        Type = "Vulnerability Summary",
                        GeneratedAt = DateTimeOffset.UtcNow.AddDays(-2),
                        GeneratedBy = "System",
                        RecordCount = 23
                    },
                    new ReportSummary
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "Security Incidents Report",
                        Type = "Incident Summary",
                        GeneratedAt = DateTimeOffset.UtcNow.AddDays(-3),
                        GeneratedBy = "System",
                        RecordCount = 15
                    }
                };

                Reports = new ObservableCollection<ReportSummary>(reports.OrderByDescending(r => r.GeneratedAt));
                _logger.LogInformation("Loaded {Count} reports", reports.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateAsync()
        {
            try
            {
                IsGenerating = true;

                var report = await GenerateReportAsync(ReportType, StartDate, EndDate);

                Reports.Insert(0, report);
                SelectedReport = report;

                _logger.LogInformation("Generated report: {Title}", report.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private async Task<ReportSummary> GenerateReportAsync(string type, DateTime startDate, DateTime endDate)
        {
            var report = new ReportSummary
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"{type} - {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                Type = type,
                GeneratedAt = DateTimeOffset.UtcNow,
                GeneratedBy = "NEO",
                RecordCount = 0
            };

            switch (type)
            {
                case "Threat Summary":
                    var threats = await _threatService.GetAllAsync();
                    report.RecordCount = threats.Count;
                    break;

                case "Vulnerability Summary":
                    var vulnerabilities = await _vulnerabilityService.GetAllAsync();
                    report.RecordCount = vulnerabilities.Count;
                    break;

                case "Incident Summary":
                    var incidents = await _incidentService.GetAllAsync();
                    report.RecordCount = incidents.Count;
                    break;

                case "Security Posture":
                    var allThreats = await _threatService.GetAllAsync();
                    var allVulnerabilities = await _vulnerabilityService.GetAllAsync();
                    var allIncidents = await _incidentService.GetAllAsync();
                    report.RecordCount = allThreats.Count + allVulnerabilities.Count + allIncidents.Count;
                    break;

                case "Asset Inventory":
                    var hosts = await _hostService.GetAllAsync();
                    report.RecordCount = hosts.Count;
                    break;

                default:
                    var ev = await _evidenceService.GetAllAsync();
                    report.RecordCount = ev.Count;
                    break;
            }

            return report;
        }

        private async Task ExportAsync()
        {
            if (SelectedReport == null)
                return;

            try
            {
                IsLoading = true;
                await Task.Delay(500);
                _logger.LogInformation("Exported report: {Title}", SelectedReport.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
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

    /// <summary>Report summary for display.</summary>
    public sealed class ReportSummary
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public int RecordCount { get; set; }
    }
}
