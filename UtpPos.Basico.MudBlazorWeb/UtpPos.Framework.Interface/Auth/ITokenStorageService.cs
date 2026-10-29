namespace UtpPos.Framework.Interface.Auth
{
    public interface ITokenStorageService
    {
        Task SetTokensAsync(string accessToken, string refreshToken);
        Task<string?> GetAccessTokenAsync();
        Task<string?> GetRefreshTokenAsync();
        Task ClearTokensAsync();
        Task<bool> HasAccessTokenAsync();
    }
}
