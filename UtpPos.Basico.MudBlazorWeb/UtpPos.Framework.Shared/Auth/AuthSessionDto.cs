namespace UtpPos.Framework.Shared.Auth
{
    public class AuthSessionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
