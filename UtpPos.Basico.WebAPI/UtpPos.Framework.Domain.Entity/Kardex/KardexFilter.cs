namespace UtpPos.Framework.Domain.Entity.Kardex
{
    public sealed class KardexFilter
    {
        public int? BusinessId { get; set; }

        public int? InventoryId { get; set; }

        public int? ProductId { get; set; }

        public int? MovementTypeId { get; set; }

        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
