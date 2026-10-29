using DomainInventory = UtpPos.Framework.Domain.Entity.Inventory.Inventory;
using DomainInventoryFilter = UtpPos.Framework.Domain.Entity.Inventory.InventoryFilter;
using DomainInventoryMovement = UtpPos.Framework.Domain.Entity.Inventory.InventoryMovement;
using DomainInventoryMovementResult = UtpPos.Framework.Domain.Entity.Inventory.InventoryMovementResult;

namespace UtpPos.Framework.InfraStructure.Interface.Inventory
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<DomainInventory>> ListAsync(DomainInventoryFilter filter);

        Task<DomainInventory?> GetByIdAsync(int inventoryId);

        Task<DomainInventoryMovementResult> RegisterMovementAsync(DomainInventoryMovement movement);
    }
}
