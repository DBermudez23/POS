namespace UtpPos.Framework.Application.DTO.Auth
{
    public class AuthSessionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? Ip { get; set; }
        public string? UserAgent { get; set; } // Is the browser or client information that initiated the session.
        public DateTime CreatedAt { get; set; }
    }
}
