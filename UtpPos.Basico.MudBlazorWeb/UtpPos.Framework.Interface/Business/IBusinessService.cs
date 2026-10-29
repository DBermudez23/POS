using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Business;

namespace UtpPos.Framework.Interface.Business
{
    public interface IBusinessService
    {
        Task<Response<IReadOnlyList<BusinessDto>>> ListAsync(BusinessListFilterDto filter);
        Task<Response<BusinessDto>> GetByIdAsync(int id);
        Task<Response<int>> CreateAsync(BusinessCreateDto request);
        Task<Response<int>> UpdateAsync(int id, BusinessUpdateDto request);
        Task<Response<int>> DeleteAsync(int id);
    }
}
