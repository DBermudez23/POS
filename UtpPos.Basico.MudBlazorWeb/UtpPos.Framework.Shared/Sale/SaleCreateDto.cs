namespace UtpPos.Framework.Shared.Sale
{
    public sealed class SaleCreateDto
    {
        public int BusinessId { get; set; }
        public int ThirdPartyId { get; set; }
        public int KardexMovementTypeId { get; set; }
        public IReadOnlyList<SaleCreateDetailDto> Details { get; set; } = Array.Empty<SaleCreateDetailDto>();
        public IReadOnlyList<SaleCreatePaymentDto> Payments { get; set; } = Array.Empty<SaleCreatePaymentDto>();
    }
}
