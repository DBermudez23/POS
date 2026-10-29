using UtpPos.Framework.Application.DTO.Sale;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Sale
{
    public interface ISaleApplication
    {
        Task<Response<IEnumerable<SaleDto>>> ListAsync(SaleListFilterDto filter);
        Task<Response<SaleDetailDto>> GetByIdAsync(int saleId);
        Task<Response<SaleWriteResultDto>> CreateAsync(SaleCreateDto dto, int userId);
        Task<Response<SaleCancelResultDto>> CancelAsync(int saleId, SaleCancelDto dto, int userId);
    }
}
