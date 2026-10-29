namespace UtpPos.Framework.Domain.Entity.Product
{
    public sealed class ProductFilter
    {
        public int? BusinessId { get; set; }

        public int? CategoryId { get; set; }

        public string? Status { get; set; }

        public string? Name { get; set; }

        public string? ExternalId { get; set; }
    }
}