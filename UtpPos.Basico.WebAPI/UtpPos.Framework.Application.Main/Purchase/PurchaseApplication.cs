using AutoMapper;
using UtpPos.Framework.Application.DTO.Purchase;
using UtpPos.Framework.Application.Interface.Purchase;
using UtpPos.Framework.Domain.Interface.Purchase;
using UtpPos.Framework.Transversal.Common;
using DomainPurchaseCancel = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCancel;
using DomainPurchaseCreate = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreate;
using DomainPurchaseFilter = UtpPos.Framework.Domain.Entity.Purchase.PurchaseFilter;

namespace UtpPos.Framework.Application.Main.Purchase
{
    public sealed class PurchaseApplication : IPurchaseApplication
    {
        private readonly IPurchaseDomain _domain;
        private readonly IMapper _mapper;

        public PurchaseApplication(IPurchaseDomain domain, IMapper mapper)
        {
            _domain = domain;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<PurchaseDto>>> ListAsync(PurchaseListFilterDto filter)
        {
            try
            {
                filter ??= new PurchaseListFilterDto();
                var purchases = await _domain.ListAsync(_mapper.Map<DomainPurchaseFilter>(filter));
                return Success(_mapper.Map<IEnumerable<PurchaseDto>>(purchases), "Compras listadas correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<PurchaseDto>>(ex.Message);
            }
        }

        public async Task<Response<PurchaseDetailDto>> GetByIdAsync(int purchaseId)
        {
            try
            {
                var purchase = await _domain.GetByIdAsync(purchaseId);

                if (purchase is null)
                    return Fail<PurchaseDetailDto>("Compra no encontrada.");

                return Success(_mapper.Map<PurchaseDetailDto>(purchase), "Compra encontrada correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<PurchaseDetailDto>(ex.Message);
            }
        }

        public async Task<Response<PurchaseWriteResultDto>> CreateAsync(PurchaseCreateDto dto, int userId)
        {
            try
            {
                if (dto is null)
                    return Fail<PurchaseWriteResultDto>("La solicitud de compra es obligatoria.");

                var purchase = _mapper.Map<DomainPurchaseCreate>(dto);
                purchase.UserId = userId;

                var result = await _domain.CreateAsync(purchase);
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Compra registrada correctamente."
                    : result.Message;

                return Success(_mapper.Map<PurchaseWriteResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<PurchaseWriteResultDto>(ex.Message);
            }
        }

        public async Task<Response<PurchaseCancelResultDto>> CancelAsync(int purchaseId, PurchaseCancelDto dto, int userId)
        {
            try
            {
                if (dto is null)
                    return Fail<PurchaseCancelResultDto>("La solicitud de anulación de compra es obligatoria.");

                var cancellation = _mapper.Map<DomainPurchaseCancel>(dto);
                cancellation.PurchaseId = purchaseId;
                cancellation.UserId = userId;

                var result = await _domain.CancelAsync(cancellation);
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Compra anulada correctamente."
                    : result.Message;

                return Success(_mapper.Map<PurchaseCancelResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<PurchaseCancelResultDto>(ex.Message);
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
