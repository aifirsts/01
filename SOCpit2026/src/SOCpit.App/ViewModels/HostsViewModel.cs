// SOCpit - Security Operations Center
// Hosts view model for WPF application
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
    /// <summary>Hosts view model.</summary>
    public sealed class HostsViewModel : INotifyPropertyChanged
    {
        private readonly HostService _hostService;
        private readonly NetworkService _networkService;
        private readonly ILogger<HostsViewModel> _logger;

        private ObservableCollection<Host> _hosts = new();
        private Host? _selectedHost;
        private AssetType _selectedTypeFilter = AssetType.Server;
        private AssetStatus _selectedStatusFilter = AssetStatus.Active;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;

        public HostsViewModel(
            HostService hostService,
            NetworkService networkService,
            ILogger<HostsViewModel> logger)
        {
            _hostService = hostService;
            _networkService = networkService;
            _logger = logger;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEditOrDelete);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, CanEditOrDelete);
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => _isEditMode);
            CancelCommand = new RelayCommand(Cancel, () => _isEditMode);
            FilterCommand = new AsyncRelayCommand(FilterAsync);

            TypeFilters = new ObservableCollection<AssetType>(Enum.GetValues(typeof(AssetType)).Cast<AssetType>());
            StatusFilters = new ObservableCollection<AssetStatus>(Enum.GetValues(typeof(AssetStatus)).Cast<AssetStatus>());

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Host> Hosts
        {
            get => _hosts;
            set
            {
                if (_hosts != value)
                {
                    _hosts = value;
                    OnPropertyChanged();
                }
            }
        }

        public Host? SelectedHost
        {
            get => _selectedHost;
            set
            {
                if (_selectedHost != value)
                {
                    _selectedHost = value;
                    OnPropertyChanged();
                }
            }
        }

        public AssetType SelectedTypeFilter
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

        public AssetStatus SelectedStatusFilter
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

        public ObservableCollection<AssetType> TypeFilters { get; }
        public ObservableCollection<AssetStatus> StatusFilters { get; }

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
                var hosts = await _hostService.GetAllAsync();
                Hosts = new ObservableCollection<Host>(hosts.OrderByDescending(h => h.CreatedAt));
                _logger.LogInformation("Loaded {Count} hosts", hosts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading hosts");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Add()
        {
            SelectedHost = Host.Create(
                id: Guid.NewGuid().ToString(),
                name: "New Host",
                ipAddress: "192.168.1.1",
                macAddress: "00:00:00:00:00:00",
                type: AssetType.Server,
                status: AssetStatus.Active,
                operatingSystem: OperatingSystemType.Windows,
                createdAt: DateTimeOffset.UtcNow,
                tags: new List<string>(),
                osVersion: null,
                hostname: null,
                domain: null,
                location: null,
                owner: null);
            IsEditMode = true;
        }

        private void Edit()
        {
            if (SelectedHost == null)
                return;
            IsEditMode = true;
        }

        private bool CanEditOrDelete() => SelectedHost != null;

        private async Task DeleteAsync()
        {
            if (SelectedHost == null)
                return;

            try
            {
                IsLoading = true;
                var result = await _hostService.DeleteAsync(SelectedHost.Id, "admin");
                if (result)
                {
                    await LoadAsync();
                    SelectedHost = null;
                    _logger.LogInformation("Deleted host {HostId}", SelectedHost?.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting host");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedHost == null)
                return;

            try
            {
                IsLoading = true;
                var updated = await _hostService.UpdateAsync(
                    SelectedHost.Id,
                    SelectedHost.Name,
                    SelectedHost.IpAddress,
                    SelectedHost.MacAddress,
                    SelectedHost.Type,
                    SelectedHost.Status,
                    SelectedHost.OperatingSystem,
                    SelectedHost.OsVersion,
                    SelectedHost.Hostname,
                    SelectedHost.Domain,
                    SelectedHost.Location,
                    SelectedHost.Owner,
                    SelectedHost.Tags,
                    "admin");

                await LoadAsync();
                SelectedHost = Hosts.FirstOrDefault(h => h.Id == updated.Id);
                IsEditMode = false;
                _logger.LogInformation("Updated host {HostId}", updated.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving host");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            IsEditMode = false;
            SelectedHost = null;
        }

        private async Task FilterAsync()
        {
            try
            {
                IsLoading = true;

                var allHosts = await _hostService.GetAllAsync();

                var filtered = allHosts.AsEnumerable();

                if (SelectedTypeFilter != AssetType.Server)
                {
                    filtered = filtered.Where(h => h.Type == SelectedTypeFilter);
                }

                if (SelectedStatusFilter != AssetStatus.Active)
                {
                    filtered = filtered.Where(h => h.Status == SelectedStatusFilter);
                }

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLowerInvariant();
                    filtered = filtered.Where(h =>
                        h.Name.ToLowerInvariant().Contains(searchLower) ||
                        h.IpAddress.ToLowerInvariant().Contains(searchLower) ||
                        (h.Hostname?.ToLowerInvariant().Contains(searchLower) ?? false));
                }

                Hosts = new ObservableCollection<Host>(filtered.OrderByDescending(h => h.CreatedAt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering hosts");
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
