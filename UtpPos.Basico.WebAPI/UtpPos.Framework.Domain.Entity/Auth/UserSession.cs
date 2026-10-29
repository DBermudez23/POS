namespace UtpPos.Framework.Domain.Entity.Auth
{
    public class UserSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? BusinessId { get; set; }
        public string? BusinessName { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserStatus { get; set; } = string.Empty;
        public string RefreshTokenHash { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
        public bool Revoked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
