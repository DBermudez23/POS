namespace UtpPos.Framework.Shared.Parameter
{
    public class ParameterDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassDescription { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
