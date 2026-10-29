namespace UtpPos.Framework.Shared.Sale
{
    public class SaleDto
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public int ThirdPartyId { get; set; }
        public string ThirdPartyName { get; set; } = string.Empty;
        public string ThirdPartyDocumentNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatValue { get; set; }
        public decimal Total { get; set; }
        public decimal CostTotal { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal TotalPaid { get; set; }
    }
}
