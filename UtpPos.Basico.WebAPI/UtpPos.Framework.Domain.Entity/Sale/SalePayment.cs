namespace UtpPos.Framework.Domain.Entity.Sale
{
    public sealed class SalePayment
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public decimal ReceivedValue { get; set; }
        public decimal ChangeValue { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
