using Blazored.LocalStorage;
using UtpPos.Framework.Interface.Auth;

namespace UtpPos.Framework.Main.Auth
{
    public class TokenStorageService : ITokenStorageService
    {
        private const string AccessTokenKey = "access_token";
        private const string RefreshTokenKey = "refresh_token";

        private readonly ILocalStorageService _localStorage;

        public TokenStorageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task SetTokensAsync(string accessToken, string refreshToken)
        {
            await _localStorage.SetItemAsStringAsync(AccessTokenKey, accessToken);
            await _localStorage.SetItemAsStringAsync(RefreshTokenKey, refreshToken);
        }

        public async Task<string?> GetAccessTokenAsync() => await _localStorage.GetItemAsStringAsync(AccessTokenKey);

        public async Task<string?> GetRefreshTokenAsync() => await _localStorage.GetItemAsStringAsync(RefreshTokenKey);

        public async Task ClearTokensAsync()
        {
            await _localStorage.RemoveItemAsync(AccessTokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenKey);
        }

        public async Task<bool> HasAccessTokenAsync() =>
            !string.IsNullOrWhiteSpace(await GetAccessTokenAsync());
    }
}
