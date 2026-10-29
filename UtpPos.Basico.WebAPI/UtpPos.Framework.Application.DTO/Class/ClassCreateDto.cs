using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Class
{
    public class ClassCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [DefaultValue("S")]
        [StringLength(1)]
        public string IsEditable { get; set; } = "S";

        [DefaultValue("A")]
        [StringLength(1)]
        public string Status { get; set; } = "A";
    }
}
