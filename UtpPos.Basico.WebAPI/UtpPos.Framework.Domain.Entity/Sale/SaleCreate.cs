namespace UtpPos.Framework.Domain.Entity.Sale
{
    public sealed class SaleCreate
    {
        public int BusinessId { get; set; }
        public int ThirdPartyId { get; set; }
        public int UserId { get; set; }
        public int KardexMovementTypeId { get; set; }
        public IReadOnlyList<SaleCreateDetail> Details { get; set; } = Array.Empty<SaleCreateDetail>();
        public IReadOnlyList<SaleCreatePayment> Payments { get; set; } = Array.Empty<SaleCreatePayment>();
    }
}
