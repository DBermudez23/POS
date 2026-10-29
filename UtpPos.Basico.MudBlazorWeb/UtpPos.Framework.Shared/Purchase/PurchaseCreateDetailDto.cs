namespace UtpPos.Framework.Shared.Purchase
{
    public sealed class PurchaseCreateDetailDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitValue { get; set; }
        public decimal VatPercentage { get; set; }
    }
}
