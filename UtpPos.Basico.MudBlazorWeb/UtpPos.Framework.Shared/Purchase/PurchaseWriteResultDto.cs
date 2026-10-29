namespace UtpPos.Framework.Shared.Purchase
{
    public sealed class PurchaseWriteResultDto
    {
        public int PurchaseId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatValue { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPaid { get; set; }
    }
}
