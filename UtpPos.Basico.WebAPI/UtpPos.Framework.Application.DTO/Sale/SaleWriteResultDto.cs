namespace UtpPos.Framework.Application.DTO.Sale
{
    public sealed class SaleWriteResultDto
    {
        public int SaleId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatValue { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPaid { get; set; }
    }
}
