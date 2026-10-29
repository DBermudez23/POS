namespace UtpPos.Framework.Domain.Entity.Sale
{
    public sealed class SaleFilter
    {
        public int? BusinessId { get; set; }
        public int? ThirdPartyId { get; set; }
        public int? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
