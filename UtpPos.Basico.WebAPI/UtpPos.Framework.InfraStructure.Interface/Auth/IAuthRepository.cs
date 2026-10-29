using UtpPos.Framework.Domain.Entity.Auth;

namespace UtpPos.Framework.InfraStructure.Interface.Auth
{
    public interface IAuthRepository
    {
        Task<AuthUser?> GetUserForAuthenticationAsync(string email);
        Task<AuthUser?> GetUserProfileAsync(int userId);
        Task<int> RegisterUserSessionAsync(
            int userId,
            string refreshTokenHash,
            DateTime expirationDate,
            string? ip,
            string? userAgent);
        Task<UserSession?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash);
        Task<bool> RotateRefreshTokenAsync(
            int sessionId,
            string newRefreshTokenHash,
            DateTime newExpirationDate,
            string? ip,
            string? userAgent);
        Task<bool> RevokeSessionByRefreshTokenHashAsync(string refreshTokenHash);
        Task<bool> RevokeUserSessionAsync(int sessionId, int userId);
        Task<int> RevokeAllUserSessionsAsync(int userId);
        Task<int> CleanupExpiredSessionsAsync();
    }
}
