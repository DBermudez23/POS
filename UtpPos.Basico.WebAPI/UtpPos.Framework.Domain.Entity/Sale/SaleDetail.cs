namespace UtpPos.Framework.Domain.Entity.Sale
{
    public sealed class SaleDetail
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductExternalId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitValue { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatValue { get; set; }
        public decimal Total { get; set; }
        public decimal VatPercentage { get; set; }
        public decimal UnitCost { get; set; }
        public decimal CostTotal { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
