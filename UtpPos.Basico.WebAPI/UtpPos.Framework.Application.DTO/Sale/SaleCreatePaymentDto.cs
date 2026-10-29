namespace UtpPos.Framework.Application.DTO.Sale
{
    public sealed class SaleCreatePaymentDto
    {
        public int PaymentMethodId { get; set; }
        public decimal ReceivedValue { get; set; }
        public decimal ChangeValue { get; set; }
    }
}
