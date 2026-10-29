using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacRoleDto
    {
        public int Id { get; set; }
        public int? BusinessId { get; set; }
        public string? BusinessName { get; set; }

        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(1)]
        public string IsSystem { get; set; } = "N";

        [StringLength(1)]
        public string FullAccess { get; set; } = "N";

        [StringLength(1)]
        public string Status { get; set; } = "A";

        public DateTime CreatedAt { get; set; }
    }
}
