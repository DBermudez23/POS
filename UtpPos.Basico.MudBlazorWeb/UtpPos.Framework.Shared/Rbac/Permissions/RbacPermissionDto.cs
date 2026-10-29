namespace UtpPos.Framework.Shared.Rbac
{
    public class RbacPermissionDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public int ActionId { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
