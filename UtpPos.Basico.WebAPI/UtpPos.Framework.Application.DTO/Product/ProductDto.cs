namespace UtpPos.Framework.Application.DTO.Product
{
    public sealed class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public string? Image { get; set; }

        public decimal? SaleValue { get; set; }

        public decimal VatPercentage { get; set; }

        public decimal ProfitMarginPercentage { get; set; }

        public string? ExternalId { get; set; }

        public int? BusinessId { get; set; }

        public string? BusinessName { get; set; }

        public int CreatedByUserId { get; set; }

        public string? CreatedByUserName { get; set; }

        public string Status { get; set; } = "A";

        public DateTime CreatedAt { get; set; }
    }
}
