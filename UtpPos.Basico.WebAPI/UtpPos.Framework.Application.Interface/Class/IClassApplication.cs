using UtpPos.Framework.Application.DTO.Class;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Class
{
    public interface IClassApplication
    {
        Task<Response<IEnumerable<ClassDto>>> ListAsync(ClassListFilterDto filter);
        Task<Response<ClassDto>> GetByIdAsync(int id);
        Task<Response<int>> CreateAsync(ClassCreateDto dto);
        Task<Response<int>> UpdateAsync(int id, ClassUpdateDto dto);
        Task<Response<int>> DeleteAsync(int id);
    }
}
