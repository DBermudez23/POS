using UtpPos.Framework.Application.DTO.Purchase;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Purchase
{
    public interface IPurchaseApplication
    {
        Task<Response<IEnumerable<PurchaseDto>>> ListAsync(PurchaseListFilterDto filter);
        Task<Response<PurchaseDetailDto>> GetByIdAsync(int purchaseId);
        Task<Response<PurchaseWriteResultDto>> CreateAsync(PurchaseCreateDto dto, int userId);
        Task<Response<PurchaseCancelResultDto>> CancelAsync(int purchaseId, PurchaseCancelDto dto, int userId);
    }
}
