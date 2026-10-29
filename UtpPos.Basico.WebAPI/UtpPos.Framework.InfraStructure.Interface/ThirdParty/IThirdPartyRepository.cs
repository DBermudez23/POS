using DomainThirdParty = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdParty;
using DomainThirdPartyBusiness = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusiness;
using DomainThirdPartyBusinessAssociation = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessAssociation;
using DomainThirdPartyBusinessFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessFilter;
using DomainThirdPartyBusinessResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessResult;
using DomainThirdPartyCreate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreate;
using DomainThirdPartyFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyFilter;
using DomainThirdPartyResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyResult;
using DomainThirdPartyUpdate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyUpdate;

namespace UtpPos.Framework.InfraStructure.Interface.ThirdParty
{
    public interface IThirdPartyRepository
    {
        Task<IEnumerable<DomainThirdParty>> ListAsync(DomainThirdPartyFilter filter);
        Task<DomainThirdParty?> GetByIdAsync(int thirdPartyId);
        Task<DomainThirdPartyResult> CreateAsync(DomainThirdPartyCreate thirdParty);
        Task<DomainThirdPartyResult> UpdateAsync(DomainThirdPartyUpdate thirdParty);
        Task<DomainThirdPartyResult> DeleteAsync(int thirdPartyId);
        Task<IEnumerable<DomainThirdPartyBusiness>> ListByBusinessAsync(DomainThirdPartyBusinessFilter filter);
        Task<DomainThirdPartyBusinessResult> AssociateBusinessAsync(DomainThirdPartyBusinessAssociation association);
        Task<DomainThirdPartyBusinessResult> InactivateBusinessAsync(DomainThirdPartyBusinessAssociation association);
        Task<bool> ValidateBusinessAsync(int businessId, int thirdPartyId);
    }
}
