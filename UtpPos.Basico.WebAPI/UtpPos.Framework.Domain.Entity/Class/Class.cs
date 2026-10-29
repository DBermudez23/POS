namespace UtpPos.Framework.Domain.Entity.Class
{
    public class Class
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string IsEditable { get; set; } = "S";
        public string Status { get; set; } = "A";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int TotalParameters { get; set; }
    }
}
