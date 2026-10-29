namespace UtpPos.Framework.Domain.Entity.Rbac
{
    public class RbacUserRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? BusinessId { get; set; }
        public string? BusinessName { get; set; }
        public string UserDocumentNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserStatus { get; set; } = "A";
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? RoleDescription { get; set; }
        public string RoleIsSystem { get; set; } = "N";
        public string RoleFullAccess { get; set; } = "N";
        public string RoleStatus { get; set; } = "A";
        public int? AssignedByUserId { get; set; }
        public string? AssignedByUserName { get; set; }
        public string Status { get; set; } = "A";
        public DateTime AssignedAt { get; set; }
    }
}
