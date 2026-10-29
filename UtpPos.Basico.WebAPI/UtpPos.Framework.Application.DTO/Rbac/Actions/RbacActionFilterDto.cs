using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacActionFilterDto
    {
        [StringLength(80)]
        public string? Code { get; set; }

        [StringLength(120)]
        public string? Name { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }
    }
}
