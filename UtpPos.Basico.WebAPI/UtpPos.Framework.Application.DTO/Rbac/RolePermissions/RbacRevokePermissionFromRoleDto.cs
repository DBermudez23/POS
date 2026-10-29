using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacRevokePermissionFromRoleDto
    {
        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }

        [Range(1, int.MaxValue)]
        public int PermissionId { get; set; }
    }
}
