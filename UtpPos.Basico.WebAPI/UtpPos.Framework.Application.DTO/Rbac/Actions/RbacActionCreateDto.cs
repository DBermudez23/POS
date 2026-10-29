using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacActionCreateDto
    {
        [Required]
        [StringLength(80)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [Required]
        [DefaultValue("S")]
        [StringLength(1)]
        public string IsSystem { get; set; } = "S";

        [Required]
        [DefaultValue("A")]
        [StringLength(1)]
        public string Status { get; set; } = "A";
    }
}
