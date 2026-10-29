using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacModuleDto
    {
        public int Id { get; set; }

        [StringLength(80)]
        public string Code { get; set; } = string.Empty;

        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(1)]
        public string IsSystem { get; set; } = string.Empty;

        [StringLength(1)]
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
