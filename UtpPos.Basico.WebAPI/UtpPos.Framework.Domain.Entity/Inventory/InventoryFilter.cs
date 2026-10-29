namespace UtpPos.Framework.Domain.Entity.Inventory
{
    public sealed class InventoryFilter
    {
        public int? BusinessId { get; set; }

        public int? ProductId { get; set; }

        public string? Status { get; set; }

        public string? ProductName { get; set; }
    }
}
