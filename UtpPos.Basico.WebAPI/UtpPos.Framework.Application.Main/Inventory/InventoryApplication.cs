using AutoMapper;
using UtpPos.Framework.Application.DTO.Inventory;
using UtpPos.Framework.Application.Interface.Inventory;
using UtpPos.Framework.Domain.Interface.Inventory;
using UtpPos.Framework.Transversal.Common;
using DomainInventoryFilter = UtpPos.Framework.Domain.Entity.Inventory.InventoryFilter;
using DomainInventoryMovement = UtpPos.Framework.Domain.Entity.Inventory.InventoryMovement;

namespace UtpPos.Framework.Application.Main.Inventory
{
    public sealed class InventoryApplication : IInventoryApplication
    {
        private readonly IInventoryDomain _domain;
        private readonly IMapper _mapper;

        public InventoryApplication(IInventoryDomain domain, IMapper mapper)
        {
            _domain = domain;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<InventoryDto>>> ListAsync(InventoryListFilterDto filter)
        {
            try
            {
                filter ??= new InventoryListFilterDto();

                var inventory = await _domain.ListAsync(_mapper.Map<DomainInventoryFilter>(filter));
                return Success(_mapper.Map<IEnumerable<InventoryDto>>(inventory), "Inventario listado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<InventoryDto>>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<InventoryDto>>> ListByProductAsync(int productId)
        {
            try
            {
                var inventory = await _domain.ListAsync(new DomainInventoryFilter
                {
                    ProductId = productId
                });

                return Success(_mapper.Map<IEnumerable<InventoryDto>>(inventory), "Inventario del producto listado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<InventoryDto>>(ex.Message);
            }
        }

        public async Task<Response<InventoryDto>> GetByIdAsync(int inventoryId)
        {
            try
            {
                var inventory = await _domain.GetByIdAsync(inventoryId);

                if (inventory is null)
                    return Fail<InventoryDto>("Inventario no encontrado.");

                return Success(_mapper.Map<InventoryDto>(inventory), "Inventario encontrado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<InventoryDto>(ex.Message);
            }
        }

        public async Task<Response<InventoryMovementResultDto>> RegisterMovementAsync(InventoryMovementCreateDto dto)
        {
            try
            {
                if (dto is null)
                    return Fail<InventoryMovementResultDto>("La solicitud del movimiento de inventario es obligatoria.");

                var result = await _domain.RegisterMovementAsync(_mapper.Map<DomainInventoryMovement>(dto));
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Movimiento de inventario registrado correctamente."
                    : result.Message;

                return Success(_mapper.Map<InventoryMovementResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<InventoryMovementResultDto>(ex.Message);
            }
        }

        private static Response<T> Success<T>(T data, string message)
        {
            return new Response<T> { IsSuccess = true, Data = data, Message = message };
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T> { IsSuccess = false, Data = default, Message = message };
        }
    }
}
