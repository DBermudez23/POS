namespace UtpPos.Framework.Application.DTO.Sale
{
    public sealed class SaleDetailDto : SaleDto
    {
        public IReadOnlyList<SaleLineDto> Details { get; set; } = Array.Empty<SaleLineDto>();
        public IReadOnlyList<SalePaymentDto> Payments { get; set; } = Array.Empty<SalePaymentDto>();
    }
}
