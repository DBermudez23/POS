namespace UtpPos.Framework.Shared.Rbac
{
    public class RbacAssignPermissionToRoleDto
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public int? AssignedByUserId { get; set; }
    }
}
