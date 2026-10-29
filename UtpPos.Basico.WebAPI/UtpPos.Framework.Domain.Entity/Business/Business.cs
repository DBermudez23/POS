namespace UtpPos.Framework.Domain.Entity.Business
{
    public class Business
    {
        public int Id { get; set; }
        public string Nit { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = "A";
    }
}
