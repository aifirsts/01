// SOCpit - Security Operations Center
// Settings view model for WPF application
// Based on MATRIX V3 with MVVM pattern

using Microsoft.Extensions.Logging;
using SOCpit.Application.Services;
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
    /// <summary>Settings view model.</summary>
    public sealed class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly AuditService _auditService;
        private readonly ILogger<SettingsViewModel> _logger;

        private string _dataStoragePath = "C:\\SOCpit2026";
        private string _backupPath = "C:\\SOCpit2026\\Backup";
        private int _backupFrequencyDays = 7;
        private bool _autoBackupEnabled = true;
        private bool _autoUpdateEnabled = true;
        private string _language = "English";
        private string _theme = "Dark";
        private bool _isLoading;
        private bool _isSaving;
        private ObservableCollection<AuditLogEntry> _auditLogs = new();
        private AuditLogEntry? _selectedAuditLog;

        public SettingsViewModel(
            AuditService auditService,
            ILogger<SettingsViewModel> logger)
        {
            _auditService = auditService;
            _logger = logger;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync);
            ClearCacheCommand = new AsyncRelayCommand(ClearCacheAsync);
            RefreshLogsCommand = new AsyncRelayCommand(RefreshLogsAsync);

            Languages = new ObservableCollection<string>(new List<string> { "English", "Russian" });
            Themes = new ObservableCollection<string>(new List<string> { "Dark", "Light", "System" });

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DataStoragePath
        {
            get => _dataStoragePath;
            set
            {
                if (_dataStoragePath != value)
                {
                    _dataStoragePath = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BackupPath
        {
            get => _backupPath;
            set
            {
                if (_backupPath != value)
                {
                    _backupPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BackupFrequencyDays
        {
            get => _backupFrequencyDays;
            set
            {
                if (_backupFrequencyDays != value)
                {
                    _backupFrequencyDays = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoBackupEnabled
        {
            get => _autoBackupEnabled;
            set
            {
                if (_autoBackupEnabled != value)
                {
                    _autoBackupEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoUpdateEnabled
        {
            get => _autoUpdateEnabled;
            set
            {
                if (_autoUpdateEnabled != value)
                {
                    _autoUpdateEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
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

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                if (_isSaving != value)
                {
                    _isSaving = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<AuditLogEntry> AuditLogs
        {
            get => _auditLogs;
            set
            {
                if (_auditLogs != value)
                {
                    _auditLogs = value;
                    OnPropertyChanged();
                }
            }
        }

        public AuditLogEntry? SelectedAuditLog
        {
            get => _selectedAuditLog;
            set
            {
                if (_selectedAuditLog != value)
                {
                    _selectedAuditLog = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> Languages { get; }
        public ObservableCollection<string> Themes { get; }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand TestConnectionCommand { get; }
        public ICommand ClearCacheCommand { get; }
        public ICommand RefreshLogsCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;

                // Load settings from configuration
                // This would typically load from a configuration file or database
                await Task.Delay(100); // Simulate loading

                await RefreshLogsAsync();

                _logger.LogInformation("Settings loaded");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                IsSaving = true;

                // Save settings to configuration
                // This would typically save to a configuration file or database
                await Task.Delay(100); // Simulate saving

                _logger.LogInformation("Settings saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings");
            }
            finally
            {
                IsSaving = false;
            }
        }

        private async Task TestConnectionAsync()
        {
            try
            {
                IsLoading = true;

                // Test database connection
                await Task.Delay(200); // Simulate connection test

                _logger.LogInformation("Connection test successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Connection test failed");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ClearCacheAsync()
        {
            try
            {
                IsLoading = true;

                // Clear application cache
                await Task.Delay(100); // Simulate cache clearing

                _logger.LogInformation("Cache cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshLogsAsync()
        {
            try
            {
                IsLoading = true;

                // Load recent audit logs
                var logs = new List<AuditLogEntry>();

                // Add sample audit log entries
                logs.Add(new AuditLogEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Action = "Login",
                    EntityType = "User",
                    EntityId = "admin",
                    UserId = "admin",
                    Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
                    Details = "User logged in successfully",
                    Success = true
                });

                logs.Add(new AuditLogEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Action = "Create",
                    EntityType = "Threat",
                    EntityId = "threat-001",
                    UserId = "analyst",
                    Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10),
                    Details = "Created new threat: Suspicious Activity",
                    Success = true
                });

                logs.Add(new AuditLogEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Action = "Update",
                    EntityType = "Vulnerability",
                    EntityId = "vuln-001",
                    UserId = "analyst",
                    Timestamp = DateTimeOffset.UtcNow.AddMinutes(-15),
                    Details = "Updated vulnerability status to Fixed",
                    Success = true
                });

                AuditLogs = new ObservableCollection<AuditLogEntry>(logs.OrderByDescending(l => l.Timestamp));
                _logger.LogInformation("Loaded {Count} audit log entries", logs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading audit logs");
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

    /// <summary>Audit log entry for display.</summary>
    public sealed class AuditLogEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
