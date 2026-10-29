namespace UtpPos.Framework.Shared.Kardex
{
    public sealed class KardexDto
    {
        public int Id { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public int MovementTypeId { get; set; }
        public string MovementTypeName { get; set; } = string.Empty;
        public decimal InputQuantity { get; set; }
        public decimal OutputQuantity { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal UnitCost { get; set; }
        public decimal PreviousWeightedAverageCost { get; set; }
        public decimal CurrentWeightedAverageCost { get; set; }
        public decimal Total { get; set; }
        public string? Observation { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
