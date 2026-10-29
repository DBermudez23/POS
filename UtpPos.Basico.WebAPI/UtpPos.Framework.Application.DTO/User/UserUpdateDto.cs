namespace UtpPos.Framework.Application.DTO.User
{
    public class UserUpdateDto
    {
        public int UserId { get; set; }
        public int? BusinessId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public bool ClearBusinessId { get; set; }
        public bool ClearPhone { get; set; }
    }
}
