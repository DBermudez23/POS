namespace UtpPos.Database.IntegrationTests.ThirdParty;

public sealed record ThirdPartySeedData(
    int BusinessId,
    int UserId,
    int DocumentTypeId,
    int DepartmentId,
    int CityId,
    int ThirdPartyTypeId);
