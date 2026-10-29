using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacPermissionCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(80)]
        public string ModuleCode { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int ActionId { get; set; }

        [Required]
        [StringLength(80)]
        public string ActionCode { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [Required]
        [DefaultValue("A")]
        [StringLength(1)]
        public string Status { get; set; } = "A";
    }
}
