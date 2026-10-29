namespace UtpPos.Framework.Application.DTO.User
{
    public class UserListFilterDto
    {
        public int? BusinessId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string? Status { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
    }
}
