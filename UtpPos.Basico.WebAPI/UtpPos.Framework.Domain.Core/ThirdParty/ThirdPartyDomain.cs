using UtpPos.Framework.Domain.Core.Common;
using UtpPos.Framework.Domain.Interface.ThirdParty;
using UtpPos.Framework.InfraStructure.Interface.ThirdParty;
using DomainThirdParty = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdParty;
using DomainThirdPartyBusiness = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusiness;
using DomainThirdPartyBusinessAssociation = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessAssociation;
using DomainThirdPartyBusinessFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessFilter;
using DomainThirdPartyBusinessResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessResult;
using DomainThirdPartyCreate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreate;
using DomainThirdPartyCreateForBusiness = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreateForBusiness;
using DomainThirdPartyFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyFilter;
using DomainThirdPartyResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyResult;
using DomainThirdPartyUpdate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyUpdate;

namespace UtpPos.Framework.Domain.Core.ThirdParty
{
    public sealed class ThirdPartyDomain : IThirdPartyDomain
    {
        private readonly IThirdPartyRepository _repository;

        public ThirdPartyDomain(IThirdPartyRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<DomainThirdParty>> ListAsync(DomainThirdPartyFilter filter)
        {
            filter ??= new DomainThirdPartyFilter();

            if (filter.DocumentTypeId.HasValue)
                DomainValidation.EnsurePositiveId(filter.DocumentTypeId.Value, "DocumentTypeId");

            DomainValidation.EnsureActiveInactiveStatus(filter.Status);
            filter.Status = DomainValidation.NormalizeStatus(filter.Status);
            filter.DocumentNumber = DomainValidation.EmptyToNull(filter.DocumentNumber);
            filter.Name = DomainValidation.EmptyToNull(filter.Name);
            filter.Phone = DomainValidation.EmptyToNull(filter.Phone);

            return _repository.ListAsync(filter);
        }

        public Task<DomainThirdParty?> GetByIdAsync(int thirdPartyId)
        {
            DomainValidation.EnsurePositiveId(thirdPartyId, "ThirdPartyId");
            return _repository.GetByIdAsync(thirdPartyId);
        }

        public Task<DomainThirdPartyResult> CreateAsync(DomainThirdPartyCreate thirdParty)
        {
            if (thirdParty is null)
                throw new ArgumentNullException(nameof(thirdParty));

            DomainValidation.EnsurePositiveId(thirdParty.DocumentTypeId, "DocumentTypeId");
            DomainValidation.EnsureRequired(thirdParty.DocumentNumber, "DocumentNumber");
            DomainValidation.EnsureRequired(thirdParty.Name, "Name");
            DomainValidation.EnsurePositiveId(thirdParty.DepartmentId, "DepartmentId");
            DomainValidation.EnsurePositiveId(thirdParty.CityId, "CityId");
            DomainValidation.EnsureRequired(thirdParty.Phone, "Phone");
            DomainValidation.EnsurePositiveId(thirdParty.CreatedByUserId, "CreatedByUserId");
            DomainValidation.EnsurePositiveId(thirdParty.ThirdPartyTypeId, "ThirdPartyTypeId");
            DomainValidation.EnsureActiveInactiveStatus(thirdParty.Status, required: true);

            thirdParty.DocumentNumber = thirdParty.DocumentNumber.Trim();
            thirdParty.Name = thirdParty.Name.Trim();
            thirdParty.Address = DomainValidation.EmptyToNull(thirdParty.Address);
            thirdParty.Phone = thirdParty.Phone.Trim();
            thirdParty.Status = DomainValidation.NormalizeStatus(thirdParty.Status) ?? "A";

            return _repository.CreateAsync(thirdParty);
        }

        public Task<DomainThirdPartyResult> UpdateAsync(DomainThirdPartyUpdate thirdParty)
        {
            if (thirdParty is null)
                throw new ArgumentNullException(nameof(thirdParty));

            DomainValidation.EnsurePositiveId(thirdParty.Id, "ThirdPartyId");

            if (thirdParty.DocumentTypeId.HasValue)
                DomainValidation.EnsurePositiveId(thirdParty.DocumentTypeId.Value, "DocumentTypeId");

            if (thirdParty.DepartmentId.HasValue)
                DomainValidation.EnsurePositiveId(thirdParty.DepartmentId.Value, "DepartmentId");

            if (thirdParty.CityId.HasValue)
                DomainValidation.EnsurePositiveId(thirdParty.CityId.Value, "CityId");

            if (thirdParty.ThirdPartyTypeId.HasValue)
                DomainValidation.EnsurePositiveId(thirdParty.ThirdPartyTypeId.Value, "ThirdPartyTypeId");

            DomainValidation.EnsureActiveInactiveStatus(thirdParty.Status);

            thirdParty.DocumentNumber = DomainValidation.EmptyToNull(thirdParty.DocumentNumber);
            thirdParty.Name = DomainValidation.EmptyToNull(thirdParty.Name);
            thirdParty.Address = DomainValidation.EmptyToNull(thirdParty.Address);
            thirdParty.Phone = DomainValidation.EmptyToNull(thirdParty.Phone);
            thirdParty.Status = DomainValidation.NormalizeStatus(thirdParty.Status);

            return _repository.UpdateAsync(thirdParty);
        }

        public Task<DomainThirdPartyResult> DeleteAsync(int thirdPartyId)
        {
            DomainValidation.EnsurePositiveId(thirdPartyId, "ThirdPartyId");
            return _repository.DeleteAsync(thirdPartyId);
        }

        public Task<IEnumerable<DomainThirdPartyBusiness>> ListByBusinessAsync(DomainThirdPartyBusinessFilter filter)
        {
            filter ??= new DomainThirdPartyBusinessFilter();

            DomainValidation.EnsurePositiveId(filter.BusinessId, "BusinessId");

            if (filter.DocumentTypeId.HasValue)
                DomainValidation.EnsurePositiveId(filter.DocumentTypeId.Value, "DocumentTypeId");

            DomainValidation.EnsureActiveInactiveStatus(filter.Status);
            filter.Status = DomainValidation.NormalizeStatus(filter.Status);
            filter.DocumentNumber = DomainValidation.EmptyToNull(filter.DocumentNumber);
            filter.Name = DomainValidation.EmptyToNull(filter.Name);
            filter.Phone = DomainValidation.EmptyToNull(filter.Phone);

            return _repository.ListByBusinessAsync(filter);
        }

        public Task<DomainThirdPartyBusinessResult> AssociateBusinessAsync(DomainThirdPartyBusinessAssociation association)
        {
            ValidateAssociation(association);
            return _repository.AssociateBusinessAsync(association);
        }

        public Task<DomainThirdPartyBusinessResult> InactivateBusinessAsync(DomainThirdPartyBusinessAssociation association)
        {
            ValidateAssociation(association);
            return _repository.InactivateBusinessAsync(association);
        }

        public Task<bool> ValidateBusinessAsync(int businessId, int thirdPartyId)
        {
            DomainValidation.EnsurePositiveId(businessId, "BusinessId");
            DomainValidation.EnsurePositiveId(thirdPartyId, "ThirdPartyId");
            return _repository.ValidateBusinessAsync(businessId, thirdPartyId);
        }

        public async Task<DomainThirdPartyBusinessResult> CreateForBusinessAsync(DomainThirdPartyCreateForBusiness thirdParty)
        {
            if (thirdParty is null)
                throw new ArgumentNullException(nameof(thirdParty));

            DomainValidation.EnsurePositiveId(thirdParty.BusinessId, "BusinessId");
            var createdByUserId = thirdParty.CreatedByUserId;
            var normalizedDocumentNumber = thirdParty.DocumentNumber?.Trim() ?? string.Empty;

            DomainValidation.EnsurePositiveId(thirdParty.DocumentTypeId, "DocumentTypeId");
            DomainValidation.EnsureRequired(normalizedDocumentNumber, "DocumentNumber");

            var existing = (await _repository.ListAsync(new DomainThirdPartyFilter
                {
                    DocumentTypeId = thirdParty.DocumentTypeId,
                    DocumentNumber = normalizedDocumentNumber
                }))
                .FirstOrDefault(item => item.DocumentTypeId == thirdParty.DocumentTypeId
                    && string.Equals(item.DocumentNumber, normalizedDocumentNumber, StringComparison.OrdinalIgnoreCase));

            var thirdPartyId = existing?.Id;
            if (!thirdPartyId.HasValue)
            {
                var createResult = await CreateAsync(thirdParty);
                thirdPartyId = createResult.ThirdPartyId;
            }

            return await AssociateBusinessAsync(new DomainThirdPartyBusinessAssociation
            {
                BusinessId = thirdParty.BusinessId,
                ThirdPartyId = thirdPartyId.Value,
                UserId = createdByUserId
            });
        }

        private static void ValidateAssociation(DomainThirdPartyBusinessAssociation association)
        {
            if (association is null)
                throw new ArgumentNullException(nameof(association));

            DomainValidation.EnsurePositiveId(association.BusinessId, "BusinessId");
            DomainValidation.EnsurePositiveId(association.ThirdPartyId, "ThirdPartyId");
            DomainValidation.EnsurePositiveId(association.UserId, "UserId");
        }
    }
}
