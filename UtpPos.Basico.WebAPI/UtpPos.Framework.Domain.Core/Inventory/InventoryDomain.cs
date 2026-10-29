using UtpPos.Framework.Domain.Core.Common;
using UtpPos.Framework.Domain.Interface.Inventory;
using UtpPos.Framework.InfraStructure.Interface.Inventory;
using DomainInventory = UtpPos.Framework.Domain.Entity.Inventory.Inventory;
using DomainInventoryFilter = UtpPos.Framework.Domain.Entity.Inventory.InventoryFilter;
using DomainInventoryMovement = UtpPos.Framework.Domain.Entity.Inventory.InventoryMovement;
using DomainInventoryMovementResult = UtpPos.Framework.Domain.Entity.Inventory.InventoryMovementResult;

namespace UtpPos.Framework.Domain.Core.Inventory
{
    public sealed class InventoryDomain : IInventoryDomain
    {
        private readonly IInventoryRepository _repository;

        public InventoryDomain(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<DomainInventory>> ListAsync(DomainInventoryFilter filter)
        {
            filter ??= new DomainInventoryFilter();

            if (filter.BusinessId.HasValue)
                DomainValidation.EnsurePositiveId(filter.BusinessId.Value, "BusinessId");

            if (filter.ProductId.HasValue)
                DomainValidation.EnsurePositiveId(filter.ProductId.Value, "ProductId");

            DomainValidation.EnsureActiveInactiveStatus(filter.Status);
            filter.Status = DomainValidation.NormalizeStatus(filter.Status);
            filter.ProductName = DomainValidation.EmptyToNull(filter.ProductName);

            return _repository.ListAsync(filter);
        }

        public Task<DomainInventory?> GetByIdAsync(int inventoryId)
        {
            DomainValidation.EnsurePositiveId(inventoryId, "InventoryId");
            return _repository.GetByIdAsync(inventoryId);
        }

        public Task<DomainInventoryMovementResult> RegisterMovementAsync(DomainInventoryMovement movement)
        {
            if (movement is null)
                throw new ArgumentNullException(nameof(movement));

            DomainValidation.EnsurePositiveId(movement.BusinessId, "BusinessId");
            DomainValidation.EnsurePositiveId(movement.ProductId, "ProductId");
            DomainValidation.EnsurePositiveId(movement.MovementTypeId, "MovementTypeId");
            DomainValidation.EnsurePositiveId(movement.CreatedByUserId, "CreatedByUserId");
            DomainValidation.EnsureAllowedCode(movement.OperationType, "OperationType", required: true, "E", "S");

            movement.OperationType = DomainValidation.NormalizeCode(movement.OperationType)!;
            DomainValidation.EnsurePositiveAmount(movement.Quantity, "Quantity");

            if (movement.OperationType == "E" && !movement.UnitCost.HasValue)
                throw new ArgumentException("UnitCost is required for input movements.");

            if (movement.UnitCost.HasValue)
                DomainValidation.EnsureNonNegativeAmount(movement.UnitCost.Value, "UnitCost");

            movement.Observation = DomainValidation.EmptyToNull(movement.Observation);

            return _repository.RegisterMovementAsync(movement);
        }
    }
}
