namespace UtpPos.Framework.Domain.Entity.Parameter
{
    public class Parameter
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassDescription { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "A";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
