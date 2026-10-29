namespace UtpPos.Framework.Application.DTO.Rbac
{
    public class RbacUserPermissionDto
    {
        public int UserId { get; set; }
        public string UserAccessTotal { get; set; } = "N";
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string? PermissionDescription { get; set; }
        public string PermissionStatus { get; set; } = "A";
    }
}
