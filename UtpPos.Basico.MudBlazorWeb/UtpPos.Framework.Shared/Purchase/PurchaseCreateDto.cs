namespace UtpPos.Framework.Shared.Purchase
{
    public sealed class PurchaseCreateDto
    {
        public int BusinessId { get; set; }
        public int ThirdPartyId { get; set; }
        public int KardexMovementTypeId { get; set; }
        public IReadOnlyList<PurchaseCreateDetailDto> Details { get; set; } = Array.Empty<PurchaseCreateDetailDto>();
        public IReadOnlyList<PurchaseCreatePaymentDto> Payments { get; set; } = Array.Empty<PurchaseCreatePaymentDto>();
    }
}
