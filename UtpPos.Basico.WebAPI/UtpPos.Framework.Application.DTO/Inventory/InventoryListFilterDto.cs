namespace UtpPos.Framework.Application.DTO.Inventory
{
    public sealed class InventoryListFilterDto
    {
        public int? BusinessId { get; set; }

        public int? ProductId { get; set; }

        public string? Status { get; set; }

        public string? ProductName { get; set; }
    }
}
