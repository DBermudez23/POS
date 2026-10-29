using UtpPos.Framework.Interface.Authorization;

namespace UtpPos.Framework.Main.Authorization
{
    public sealed class PermissionStateService
    {
        private readonly IPermissionService _permissionService;
        private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);
        private readonly object _syncRoot = new();
        private bool _isLoaded;
        private Task? _loadingTask;

        public PermissionStateService(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public event Action? OnChange;

        public bool IsLoaded
        {
            get
            {
                // lock is neccessary to avoid race conditions
                lock (_syncRoot)
                {
                    return _isLoaded;
                }
            }
        }

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

        public bool HasPermission(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
            {
                return false;
            }

            lock (_syncRoot)
            {
                return _permissions.Contains(permissionName.Trim());
            }
        }

        public bool HasPermission(string moduleCode, string actionCode)
        {
            if (string.IsNullOrWhiteSpace(moduleCode) || string.IsNullOrWhiteSpace(actionCode))
            {
                return false;
            }

            return HasPermission($"{moduleCode.Trim()}.{actionCode.Trim()}");
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                _permissions.Clear();
                _isLoaded = false;
                ErrorMessage = null;
            }

            OnChange?.Invoke();
        }

        private async Task LoadCoreAsync()
        {
            try
            {
                var response = await _permissionService.GetCurrentUserPermissionsAsync();

                lock (_syncRoot)
                {
                    _permissions.Clear();

                    if (response.IsSuccess && response.Data is not null)
                    {
                        foreach (var permission in response.Data)
                        {
                            if (!string.IsNullOrWhiteSpace(permission.PermissionName))
                            {
                                _permissions.Add(permission.PermissionName.Trim());
                            }
                        }
                    }

                    _isLoaded = response.IsSuccess;
                    ErrorMessage = response.IsSuccess ? null : response.Message;
                }
            }
            catch (Exception exception)
            {
                lock (_syncRoot)
                {
                    _permissions.Clear();
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
    }
}
