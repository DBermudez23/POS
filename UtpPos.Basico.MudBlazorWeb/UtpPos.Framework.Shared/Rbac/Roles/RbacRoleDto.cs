namespace UtpPos.Framework.Shared.Rbac
{
    public class RbacRoleDto
    {
        public int Id { get; set; }
        public int? BusinessId { get; set; }
        public string? BusinessName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string IsSystem { get; set; } = "N";
        public string FullAccess { get; set; } = "N";
        public string Status { get; set; } = "A";
        public DateTime CreatedAt { get; set; }
    }
}
