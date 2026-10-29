namespace UtpPos.Framework.Shared.Rbac
{
    public class RbacRoleFilterDto
    {
        public int? BusinessId { get; set; }
        public string? Name { get; set; }
        public string? Status { get; set; }
        public bool? OnlyGlobal { get; set; }
    }
}
