using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Parameter
{
    public class ParameterUpdateDto
    {
        [Range(1, int.MaxValue)]
        public int? ClassId { get; set; }

        [StringLength(100)]
        public string? Description { get; set; }

        [StringLength(1)]
        public string? Status { get; set; }
    }
}
