namespace UtpPos.Framework.Shared.Inventory
{
    public sealed class InventoryMovementCreateDto
    {
        public int BusinessId { get; set; }
        public int ProductId { get; set; }
        public int MovementTypeId { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public string? Observation { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
