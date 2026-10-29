namespace UtpPos.Framework.Domain.Entity.Purchase
{
    public sealed class PurchaseCancelResult
    {
        public int PurchaseId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
