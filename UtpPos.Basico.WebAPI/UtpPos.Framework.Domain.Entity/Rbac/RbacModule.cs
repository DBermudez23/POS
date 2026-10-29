namespace UtpPos.Framework.Domain.Entity.Rbac
{
    public class RbacModule
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string IsSystem { get; set; } = "S";
        public string Status { get; set; } = "A";
        public DateTime CreatedAt { get; set; }
    }
}
