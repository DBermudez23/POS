namespace UtpPos.Framework.Domain.Entity.Purchase
{
    public sealed class PurchaseCreate
    {
        public int BusinessId { get; set; }
        public int ThirdPartyId { get; set; }
        public int UserId { get; set; }
        public int KardexMovementTypeId { get; set; }
        public IReadOnlyList<PurchaseCreateDetail> Details { get; set; } = Array.Empty<PurchaseCreateDetail>();
        public IReadOnlyList<PurchaseCreatePayment> Payments { get; set; } = Array.Empty<PurchaseCreatePayment>();
    }
}
