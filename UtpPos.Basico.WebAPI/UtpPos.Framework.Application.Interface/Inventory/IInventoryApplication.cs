using UtpPos.Framework.Application.DTO.Inventory;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Inventory
{
    public interface IInventoryApplication
    {
        Task<Response<IEnumerable<InventoryDto>>> ListAsync(InventoryListFilterDto filter);

        Task<Response<IEnumerable<InventoryDto>>> ListByProductAsync(int productId);

        Task<Response<InventoryDto>> GetByIdAsync(int inventoryId);

        Task<Response<InventoryMovementResultDto>> RegisterMovementAsync(InventoryMovementCreateDto dto);
    }
}
