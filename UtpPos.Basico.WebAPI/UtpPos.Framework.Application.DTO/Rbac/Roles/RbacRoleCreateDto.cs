using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacRoleCreateDto
    {
        [Range(1, int.MaxValue)]
        public int? BusinessId { get; set; }

        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [Required]
        [DefaultValue("N")]
        [StringLength(1)]
        public string IsSystem { get; set; } = "N";

        [Required]
        [DefaultValue("N")]
        [StringLength(1)]
        public string FullAccess { get; set; } = "N";

        [Required]
        [DefaultValue("A")]
        [StringLength(1)]
        public string Status { get; set; } = "A";
    }
}
