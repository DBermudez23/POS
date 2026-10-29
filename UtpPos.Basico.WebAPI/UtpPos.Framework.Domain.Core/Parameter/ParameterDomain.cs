using UtpPos.Framework.Domain.Core.Common;
using UtpPos.Framework.Domain.Interface.Parameter;
using UtpPos.Framework.InfraStructure.Interface.Class;
using UtpPos.Framework.InfraStructure.Interface.Parameter;
using DomainParameter = UtpPos.Framework.Domain.Entity.Parameter.Parameter;

namespace UtpPos.Framework.Domain.Core.Parameter
{
    public class ParameterDomain : IParameterDomain
    {
        private readonly IParameterRepository _repository;
        private readonly IClassRepository _classRepository;

        public ParameterDomain(IParameterRepository repository, IClassRepository classRepository)
        {
            _repository = repository;
            _classRepository = classRepository;
        }

        public Task<IEnumerable<DomainParameter>> ListAsync(int? classId, string? classDescription, string? description, string? status)
        {
            if (classId.HasValue)
                DomainValidation.EnsurePositiveId(classId.Value, "ClassId");

            DomainValidation.EnsureActiveInactiveStatus(status);

            return _repository.ListAsync(
                classId,
                classDescription,
                description,
                DomainValidation.NormalizeStatus(status));
        }

        public Task<IEnumerable<DomainParameter>> ListByClassCodeAsync(string classCode, string? status)
        {
            DomainValidation.EnsureRequired(classCode, "ClassCode");
            DomainValidation.EnsureActiveInactiveStatus(status);

            return _repository.ListByClassCodeAsync(
                DomainValidation.NormalizeCode(classCode)!,
                DomainValidation.NormalizeStatus(status));
        }

        public Task<DomainParameter?> GetByIdAsync(int id)
        {
            DomainValidation.EnsurePositiveId(id);
            return _repository.GetByIdAsync(id);
        }

        public Task<DomainParameter?> GetByCodeAsync(string parameterCode)
        {
            DomainValidation.EnsureRequired(parameterCode, "ParameterCode");
            return _repository.GetByCodeAsync(DomainValidation.NormalizeCode(parameterCode)!);
        }

        public async Task<int> CreateAsync(DomainParameter parameter)
        {
            DomainValidation.EnsurePositiveId(parameter.ClassId, "ClassId");
            DomainValidation.EnsureRequired(parameter.Description, "Description");

            parameter.Status = DomainValidation.NormalizeStatus(parameter.Status) ?? "A";
            DomainValidation.EnsureActiveInactiveStatus(parameter.Status, required: true);

            await EnsureClassAllowsParameterChangesAsync(parameter.ClassId);

            return await _repository.CreateAsync(parameter);
        }

        public async Task<int> UpdateAsync(DomainParameter parameter)
        {
            DomainValidation.EnsurePositiveId(parameter.Id);

            if (parameter.ClassId > 0)
                DomainValidation.EnsurePositiveId(parameter.ClassId, "ClassId");

            parameter.Status = DomainValidation.NormalizeStatus(parameter.Status) ?? string.Empty;
            DomainValidation.EnsureActiveInactiveStatus(parameter.Status);

            var currentParameter = await EnsureExistingParameterAsync(parameter.Id);
            await EnsureClassAllowsParameterChangesAsync(currentParameter.ClassId);

            if (parameter.ClassId > 0 && parameter.ClassId != currentParameter.ClassId)
                await EnsureClassAllowsParameterChangesAsync(parameter.ClassId);

            return await _repository.UpdateAsync(parameter);
        }

        public async Task<int> DeleteAsync(int id)
        {
            DomainValidation.EnsurePositiveId(id);

            var currentParameter = await EnsureExistingParameterAsync(id);
            await EnsureClassAllowsParameterChangesAsync(currentParameter.ClassId);

            return await _repository.DeleteAsync(id);
        }

        private async Task<DomainParameter> EnsureExistingParameterAsync(int id)
        {
            var parameter = await _repository.GetByIdAsync(id);
            if (parameter is null)
                throw new InvalidOperationException("Parameter was not found.");

            return parameter;
        }

        private async Task EnsureClassAllowsParameterChangesAsync(int classId)
        {
            var classEntity = await _classRepository.GetByIdAsync(classId);
            if (classEntity is null)
                throw new InvalidOperationException("The parameter class was not found.");

            if (DomainValidation.NormalizeStatus(classEntity.Status) != "A")
                throw new InvalidOperationException("The parameter class is inactive.");

            if (DomainValidation.NormalizeCode(classEntity.IsEditable) != "S")
                throw new InvalidOperationException("The parameter class is not editable.");
        }
    }
}
