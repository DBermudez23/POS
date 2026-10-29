using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Class
{
    public class ClassUpdateDto
    {
        [StringLength(100)]
        public string? Description { get; set; }

        [StringLength(1)]
        public string? IsEditable { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }
    }
}
