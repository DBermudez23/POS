using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.ThirdParty;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.ThirdParty;

namespace UtpPos.Framework.Main.ThirdParty
{
    public sealed class ThirdPartyService : IThirdPartyService
    {
        private const string Endpoint = "api/terceros";

        private readonly IRestApi _restApi;

        public ThirdPartyService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<ThirdPartyDto>>> ListAsync(ThirdPartyListFilterDto? filter = null)
        {
            filter ??= new ThirdPartyListFilterDto();

            var response = await _restApi.GetAsync<IEnumerable<ThirdPartyDto>>(
                BuildListEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<ThirdPartyDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                StatusCode = response.StatusCode,
                Data = response.Data?.ToArray() ?? Array.Empty<ThirdPartyDto>()
            };
        }

        public Task<Response<ThirdPartyDto>> GetByIdAsync(int id) =>
            _restApi.GetAsync<ThirdPartyDto>($"{Endpoint}/{id}", authorized: true);

        public Task<Response<ThirdPartyResultDto>> CreateAsync(ThirdPartyCreateDto request) =>
            _restApi.PostAsync<ThirdPartyCreateDto, ThirdPartyResultDto>(Endpoint, request, authorized: true);

        public Task<Response<ThirdPartyResultDto>> UpdateAsync(int id, ThirdPartyUpdateDto request) =>
            _restApi.PatchAsync<ThirdPartyUpdateDto, ThirdPartyResultDto>($"{Endpoint}/{id}", request, authorized: true);

        public Task<Response<ThirdPartyResultDto>> DeleteAsync(int id) =>
            _restApi.DeleteAsync<ThirdPartyResultDto>($"{Endpoint}/{id}", authorized: true);

        public async Task<Response<IReadOnlyList<ThirdPartyBusinessDto>>> ListByBusinessAsync(int businessId, ThirdPartyBusinessListFilterDto? filter = null)
        {
            filter ??= new ThirdPartyBusinessListFilterDto();

            var response = await _restApi.GetAsync<IEnumerable<ThirdPartyBusinessDto>>(
                BuildBusinessListEndpoint(businessId, filter),
                authorized: true);

            return new Response<IReadOnlyList<ThirdPartyBusinessDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                StatusCode = response.StatusCode,
                Data = response.Data?.ToArray() ?? Array.Empty<ThirdPartyBusinessDto>()
            };
        }

        public Task<Response<ThirdPartyBusinessResultDto>> CreateForBusinessAsync(ThirdPartyCreateForBusinessDto request) =>
            _restApi.PostAsync<ThirdPartyCreateForBusinessDto, ThirdPartyBusinessResultDto>($"{Endpoint}/por-negocio", request, authorized: true);

        public Task<Response<ThirdPartyBusinessResultDto>> AssociateBusinessAsync(int businessId, int thirdPartyId) =>
            _restApi.PostAsync<object, ThirdPartyBusinessResultDto>($"{Endpoint}/por-negocio/{businessId}/{thirdPartyId}", new { }, authorized: true);

        public Task<Response<ThirdPartyBusinessResultDto>> InactivateBusinessAsync(int businessId, int thirdPartyId) =>
            _restApi.DeleteAsync<ThirdPartyBusinessResultDto>($"{Endpoint}/por-negocio/{businessId}/{thirdPartyId}", authorized: true);

        public Task<Response<ThirdPartyRelationValidationDto>> ValidateBusinessAsync(int businessId, int thirdPartyId) =>
            _restApi.GetAsync<ThirdPartyRelationValidationDto>($"{Endpoint}/por-negocio/{businessId}/{thirdPartyId}/validar", authorized: true);

        private static string BuildListEndpoint(ThirdPartyListFilterDto filter)
        {
            var query = new List<string>();
            AddQueryValue(query, nameof(filter.DocumentTypeId), filter.DocumentTypeId);
            AddQueryValue(query, nameof(filter.Status), filter.Status);
            AddQueryValue(query, nameof(filter.DocumentNumber), filter.DocumentNumber);
            AddQueryValue(query, nameof(filter.Name), filter.Name);
            AddQueryValue(query, nameof(filter.Phone), filter.Phone);

            return query.Count == 0 ? Endpoint : $"{Endpoint}?{string.Join("&", query)}";
        }

        private static string BuildBusinessListEndpoint(int businessId, ThirdPartyBusinessListFilterDto filter)
        {
            var query = new List<string>();
            AddQueryValue(query, nameof(filter.DocumentTypeId), filter.DocumentTypeId);
            AddQueryValue(query, nameof(filter.Status), filter.Status);
            AddQueryValue(query, nameof(filter.DocumentNumber), filter.DocumentNumber);
            AddQueryValue(query, nameof(filter.Name), filter.Name);
            AddQueryValue(query, nameof(filter.Phone), filter.Phone);

            var endpoint = $"{Endpoint}/por-negocio/{businessId}";
            return query.Count == 0 ? endpoint : $"{endpoint}?{string.Join("&", query)}";
        }

        private static void AddQueryValue(ICollection<string> query, string name, int? value)
        {
            if (value.HasValue)
            {
                query.Add($"{name}={value.Value}");
            }
        }

        private static void AddQueryValue(ICollection<string> query, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
            }
        }
    }
}
