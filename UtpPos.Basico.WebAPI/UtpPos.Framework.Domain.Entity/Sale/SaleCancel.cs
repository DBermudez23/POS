namespace UtpPos.Framework.Domain.Entity.Sale
{
    public sealed class SaleCancel
    {
        public int SaleId { get; set; }
        public int UserId { get; set; }
        public int CancellationKardexMovementTypeId { get; set; }
    }
}
