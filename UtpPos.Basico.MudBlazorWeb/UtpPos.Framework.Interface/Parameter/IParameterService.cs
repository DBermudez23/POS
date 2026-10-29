using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Parameter;

namespace UtpPos.Framework.Interface.Parameter
{
    public interface IParameterService
    {
        Task<Response<ParameterDto>> GetByCodeAsync(string parameterCode);
        Task<Response<IReadOnlyList<ParameterDto>>> ListAsync(ParameterListFilterDto filter);
        Task<Response<IReadOnlyList<ParameterOptionDto>>> ListByClassCodeAsync(string classCode);
    }
}
