using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacRolePermissionFilterDto
    {
        [Range(1, int.MaxValue)]
        public int? RoleId { get; set; }

        [Range(1, int.MaxValue)]
        public int? PermissionId { get; set; }

        [StringLength(180)]
        public string? PermissionName { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }
    }
}
