using System.Security.Cryptography;
using System.Text;

namespace UtpPos.Framework.Transversal.Common.Auth
{
    public class RefreshTokenHashService : IRefreshTokenHashService
    {
        private readonly byte[] _hashKey;

        public RefreshTokenHashService(JwtOptions options)
        {
            _hashKey = Encoding.UTF8.GetBytes(options.RefreshTokenHashKey);
        }

        public string Hash(string refreshToken)
        {
            using var hmac = new HMACSHA256(_hashKey);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(hash);
        }
    }
}
