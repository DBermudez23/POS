using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Business
{
    public class BusinessCreateDto
    {
        [Required]
        [StringLength(20)]
        public string Nit { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [DefaultValue("A")]
        [StringLength(1)]
        public string Status { get; set; } = "A";
    }
}
