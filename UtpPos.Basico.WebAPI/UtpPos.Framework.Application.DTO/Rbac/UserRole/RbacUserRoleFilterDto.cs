using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacUserRoleFilterDto
    {
        [Range(1, int.MaxValue)]
        public int? UserId { get; set; }

        [Range(1, int.MaxValue)]
        public int? RoleId { get; set; }

        [EmailAddress]
        [StringLength(80)]
        public string? UserEmail { get; set; }

        [StringLength(80)]
        public string? RoleName { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }
    }
}
