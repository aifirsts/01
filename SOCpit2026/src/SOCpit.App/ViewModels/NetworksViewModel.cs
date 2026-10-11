// SOCpit - Security Operations Center
// Networks view model for WPF application
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
    /// <summary>Networks view model.</summary>
    public sealed class NetworksViewModel : INotifyPropertyChanged
    {
        private readonly NetworkService _networkService;
        private readonly HostService _hostService;
        private readonly ILogger<NetworksViewModel> _logger;

        private ObservableCollection<Network> _networks = new();
        private Network? _selectedNetwork;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;

        public NetworksViewModel(
            NetworkService networkService,
            HostService hostService,
            ILogger<NetworksViewModel> logger)
        {
            _networkService = networkService;
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

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Network> Networks
        {
            get => _networks;
            set
            {
                if (_networks != value)
                {
                    _networks = value;
                    OnPropertyChanged();
                }
            }
        }

        public Network? SelectedNetwork
        {
            get => _selectedNetwork;
            set
            {
                if (_selectedNetwork != value)
                {
                    _selectedNetwork = value;
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
                var networks = await _networkService.GetAllAsync();
                Networks = new ObservableCollection<Network>(networks.OrderBy(n => n.Name));
                _logger.LogInformation("Loaded {Count} networks", networks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading networks");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Add()
        {
            SelectedNetwork = Network.Create(
                id: Guid.NewGuid().ToString(),
                name: "New Network",
                cidr: "192.168.1.0/24",
                description: string.Empty,
                type: "Internal",
                tags: new List<string>(),
                vlanIds: new List<string>(),
                subnetIds: new List<string>());
            IsEditMode = true;
        }

        private void Edit()
        {
            if (SelectedNetwork == null)
                return;
            IsEditMode = true;
        }

        private bool CanEditOrDelete() => SelectedNetwork != null;

        private async Task DeleteAsync()
        {
            if (SelectedNetwork == null)
                return;

            try
            {
                IsLoading = true;
                var result = await _networkService.DeleteAsync(SelectedNetwork.Id, "admin");
                if (result)
                {
                    await LoadAsync();
                    SelectedNetwork = null;
                    _logger.LogInformation("Deleted network {NetworkId}", SelectedNetwork?.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting network");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedNetwork == null)
                return;

            try
            {
                IsLoading = true;
                var updated = await _networkService.UpdateAsync(
                    SelectedNetwork.Id,
                    SelectedNetwork.Name,
                    SelectedNetwork.Cidr,
                    SelectedNetwork.Description,
                    SelectedNetwork.Type,
                    SelectedNetwork.VlanIds,
                    SelectedNetwork.SubnetIds,
                    SelectedNetwork.Tags,
                    "admin");

                await LoadAsync();
                SelectedNetwork = Networks.FirstOrDefault(n => n.Id == updated.Id);
                IsEditMode = false;
                _logger.LogInformation("Updated network {NetworkId}", updated.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving network");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            IsEditMode = false;
            SelectedNetwork = null;
        }

        private async Task FilterAsync()
        {
            try
            {
                IsLoading = true;

                var allNetworks = await _networkService.GetAllAsync();

                var filtered = allNetworks.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLowerInvariant();
                    filtered = filtered.Where(n =>
                        n.Name.ToLowerInvariant().Contains(searchLower) ||
                        n.Cidr.ToLowerInvariant().Contains(searchLower) ||
                        n.Description.ToLowerInvariant().Contains(searchLower));
                }

                Networks = new ObservableCollection<Network>(filtered.OrderBy(n => n.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering networks");
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
