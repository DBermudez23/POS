using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Auth;
using UtpPos.Framework.Main.Authorization;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Auth;

namespace UtpPos.Framework.Main.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IRestApi _restApi;
        private readonly ITokenStorageService _tokenStorageService;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private readonly PermissionStateService _permissionStateService;
        private readonly CurrentUserRoleStateService _roleStateService;

        public AuthService(
            IRestApi restApi,
            ITokenStorageService tokenStorageService,
            CustomAuthenticationStateProvider authenticationStateProvider,
            PermissionStateService permissionStateService,
            CurrentUserRoleStateService roleStateService)
        {
            _restApi = restApi;
            _tokenStorageService = tokenStorageService;
            _authenticationStateProvider = authenticationStateProvider;
            _permissionStateService = permissionStateService;
            _roleStateService = roleStateService;
        }

        public async Task<Response<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var response = await _restApi.PostAsync<LoginRequest, LoginResponse>(
                "api/auth/login",
                request);

            if (response.IsSuccess && response.Data is not null)
            {
                if (string.IsNullOrWhiteSpace(response.Data.AccessToken) || string.IsNullOrWhiteSpace(response.Data.RefreshToken))
                {
                    return new Response<LoginResponse>
                    {
                        IsSuccess = false,
                        Message = "El servidor no devolvió tokens de autenticación válidos."
                    };
                }

                await _tokenStorageService.SetTokensAsync(
                    response.Data.AccessToken,
                    response.Data.RefreshToken);

                await _authenticationStateProvider.NotifyUserAuthenticationAsync(response.Data.AccessToken);
                await Task.WhenAll(
                    _permissionStateService.LoadAsync(forceReload: true),
                    _roleStateService.LoadAsync(forceReload: true));
            }

            return response;
        }

        public Task<Response<AuthUserDto>> GetCurrentUserAsync() => _restApi.GetAsync<AuthUserDto>("api/auth/me", authorized: true);

        public async Task LogoutAsync()
        {
            var refreshToken = await _tokenStorageService.GetRefreshTokenAsync();

            try
            {
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    await _restApi.PostAsync<object, bool>(
                        "api/auth/logout",
                        new { RefreshToken = refreshToken });
                }
            }
            finally
            {
                await _authenticationStateProvider.ClearAuthenticationAsync();
                _permissionStateService.Clear();
                _roleStateService.Clear();
            }
        }

        public Task<bool> IsAuthenticatedAsync() => _tokenStorageService.HasAccessTokenAsync();

        public Task<string?> GetAccessTokenAsync() => _tokenStorageService.GetAccessTokenAsync();
    }
}
