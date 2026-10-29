using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacRoleUpdateDto : RbacRoleCreateDto
    {
        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}
