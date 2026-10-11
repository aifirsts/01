// SOCpit - Security Operations Center
// Users view model for WPF application
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
    /// <summary>Users view model.</summary>
    public sealed class UsersViewModel : INotifyPropertyChanged
    {
        private readonly UserService _userService;
        private readonly ILogger<UsersViewModel> _logger;

        private ObservableCollection<User> _users = new();
        private User? _selectedUser;
        private UserRole _selectedRoleFilter = UserRole.Analyst;
        private UserStatus _selectedStatusFilter = UserStatus.Active;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;

        public UsersViewModel(
            UserService userService,
            ILogger<UsersViewModel> logger)
        {
            _userService = userService;
            _logger = logger;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
            RefreshCommand = new AsyncRelayCommand(LoadAsync);
            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, CanEditOrDelete);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, CanEditOrDelete);
            SaveCommand = new AsyncRelayCommand(SaveAsync, () => _isEditMode);
            CancelCommand = new RelayCommand(Cancel, () => _isEditMode);
            FilterCommand = new AsyncRelayCommand(FilterAsync);
            LockCommand = new AsyncRelayCommand(LockAsync, CanEditOrDelete);
            UnlockCommand = new AsyncRelayCommand(UnlockAsync, CanEditOrDelete);

            RoleFilters = new ObservableCollection<UserRole>(Enum.GetValues(typeof(UserRole)).Cast<UserRole>());
            StatusFilters = new ObservableCollection<UserStatus>(Enum.GetValues(typeof(UserStatus)).Cast<UserStatus>());

            _ = LoadAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                if (_users != value)
                {
                    _users = value;
                    OnPropertyChanged();
                }
            }
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public UserRole SelectedRoleFilter
        {
            get => _selectedRoleFilter;
            set
            {
                if (_selectedRoleFilter != value)
                {
                    _selectedRoleFilter = value;
                    OnPropertyChanged();
                }
            }
        }

        public UserStatus SelectedStatusFilter
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

        public ObservableCollection<UserRole> RoleFilters { get; }
        public ObservableCollection<UserStatus> StatusFilters { get; }

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand LockCommand { get; }
        public ICommand UnlockCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var users = await _userService.GetAllAsync();
                Users = new ObservableCollection<User>(users.OrderBy(u => u.Username));
                _logger.LogInformation("Loaded {Count} users", users.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Add()
        {
            SelectedUser = User.Create(
                id: Guid.NewGuid().ToString(),
                username: "newuser",
                email: "newuser@example.com",
                fullName: "New User",
                role: UserRole.Analyst,
                status: UserStatus.Active,
                authenticationType: AuthenticationType.Local,
                createdAt: DateTimeOffset.UtcNow,
                tags: new List<string>(),
                department: null,
                phone: null,
                permissionIds: new List<string>());
            IsEditMode = true;
        }

        private void Edit()
        {
            if (SelectedUser == null)
                return;
            IsEditMode = true;
        }

        private bool CanEditOrDelete() => SelectedUser != null;

        private async Task DeleteAsync()
        {
            if (SelectedUser == null)
                return;

            try
            {
                IsLoading = true;
                var result = await _userService.DeleteAsync(SelectedUser.Id, "admin");
                if (result)
                {
                    await LoadAsync();
                    SelectedUser = null;
                    _logger.LogInformation("Deleted user {UserId}", SelectedUser?.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedUser == null)
                return;

            try
            {
                IsLoading = true;
                var updated = await _userService.UpdateAsync(
                    SelectedUser.Id,
                    SelectedUser.Username,
                    SelectedUser.Email,
                    SelectedUser.FullName,
                    SelectedUser.Role,
                    SelectedUser.Status,
                    SelectedUser.AuthenticationType,
                    SelectedUser.Department,
                    SelectedUser.Phone,
                    SelectedUser.Tags,
                    SelectedUser.PermissionIds,
                    "admin");

                await LoadAsync();
                SelectedUser = Users.FirstOrDefault(u => u.Id == updated.Id);
                IsEditMode = false;
                _logger.LogInformation("Updated user {UserId}", updated.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            IsEditMode = false;
            SelectedUser = null;
        }

        private async Task LockAsync()
        {
            if (SelectedUser == null)
                return;

            try
            {
                IsLoading = true;
                var locked = await _userService.LockAsync(SelectedUser.Id, null, "admin");
                await LoadAsync();
                SelectedUser = Users.FirstOrDefault(u => u.Id == locked.Id);
                _logger.LogInformation("Locked user {UserId}", locked.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UnlockAsync()
        {
            if (SelectedUser == null)
                return;

            try
            {
                IsLoading = true;
                var unlocked = await _userService.UnlockAsync(SelectedUser.Id, "admin");
                await LoadAsync();
                SelectedUser = Users.FirstOrDefault(u => u.Id == unlocked.Id);
                _logger.LogInformation("Unlocked user {UserId}", unlocked.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user");
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

                var allUsers = await _userService.GetAllAsync();

                var filtered = allUsers.AsEnumerable();

                if (SelectedRoleFilter != UserRole.Analyst)
                {
                    filtered = filtered.Where(u => u.Role == SelectedRoleFilter);
                }

                if (SelectedStatusFilter != UserStatus.Active)
                {
                    filtered = filtered.Where(u => u.Status == SelectedStatusFilter);
                }

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var searchLower = SearchText.ToLowerInvariant();
                    filtered = filtered.Where(u =>
                        u.Username.ToLowerInvariant().Contains(searchLower) ||
                        u.Email.ToLowerInvariant().Contains(searchLower) ||
                        u.FullName.ToLowerInvariant().Contains(searchLower));
                }

                Users = new ObservableCollection<User>(filtered.OrderBy(u => u.Username));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering users");
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
