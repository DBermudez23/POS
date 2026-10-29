namespace UtpPos.Framework.Application.DTO.Kardex
{
    public sealed class KardexListFilterDto
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
