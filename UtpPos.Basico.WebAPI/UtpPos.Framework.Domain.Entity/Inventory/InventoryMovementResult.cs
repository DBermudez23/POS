namespace UtpPos.Framework.Domain.Entity.Inventory
{
    public sealed class InventoryMovementResult
    {
        public int KardexId { get; set; }

        public int InventoryId { get; set; }

        public decimal PreviousBalance { get; set; }

        public decimal CurrentBalance { get; set; }

        public decimal PreviousWeightedAverageCost { get; set; }

        public decimal CurrentWeightedAverageCost { get; set; }

        public decimal TotalMovement { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
