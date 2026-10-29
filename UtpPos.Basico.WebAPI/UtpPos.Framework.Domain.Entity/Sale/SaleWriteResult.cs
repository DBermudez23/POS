namespace UtpPos.Framework.Domain.Entity.Sale
{
    public sealed class SaleWriteResult
    {
        public int SaleId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatValue { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPaid { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
