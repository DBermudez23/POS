using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Sale;

namespace UtpPos.Framework.Interface.Sale
{
    public interface ISaleService
    {
        Task<Response<IReadOnlyList<SaleDto>>> ListAsync(SaleListFilterDto filter);
        Task<Response<SaleDetailDto>> GetByIdAsync(int id);
        Task<Response<SaleWriteResultDto>> CreateAsync(SaleCreateDto request);
        Task<Response<SaleCancelResultDto>> CancelAsync(int id, SaleCancelDto request);
    }
}
