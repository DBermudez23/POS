namespace UtpPos.Framework.Application.DTO.Inventory
{
    public sealed class InventoryDto
    {
        public int Id { get; set; }

        public int BusinessId { get; set; }

        public string BusinessName { get; set; } = string.Empty;

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string ProductDescription { get; set; } = string.Empty;

        public int ProductCategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal CurrentStock { get; set; }

        public decimal MinimumStock { get; set; }

        public decimal MaximumStock { get; set; }

        public decimal WeightedAverageCost { get; set; }

        public decimal LastCost { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }

        public int? ModifiedByUserId { get; set; }

        public string? ModifiedByUserName { get; set; }

        public int CreatedByUserId { get; set; }

        public string CreatedByUserName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
