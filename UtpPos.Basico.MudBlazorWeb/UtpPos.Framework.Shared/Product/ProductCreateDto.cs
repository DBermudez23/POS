namespace UtpPos.Framework.Shared.Product
{
    public class ProductCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? Image { get; set; }
        public decimal VatPercentage { get; set; } = 0;
        public decimal ProfitMarginPercentage { get; set; } = 0;
        public string? ExternalId { get; set; }
        public int? BusinessId { get; set; }
        public int CreatedByUserId { get; set; }
        public string Status { get; set; } = "A";
    }
}
