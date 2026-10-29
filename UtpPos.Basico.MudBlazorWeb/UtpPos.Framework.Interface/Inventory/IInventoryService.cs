using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Inventory;

namespace UtpPos.Framework.Interface.Inventory
{
    public interface IInventoryService
    {
        Task<Response<IReadOnlyList<InventoryDto>>> ListAsync(InventoryListFilterDto filter);
        Task<Response<InventoryDto>> GetByIdAsync(int id);
        Task<Response<IReadOnlyList<InventoryDto>>> ListByProductAsync(int productId);
        Task<Response<InventoryMovementResultDto>> RegisterMovementAsync(InventoryMovementCreateDto request);
    }
}
