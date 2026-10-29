namespace UtpPos.Framework.Domain.Entity.Purchase
{
    public sealed class PurchaseCancel
    {
        public int PurchaseId { get; set; }
        public int UserId { get; set; }
        public int CancellationKardexMovementTypeId { get; set; }
    }
}
