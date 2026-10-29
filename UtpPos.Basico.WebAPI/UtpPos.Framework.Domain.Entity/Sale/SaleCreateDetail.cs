namespace UtpPos.Framework.Domain.Entity.Sale
{
    public sealed class SaleCreateDetail
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitValue { get; set; }
        public decimal VatPercentage { get; set; }
    }
}
