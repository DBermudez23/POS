namespace UtpPos.Framework.Domain.Entity.Sale
{
    public sealed class SaleCreatePayment
    {
        public int PaymentMethodId { get; set; }
        public decimal ReceivedValue { get; set; }
        public decimal ChangeValue { get; set; }
    }
}
