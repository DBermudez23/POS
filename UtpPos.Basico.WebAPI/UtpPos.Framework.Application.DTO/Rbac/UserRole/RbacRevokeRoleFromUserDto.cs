using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacRevokeRoleFromUserDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}
