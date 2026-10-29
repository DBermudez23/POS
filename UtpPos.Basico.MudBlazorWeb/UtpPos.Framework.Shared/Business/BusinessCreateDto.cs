namespace UtpPos.Framework.Shared.Business
{
    public sealed class BusinessCreateDto
    {
        public string Nit { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Status { get; set; } = "A";
    }
}
