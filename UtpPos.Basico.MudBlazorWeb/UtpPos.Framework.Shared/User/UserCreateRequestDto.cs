namespace UtpPos.Framework.Shared.User
{
    public class UserCreateRequestDto
    {
        public int? BusinessId { get; set; }
        public int DocumentTypeId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = "A";
    }
}
