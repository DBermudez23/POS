using DomainClass = UtpPos.Framework.Domain.Entity.Class.Class;

namespace UtpPos.Framework.InfraStructure.Interface.Class
{
    public interface IClassRepository
    {
        Task<IEnumerable<DomainClass>> ListAsync(string? description, string? status, string? isEditable);
        Task<DomainClass?> GetByIdAsync(int id);
        Task<int> CreateAsync(DomainClass classEntity);
        Task<int> UpdateAsync(DomainClass classEntity);
        Task<int> DeleteAsync(int id);
    }
}
