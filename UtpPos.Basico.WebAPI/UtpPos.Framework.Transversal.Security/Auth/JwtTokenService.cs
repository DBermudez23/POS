using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UtpPos.Framework.Domain.Entity.Auth;
using Microsoft.IdentityModel.Tokens;
using UtpPos.Framework.Transversal.Interface.Auth;

namespace UtpPos.Framework.Transversal.Common.Auth
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;
        private readonly SigningCredentials _signingCredentials;

        public JwtTokenService(JwtOptions options)
        {
            _options = options;
            _signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey)),
                SecurityAlgorithms.HmacSha256);
        }

        public AccessTokenResult GenerateAccessToken(AuthUser user, int sessionId)
        {
            var issuedAtUtc = DateTime.UtcNow;
            var expiresAtUtc = issuedAtUtc.AddMinutes(_options.AccessTokenExpirationMinutes);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new(
                    JwtRegisteredClaimNames.Iat,
                    EpochTime.GetIntDate(issuedAtUtc).ToString(),
                    ClaimValueTypes.Integer64),
                new(AuthClaimTypes.UserId, user.Id.ToString()),
                new(AuthClaimTypes.Email, user.Email),
                new(AuthClaimTypes.Name, user.Name),
                new(AuthClaimTypes.SessionId, sessionId.ToString())
            };

            if (user.BusinessId.HasValue)
                claims.Add(new Claim(AuthClaimTypes.BusinessId, user.BusinessId.Value.ToString()));

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: issuedAtUtc,
                expires: expiresAtUtc,
                signingCredentials: _signingCredentials);

            return new AccessTokenResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt),
                ExpiresAtUtc = expiresAtUtc,
                ExpiresIn = checked(_options.AccessTokenExpirationMinutes * 60)
            };
        }
    }
}
