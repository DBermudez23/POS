namespace UtpPos.Framework.Shared.Rbac
{
    public class RbacAssignRoleToUserDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int? AssignedByUserId { get; set; }
    }
}
