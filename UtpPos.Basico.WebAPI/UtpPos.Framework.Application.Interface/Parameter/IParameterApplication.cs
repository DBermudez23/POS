using UtpPos.Framework.Application.DTO.Parameter;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Parameter
{
    public interface IParameterApplication
    {
        Task<Response<IEnumerable<ParameterDto>>> ListAsync(ParameterListFilterDto filter);
        Task<Response<IEnumerable<ParameterOptionDto>>> ListByClassCodeAsync(string classCode);
        Task<Response<ParameterDto>> GetByIdAsync(int id);
        Task<Response<ParameterDto>> GetByCodeAsync(string parameterCode);
        Task<Response<int>> CreateAsync(ParameterCreateDto dto);
        Task<Response<int>> UpdateAsync(int id, ParameterUpdateDto dto);
        Task<Response<int>> DeleteAsync(int id);
    }
}
