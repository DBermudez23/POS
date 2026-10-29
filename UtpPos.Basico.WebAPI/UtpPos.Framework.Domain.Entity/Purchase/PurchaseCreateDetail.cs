namespace UtpPos.Framework.Domain.Entity.Purchase
{
    public sealed class PurchaseCreateDetail
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitValue { get; set; }
        public decimal VatPercentage { get; set; }
    }
}
