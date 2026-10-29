namespace UtpPos.Framework.Application.DTO.Purchase
{
    public sealed class PurchaseCreatePaymentDto
    {
        public int PaymentMethodId { get; set; }
        public decimal ReceivedValue { get; set; }
        public decimal ChangeValue { get; set; }
    }
}
