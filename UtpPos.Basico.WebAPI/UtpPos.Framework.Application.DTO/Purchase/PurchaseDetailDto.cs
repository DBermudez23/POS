namespace UtpPos.Framework.Application.DTO.Purchase
{
    public sealed class PurchaseDetailDto : PurchaseDto
    {
        public IReadOnlyList<PurchaseLineDto> Details { get; set; } = Array.Empty<PurchaseLineDto>();
        public IReadOnlyList<PurchasePaymentDto> Payments { get; set; } = Array.Empty<PurchasePaymentDto>();
    }
}
