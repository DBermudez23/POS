using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacRoleFilterDto
    {
        [Range(1, int.MaxValue)]
        public int? BusinessId { get; set; }

        [StringLength(80)]
        public string? Name { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }

        public bool? OnlyGlobal { get; set; }
    }
}
