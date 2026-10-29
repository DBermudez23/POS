namespace UtpPos.Framework.Shared.Auth
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public DateTime AccessTokenExpiresAtUtc { get; set; }
        public DateTime RefreshTokenExpiresAtUtc { get; set; }
        public AuthUserDto? User { get; set; }
        public AuthSessionDto? Session { get; set; }
    }
}
