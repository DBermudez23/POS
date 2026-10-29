using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacPermissionFilterDto
    {
        [StringLength(180)]
        public string? PermissionName { get; set; }

        [StringLength(80)]
        public string? ModuleCode { get; set; }

        [StringLength(80)]
        public string? ActionCode { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }
    }
}
