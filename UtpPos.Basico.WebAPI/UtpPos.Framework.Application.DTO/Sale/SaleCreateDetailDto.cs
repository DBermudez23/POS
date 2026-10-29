namespace UtpPos.Framework.Application.DTO.Sale
{
    public sealed class SaleCreateDetailDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitValue { get; set; }
        public decimal VatPercentage { get; set; }
    }
}
