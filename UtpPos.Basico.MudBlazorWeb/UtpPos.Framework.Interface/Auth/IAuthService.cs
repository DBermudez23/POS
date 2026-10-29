using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Auth;

namespace UtpPos.Framework.Interface.Auth
{
    public interface IAuthService
    {
        Task<Response<LoginResponse>> LoginAsync(LoginRequest request);
        Task<Response<AuthUserDto>> GetCurrentUserAsync();
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string?> GetAccessTokenAsync();
    }
}
