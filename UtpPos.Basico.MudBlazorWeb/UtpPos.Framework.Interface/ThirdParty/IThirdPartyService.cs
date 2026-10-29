using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.ThirdParty;

namespace UtpPos.Framework.Interface.ThirdParty
{
    public interface IThirdPartyService
    {
        Task<Response<IReadOnlyList<ThirdPartyDto>>> ListAsync(ThirdPartyListFilterDto filter);
        Task<Response<ThirdPartyDto>> GetByIdAsync(int id);
        Task<Response<ThirdPartyResultDto>> CreateAsync(ThirdPartyCreateDto request);
        Task<Response<ThirdPartyResultDto>> UpdateAsync(int id, ThirdPartyUpdateDto request);
        Task<Response<ThirdPartyResultDto>> DeleteAsync(int id);
        Task<Response<IReadOnlyList<ThirdPartyBusinessDto>>> ListByBusinessAsync(int businessId, ThirdPartyBusinessListFilterDto filter);
        Task<Response<ThirdPartyBusinessResultDto>> CreateForBusinessAsync(ThirdPartyCreateForBusinessDto request);
        Task<Response<ThirdPartyBusinessResultDto>> AssociateBusinessAsync(int businessId, int thirdPartyId);
        Task<Response<ThirdPartyBusinessResultDto>> InactivateBusinessAsync(int businessId, int thirdPartyId);
        Task<Response<ThirdPartyRelationValidationDto>> ValidateBusinessAsync(int businessId, int thirdPartyId);
    }
}
