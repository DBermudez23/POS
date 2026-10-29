namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacRolePermissionDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int? BusinessId { get; set; }
        public string? BusinessName { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? RoleDescription { get; set; }
        public string RoleIsSystem { get; set; } = "N";
        public string RoleFullAccess { get; set; } = "N";
        public string RoleStatus { get; set; } = "A";
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string? PermissionDescription { get; set; }
        public string PermissionStatus { get; set; } = "A";
        public int? AssignedByUserId { get; set; }
        public string? AssignedByUserName { get; set; }
        public string Status { get; set; } = "A";
        public DateTime AssignedAt { get; set; }
    }
}
