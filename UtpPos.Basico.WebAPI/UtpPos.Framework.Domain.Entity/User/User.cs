namespace UtpPos.Framework.Domain.Entity.User
{
    public class User
    {
        public int Id { get; set; }
        public int? BusinessId { get; set; }
        public string? BusinessName { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = "A";
        public DateTime CreatedAt { get; set; }
    }
}
