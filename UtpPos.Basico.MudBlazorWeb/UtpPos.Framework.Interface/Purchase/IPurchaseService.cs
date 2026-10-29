using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Purchase;

namespace UtpPos.Framework.Interface.Purchase
{
    public interface IPurchaseService
    {
        Task<Response<IReadOnlyList<PurchaseDto>>> ListAsync(PurchaseListFilterDto filter);
        Task<Response<PurchaseDetailDto>> GetByIdAsync(int id);
        Task<Response<PurchaseWriteResultDto>> CreateAsync(PurchaseCreateDto request);
        Task<Response<PurchaseCancelResultDto>> CancelAsync(int id, PurchaseCancelDto request);
    }
}
