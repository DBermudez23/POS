using UtpPos.Framework.Domain.Core.Common;
using UtpPos.Framework.Domain.Interface.Business;
using UtpPos.Framework.InfraStructure.Interface.Business;
using DomainBusiness = UtpPos.Framework.Domain.Entity.Business.Business;

namespace UtpPos.Framework.Domain.Core.Business
{
    public class BusinessDomain : IBusinessDomain
    {
        private readonly IBusinessRepository _repository;

        public BusinessDomain(IBusinessRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<DomainBusiness>> ListAsync(string? nit, string? name, string? status)
        {
            DomainValidation.EnsureActiveInactiveStatus(status);
            return _repository.ListAsync(nit, name, DomainValidation.NormalizeStatus(status));
        }

        public Task<DomainBusiness?> GetByIdAsync(int id)
        {
            DomainValidation.EnsurePositiveId(id);
            return _repository.GetByIdAsync(id);
        }

        public Task<int> CreateAsync(DomainBusiness business)
        {
            DomainValidation.EnsureRequired(business.Nit, "Nit");
            DomainValidation.EnsureRequired(business.Name, "Name");
            DomainValidation.EnsureRequired(business.Address, "Address");

            business.Status = DomainValidation.NormalizeStatus(business.Status) ?? "A";
            DomainValidation.EnsureActiveInactiveStatus(business.Status, required: true);

            return _repository.CreateAsync(business);
        }

        public Task<int> UpdateAsync(DomainBusiness business, bool clearPhone)
        {
            DomainValidation.EnsurePositiveId(business.Id);
            DomainValidation.EnsureActiveInactiveStatus(business.Status);

            business.Status = DomainValidation.NormalizeStatus(business.Status) ?? string.Empty;
            return _repository.UpdateAsync(business, clearPhone);
        }

        public Task<int> DeleteAsync(int id)
        {
            DomainValidation.EnsurePositiveId(id);
            return _repository.DeleteAsync(id);
        }
    }
}
