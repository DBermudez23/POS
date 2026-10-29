namespace UtpPos.Framework.Domain.Entity.Purchase
{
    public sealed class PurchaseCreatePayment
    {
        public int PaymentMethodId { get; set; }
        public decimal ReceivedValue { get; set; }
        public decimal ChangeValue { get; set; }
    }
}
