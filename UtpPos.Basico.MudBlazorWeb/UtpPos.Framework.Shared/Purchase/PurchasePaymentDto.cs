namespace UtpPos.Framework.Shared.Purchase
{
    public sealed class PurchasePaymentDto
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public decimal ReceivedValue { get; set; }
        public decimal ChangeValue { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
