using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Class
{
    public class ClassDto
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [StringLength(1)]
        public string IsEditable { get; set; } = string.Empty;

        [StringLength(1)]
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public int TotalParameters { get; set; }
    }
}
