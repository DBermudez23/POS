using System.ComponentModel.DataAnnotations;

namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacPermissionDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }

        [StringLength(80)]
        public string ModuleCode { get; set; } = string.Empty;

        [StringLength(120)]
        public string ModuleName { get; set; } = string.Empty;

        public int ActionId { get; set; }

        [StringLength(80)]
        public string ActionCode { get; set; } = string.Empty;

        [StringLength(120)]
        public string ActionName { get; set; } = string.Empty;

        [StringLength(180)]
        public string PermissionName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(1)]
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
