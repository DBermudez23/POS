using System.Security.Cryptography;

namespace UtpPos.Framework.Transversal.Common.Auth
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private const int TokenSizeInBytes = 64;
        private readonly JwtOptions _options;

        public RefreshTokenService(JwtOptions options)
        {
            _options = options;
        }

        public RefreshTokenResult GenerateToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);
            var token = Convert.ToBase64String(randomBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            return new RefreshTokenResult
            {
                Token = token,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays)
            };
        }
    }
}
