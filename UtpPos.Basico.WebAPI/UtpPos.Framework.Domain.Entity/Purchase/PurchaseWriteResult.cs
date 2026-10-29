namespace UtpPos.Framework.Domain.Entity.Purchase
{
    public sealed class PurchaseWriteResult
    {
        public int PurchaseId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatValue { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPaid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
