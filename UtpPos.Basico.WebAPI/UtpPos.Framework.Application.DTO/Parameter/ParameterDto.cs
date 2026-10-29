using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Parameter
{
    public class ParameterDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }

        [StringLength(100)]
        public string ClassDescription { get; set; } = string.Empty;

        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [StringLength(1)]
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
