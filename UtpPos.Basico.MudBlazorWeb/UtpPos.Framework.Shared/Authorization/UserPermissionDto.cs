namespace UtpPos.Framework.Shared.Authorization
{
    public sealed class UserPermissionDto
    {
        public int UserId { get; set; }
        public string? UserAccessTotal { get; set; }
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string ModuleCode { get; set; } = string.Empty;
        public string? ModuleName { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string? ActionName { get; set; }
        public string? PermissionDescription { get; set; }
        public string? PermissionStatus { get; set; }
    }
}
