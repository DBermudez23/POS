using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Sale;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Sale;

namespace UtpPos.Framework.Main.Sale
{
    public sealed class SaleService : ISaleService
    {
        private const string Endpoint = "api/ventas";

        private readonly IRestApi _restApi;

        public SaleService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<SaleDto>>> ListAsync(SaleListFilterDto? filter = null)
        {
            filter ??= new SaleListFilterDto();

            var response = await _restApi.GetAsync<IEnumerable<SaleDto>>(
                BuildListEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<SaleDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                StatusCode = response.StatusCode,
                Data = response.Data?.ToArray() ?? Array.Empty<SaleDto>()
            };
        }

        public Task<Response<SaleDetailDto>> GetByIdAsync(int id) =>
            _restApi.GetAsync<SaleDetailDto>($"{Endpoint}/{id}", authorized: true);

        public Task<Response<SaleWriteResultDto>> CreateAsync(SaleCreateDto request) =>
            _restApi.PostAsync<SaleCreateDto, SaleWriteResultDto>(Endpoint, request, authorized: true);

        public Task<Response<SaleCancelResultDto>> CancelAsync(int id, SaleCancelDto request) =>
            _restApi.PostAsync<SaleCancelDto, SaleCancelResultDto>($"{Endpoint}/{id}/anular", request, authorized: true);

        private static string BuildListEndpoint(SaleListFilterDto filter)
        {
            var query = new List<string>();
            AddQueryValue(query, nameof(filter.BusinessId), filter.BusinessId);
            AddQueryValue(query, nameof(filter.ThirdPartyId), filter.ThirdPartyId);
            AddQueryValue(query, nameof(filter.UserId), filter.UserId);
            AddQueryValue(query, nameof(filter.Status), filter.Status);
            AddQueryValue(query, nameof(filter.StartDate), filter.StartDate);
            AddQueryValue(query, nameof(filter.EndDate), filter.EndDate);

            return query.Count == 0 ? Endpoint : $"{Endpoint}?{string.Join("&", query)}";
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

        private static void AddQueryValue(ICollection<string> query, string name, DateTime? value)
        {
            if (value.HasValue)
            {
                query.Add($"{name}={Uri.EscapeDataString(value.Value.ToString("O"))}");
            }
        }
    }
}
