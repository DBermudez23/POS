using UtpPos.Framework.Application.DTO.ThirdParty;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.ThirdParty
{
    public interface IThirdPartyApplication
    {
        Task<Response<IEnumerable<ThirdPartyDto>>> ListAsync(ThirdPartyListFilterDto filter);
        Task<Response<ThirdPartyDto>> GetByIdAsync(int thirdPartyId);
        Task<Response<ThirdPartyResultDto>> CreateAsync(ThirdPartyCreateDto dto, int userId);
        Task<Response<ThirdPartyResultDto>> UpdateAsync(int thirdPartyId, ThirdPartyUpdateDto dto);
        Task<Response<ThirdPartyResultDto>> DeleteAsync(int thirdPartyId);
        Task<Response<IEnumerable<ThirdPartyBusinessDto>>> ListByBusinessAsync(int businessId, ThirdPartyBusinessListFilterDto filter);
        Task<Response<ThirdPartyBusinessResultDto>> AssociateBusinessAsync(int businessId, int thirdPartyId, int userId);
        Task<Response<ThirdPartyBusinessResultDto>> InactivateBusinessAsync(int businessId, int thirdPartyId, int userId);
        Task<Response<ThirdPartyRelationValidationDto>> ValidateBusinessAsync(int businessId, int thirdPartyId);
        Task<Response<ThirdPartyBusinessResultDto>> CreateForBusinessAsync(ThirdPartyCreateForBusinessDto dto, int userId);
    }
}
