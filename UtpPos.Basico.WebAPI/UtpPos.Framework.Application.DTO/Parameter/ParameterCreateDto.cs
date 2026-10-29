using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Parameter
{
    public class ParameterCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ClassId { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [DefaultValue("A")]
        [StringLength(1)]
        public string Status { get; set; } = "A";
    }
}
