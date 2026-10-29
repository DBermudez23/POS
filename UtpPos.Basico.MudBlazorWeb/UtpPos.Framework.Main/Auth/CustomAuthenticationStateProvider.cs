using System.Security.Claims;
using System.Text.Json;
using UtpPos.Framework.Interface.Auth;
using Microsoft.AspNetCore.Components.Authorization;

namespace UtpPos.Framework.Main.Auth
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly AuthenticationState AnonymousState = new(new ClaimsPrincipal(new ClaimsIdentity()));

        private readonly ITokenStorageService _tokenStorageService;

        public CustomAuthenticationStateProvider(ITokenStorageService tokenStorageService)
        {
            _tokenStorageService = tokenStorageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var accessToken = await _tokenStorageService.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return AnonymousState;
            }

            return await CreateAuthenticationStateAsync(accessToken);
        }

        public async Task NotifyUserAuthenticationAsync(string accessToken)
        {
            var authenticationState = await CreateAuthenticationStateAsync(accessToken);
            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
        }

        public async Task ClearAuthenticationAsync()
        {
            await _tokenStorageService.ClearTokensAsync();
            NotifyUserLogout();
        }

        private async Task<AuthenticationState> CreateAuthenticationStateAsync(string accessToken)
        {
            try
            {
                var claims = ParseClaims(accessToken);
                var expirationClaim = claims.FirstOrDefault(claim => claim.Type == "exp")?.Value;

                if (!long.TryParse(expirationClaim, out var expirationSeconds) || DateTimeOffset.FromUnixTimeSeconds(expirationSeconds) <= DateTimeOffset.UtcNow)
                {
                    await _tokenStorageService.ClearTokensAsync();
                    return AnonymousState;
                }

                AddStandardClaims(claims);

                var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (Exception exception) when (
                exception is FormatException
                or JsonException
                or ArgumentException
                or InvalidOperationException)
            {
                await _tokenStorageService.ClearTokensAsync();
                return AnonymousState;
            }
        }

        private static List<Claim> ParseClaims(string jwt)
        {
            var segments = jwt.Split('.');
            if (segments.Length < 2)
            {
                throw new FormatException("El token JWT no contiene un payload válido.");
            }

            var payloadBytes = DecodeBase64Url(segments[1]);
            using var payload = JsonDocument.Parse(payloadBytes);

            var claims = new List<Claim>();

            foreach (var property in payload.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    claims.AddRange(property.Value.EnumerateArray()
                        .Select(value => new Claim(property.Name, GetClaimValue(value))));
                    continue;
                }

                claims.Add(new Claim(property.Name, GetClaimValue(property.Value)));
            }

            return claims;
        }

        private static byte[] DecodeBase64Url(string value)
        {
            var base64 = value.Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            return Convert.FromBase64String(base64);
        }

        private static string GetClaimValue(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            JsonValueKind.Null => string.Empty,
            _ => value.GetRawText()
        };

        private static void AddStandardClaims(List<Claim> claims)
        {
            AddMappedClaim(claims, ClaimTypes.NameIdentifier, "sub", "usr_id");
            AddMappedClaim(claims, ClaimTypes.Name, "usr_nombre", "usr_correo");
            AddMappedClaim(claims, ClaimTypes.Email, "usr_correo");
            AddMappedClaim(claims, ClaimTypes.Sid, "sid");
        }

        private static void AddMappedClaim(List<Claim> claims, string targetType, params string[] sourceTypes)
        {
            if (claims.Any(claim => claim.Type == targetType))
            {
                return;
            }

            var sourceClaim = sourceTypes
                .Select(sourceType => claims.FirstOrDefault(claim => claim.Type == sourceType))
                .FirstOrDefault(claim => claim is not null);

            if (sourceClaim is not null)
            {
                claims.Add(new Claim(targetType, sourceClaim.Value));
            }
        }
    }
}
