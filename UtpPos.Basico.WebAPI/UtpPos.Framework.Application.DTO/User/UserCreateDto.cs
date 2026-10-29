namespace UtpPos.Framework.Application.DTO.User
{
    public class UserCreateDto
    {
        public int? BusinessId { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = "A";
    }
}
