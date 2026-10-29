namespace UtpPos.Framework.Domain.Entity.Product
{
    public sealed class ProductUpdate
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public int? CategoryId { get; set; }

        public string? Image { get; set; }

        public decimal? VatPercentage { get; set; }

        public decimal? ProfitMarginPercentage { get; set; }

        public string? ExternalId { get; set; }

        public int? BusinessId { get; set; }

        public string? Status { get; set; }

        public bool ClearImage { get; set; }

        public bool ClearExternalId { get; set; }

        public bool ClearBusiness { get; set; }
    }
}
