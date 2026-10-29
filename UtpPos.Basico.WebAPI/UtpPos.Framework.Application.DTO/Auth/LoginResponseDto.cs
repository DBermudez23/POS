using UtpPos.Framework.Application.DTO.User;

namespace UtpPos.Framework.Application.DTO.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public DateTime AccessTokenExpiresAtUtc { get; set; }
        public DateTime RefreshTokenExpiresAtUtc { get; set; }
        public UserDto User { get; set; } = new();
        public AuthSessionDto Session { get; set; } = new();
    }
}
