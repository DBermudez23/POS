namespace UtpPos.Framework.Shared.Business
{
    public sealed class BusinessUpdateDto
    {
        public string? Nit { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public bool ClearPhone { get; set; }
    }
}
