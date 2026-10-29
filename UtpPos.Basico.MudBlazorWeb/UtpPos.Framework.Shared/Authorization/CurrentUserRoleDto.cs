namespace UtpPos.Framework.Shared.Authorization
{
    public sealed class CurrentUserRoleDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? BusinessId { get; set; }
        public string? BusinessName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? RoleDescription { get; set; }
        public string RoleIsSystem { get; set; } = "N";
        public string RoleFullAccess { get; set; } = "N";
        public string RoleStatus { get; set; } = "A";
        public string Status { get; set; } = "A";
        public DateTime AssignedAt { get; set; }
    }
}
