using UtpPos.Framework.Application.DTO.Business;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Business
{
    public interface IBusinessApplication
    {
        Task<Response<IEnumerable<BusinessDto>>> ListAsync(BusinessListFilterDto filter);
        Task<Response<BusinessDto>> GetByIdAsync(int id);
        Task<Response<int>> CreateAsync(BusinessCreateDto dto);
        Task<Response<int>> UpdateAsync(int id, BusinessUpdateDto dto);
        Task<Response<int>> DeleteAsync(int id);
    }
}
