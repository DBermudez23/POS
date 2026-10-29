using UtpPos.Framework.Interface.Authorization;
using UtpPos.Framework.Shared.Authorization;

namespace UtpPos.Framework.Main.Authorization
{
    public sealed class CurrentUserRoleStateService
    {
        private readonly ICurrentUserRoleService _roleService;
        private readonly object _syncRoot = new();
        private IReadOnlyList<CurrentUserRoleDto> _roles = Array.Empty<CurrentUserRoleDto>();
        private bool _isLoaded;
        private Task? _loadingTask;

        public CurrentUserRoleStateService(ICurrentUserRoleService roleService)
        {
            _roleService = roleService;
        }

        public event Action? OnChange;

        public IReadOnlyList<CurrentUserRoleDto> Roles
        {
            get
            {
                lock (_syncRoot)
                {
                    return _roles;
                }
            }
        }

        public bool HasSystemRole => Roles.Any(role =>
            IsActive(role) && IsYes(role.RoleIsSystem));

        public bool HasFullAccessRole => Roles.Any(role =>
            IsActive(role) && IsYes(role.RoleFullAccess));

        public string? ErrorMessage { get; private set; }

        public Task LoadAsync(bool forceReload = false)
        {
            lock (_syncRoot)
            {
                if (_loadingTask is not null)
                {
                    return _loadingTask;
                }

                if (_isLoaded && !forceReload)
                {
                    return Task.CompletedTask;
                }

                _loadingTask = LoadCoreAsync();
                return _loadingTask;
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _roles = Array.Empty<CurrentUserRoleDto>();
                _isLoaded = false;
                ErrorMessage = null;
            }

            OnChange?.Invoke();
        }

        private async Task LoadCoreAsync()
        {
            try
            {
                var response = await _roleService.GetCurrentUserRolesAsync();

                lock (_syncRoot)
                {
                    _roles = response.IsSuccess && response.Data is not null
                        ? response.Data.ToArray()
                        : Array.Empty<CurrentUserRoleDto>();
                    _isLoaded = response.IsSuccess;
                    ErrorMessage = response.IsSuccess ? null : response.Message;
                }
            }
            catch (Exception exception)
            {
                lock (_syncRoot)
                {
                    _roles = Array.Empty<CurrentUserRoleDto>();
                    _isLoaded = false;
                    ErrorMessage = exception.Message;
                }
            }
            finally
            {
                lock (_syncRoot)
                {
                    _loadingTask = null;
                }

                OnChange?.Invoke();
            }
        }

        private static bool IsActive(CurrentUserRoleDto role) =>
            string.Equals(role.Status, "A", StringComparison.OrdinalIgnoreCase)
            && string.Equals(role.RoleStatus, "A", StringComparison.OrdinalIgnoreCase);

        private static bool IsYes(string value) =>
            string.Equals(value, "S", StringComparison.OrdinalIgnoreCase);
    }
}
