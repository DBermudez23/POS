using DomainBusiness = UtpPos.Framework.Domain.Entity.Business.Business;

namespace UtpPos.Framework.InfraStructure.Interface.Business
{
    public interface IBusinessRepository
    {
        Task<IEnumerable<DomainBusiness>> ListAsync(string? nit, string? name, string? status);
        Task<DomainBusiness?> GetByIdAsync(int id);
        Task<int> CreateAsync(DomainBusiness business);
        Task<int> UpdateAsync(DomainBusiness business, bool clearPhone);
        Task<int> DeleteAsync(int id);
    }
}
