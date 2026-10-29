using UtpPos.Framework.Domain.Core.Common;
using UtpPos.Framework.Domain.Interface.Class;
using UtpPos.Framework.InfraStructure.Interface.Class;
using DomainClass = UtpPos.Framework.Domain.Entity.Class.Class;

namespace UtpPos.Framework.Domain.Core.Class
{
    public class ClassDomain : IClassDomain
    {
        private readonly IClassRepository _repository;

        public ClassDomain(IClassRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<DomainClass>> ListAsync(string? description, string? status, string? isEditable)
        {
            DomainValidation.EnsureActiveInactiveStatus(status);
            DomainValidation.EnsureAllowedCode(isEditable, "IsEditable", required: false, "S", "N");

            return _repository.ListAsync(
                description,
                DomainValidation.NormalizeStatus(status),
                DomainValidation.NormalizeCode(isEditable));
        }

        public Task<DomainClass?> GetByIdAsync(int id)
        {
            DomainValidation.EnsurePositiveId(id);
            return _repository.GetByIdAsync(id);
        }

        public Task<int> CreateAsync(DomainClass classEntity)
        {
            DomainValidation.EnsureRequired(classEntity.Description, "Description");

            classEntity.IsEditable = DomainValidation.NormalizeCode(classEntity.IsEditable) ?? "S";
            DomainValidation.EnsureAllowedCode(classEntity.IsEditable, "IsEditable", required: true, "S", "N");

            classEntity.Status = DomainValidation.NormalizeStatus(classEntity.Status) ?? "A";
            DomainValidation.EnsureActiveInactiveStatus(classEntity.Status, required: true);

            return _repository.CreateAsync(classEntity);
        }

        public Task<int> UpdateAsync(DomainClass classEntity)
        {
            DomainValidation.EnsurePositiveId(classEntity.Id);

            classEntity.IsEditable = DomainValidation.NormalizeCode(classEntity.IsEditable) ?? string.Empty;
            DomainValidation.EnsureAllowedCode(classEntity.IsEditable, "IsEditable", required: false, "S", "N");

            classEntity.Status = DomainValidation.NormalizeStatus(classEntity.Status) ?? string.Empty;
            DomainValidation.EnsureActiveInactiveStatus(classEntity.Status);

            return _repository.UpdateAsync(classEntity);
        }

        public Task<int> DeleteAsync(int id)
        {
            DomainValidation.EnsurePositiveId(id);
            return _repository.DeleteAsync(id);
        }
    }
}
