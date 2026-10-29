using DomainParameter = UtpPos.Framework.Domain.Entity.Parameter.Parameter;

namespace UtpPos.Framework.Domain.Interface.Parameter
{
    public interface IParameterDomain
    {
        Task<IEnumerable<DomainParameter>> ListAsync(int? classId, string? classDescription, string? description, string? status);
        Task<IEnumerable<DomainParameter>> ListByClassCodeAsync(string classCode, string? status);
        Task<DomainParameter?> GetByIdAsync(int id);
        Task<DomainParameter?> GetByCodeAsync(string parameterCode);
        Task<int> CreateAsync(DomainParameter parameter);
        Task<int> UpdateAsync(DomainParameter parameter);
        Task<int> DeleteAsync(int id);
    }
}
