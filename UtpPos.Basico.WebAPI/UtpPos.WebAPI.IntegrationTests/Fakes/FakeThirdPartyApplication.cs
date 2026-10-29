using UtpPos.Framework.Application.DTO.ThirdParty;
using UtpPos.Framework.Application.Interface.ThirdParty;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.WebAPI.IntegrationTests.Fakes;

public sealed class FakeThirdPartyApplication : IThirdPartyApplication
{
    public int ListCalls { get; private set; }
    public int ListByBusinessCalls { get; private set; }
    public int AssociateBusinessCalls { get; private set; }
    public int InactivateBusinessCalls { get; private set; }
    public int DeleteCalls { get; private set; }
    public int? LastBusinessId { get; private set; }
    public int? LastThirdPartyId { get; private set; }
    public int? LastUserId { get; private set; }

    public Task<Response<IEnumerable<ThirdPartyDto>>> ListAsync(ThirdPartyListFilterDto filter)
    {
        ListCalls++;
        return Task.FromResult(Success<IEnumerable<ThirdPartyDto>>(Array.Empty<ThirdPartyDto>()));
    }

    public Task<Response<IEnumerable<ThirdPartyBusinessDto>>> ListByBusinessAsync(int businessId, ThirdPartyBusinessListFilterDto filter)
    {
        ListByBusinessCalls++;
        LastBusinessId = businessId;
        return Task.FromResult(Success<IEnumerable<ThirdPartyBusinessDto>>(Array.Empty<ThirdPartyBusinessDto>()));
    }

    public Task<Response<ThirdPartyBusinessResultDto>> AssociateBusinessAsync(int businessId, int thirdPartyId, int userId)
    {
        AssociateBusinessCalls++;
        LastBusinessId = businessId;
        LastThirdPartyId = thirdPartyId;
        LastUserId = userId;
        return Task.FromResult(Success(new ThirdPartyBusinessResultDto { ThirdPartyId = thirdPartyId, NegterId = 123 }));
    }

    public Task<Response<ThirdPartyBusinessResultDto>> InactivateBusinessAsync(int businessId, int thirdPartyId, int userId)
    {
        InactivateBusinessCalls++;
        LastBusinessId = businessId;
        LastThirdPartyId = thirdPartyId;
        LastUserId = userId;
        return Task.FromResult(Success(new ThirdPartyBusinessResultDto { ThirdPartyId = thirdPartyId, NegterId = 123 }));
    }

    public Task<Response<ThirdPartyResultDto>> DeleteAsync(int thirdPartyId)
    {
        DeleteCalls++;
        return Task.FromResult(Success(new ThirdPartyResultDto { ThirdPartyId = thirdPartyId }));
    }

    public Task<Response<ThirdPartyDto>> GetByIdAsync(int thirdPartyId) => Task.FromResult(Success(new ThirdPartyDto { Id = thirdPartyId }));
    public Task<Response<ThirdPartyResultDto>> CreateAsync(ThirdPartyCreateDto dto, int userId) => Task.FromResult(Success(new ThirdPartyResultDto { ThirdPartyId = 1 }));
    public Task<Response<ThirdPartyResultDto>> UpdateAsync(int thirdPartyId, ThirdPartyUpdateDto dto) => Task.FromResult(Success(new ThirdPartyResultDto { ThirdPartyId = thirdPartyId }));
    public Task<Response<ThirdPartyRelationValidationDto>> ValidateBusinessAsync(int businessId, int thirdPartyId) => Task.FromResult(Success(new ThirdPartyRelationValidationDto { ExistsActiveRelation = true }));
    public Task<Response<ThirdPartyBusinessResultDto>> CreateForBusinessAsync(ThirdPartyCreateForBusinessDto dto, int userId) => Task.FromResult(Success(new ThirdPartyBusinessResultDto { ThirdPartyId = 1, NegterId = 123 }));

    private static Response<T> Success<T>(T data)
    {
        return new Response<T>
        {
            IsSuccess = true,
            Data = data,
            Message = "OK"
        };
    }
}
