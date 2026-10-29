using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacAssignRoleToUserDto
    {
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }

        [Range(1, int.MaxValue)]
        public int? AssignedByUserId { get; set; }
    }
}
