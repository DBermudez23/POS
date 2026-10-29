using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Business;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Business;

namespace UtpPos.Framework.Main.Business
{
    public sealed class BusinessService : IBusinessService
    {
        private const string Endpoint = "api/negocios";

        private readonly IRestApi _restApi;

        public BusinessService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<BusinessDto>>> ListAsync(BusinessListFilterDto filter)
        {
            var response = await _restApi.GetAsync<IEnumerable<BusinessDto>>(
                BuildListEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<BusinessDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                StatusCode = response.StatusCode,
                Data = response.Data?.ToArray() ?? Array.Empty<BusinessDto>()
            };
        }

        public Task<Response<BusinessDto>> GetByIdAsync(int id) =>
            _restApi.GetAsync<BusinessDto>($"{Endpoint}/{id}", authorized: true);

        public Task<Response<int>> CreateAsync(BusinessCreateDto request) =>
            _restApi.PostAsync<BusinessCreateDto, int>(Endpoint, request, authorized: true);

        public Task<Response<int>> UpdateAsync(int id, BusinessUpdateDto request) =>
            _restApi.PatchAsync<BusinessUpdateDto, int>($"{Endpoint}/{id}", request, authorized: true);

        public Task<Response<int>> DeleteAsync(int id) =>
            _restApi.DeleteAsync<int>($"{Endpoint}/{id}", authorized: true);

        private static string BuildListEndpoint(BusinessListFilterDto filter)
        {
            var query = new List<string>();
            AddQueryValue(query, "Nit", filter.Nit);
            AddQueryValue(query, "Name", filter.Name);
            AddQueryValue(query, "Status", filter.Status);

            return query.Count == 0 ? Endpoint : $"{Endpoint}?{string.Join("&", query)}";
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
