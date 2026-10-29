using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Purchase;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Purchase;

namespace UtpPos.Framework.Main.Purchase
{
    public sealed class PurchaseService : IPurchaseService
    {
        private const string Endpoint = "api/compras";

        private readonly IRestApi _restApi;

        public PurchaseService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<PurchaseDto>>> ListAsync(PurchaseListFilterDto? filter = null)
        {
            filter ??= new PurchaseListFilterDto();

            var response = await _restApi.GetAsync<IEnumerable<PurchaseDto>>(
                BuildListEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<PurchaseDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                StatusCode = response.StatusCode,
                Data = response.Data?.ToArray() ?? Array.Empty<PurchaseDto>()
            };
        }

        public Task<Response<PurchaseDetailDto>> GetByIdAsync(int id) =>
            _restApi.GetAsync<PurchaseDetailDto>($"{Endpoint}/{id}", authorized: true);

        public Task<Response<PurchaseWriteResultDto>> CreateAsync(PurchaseCreateDto request) =>
            _restApi.PostAsync<PurchaseCreateDto, PurchaseWriteResultDto>(Endpoint, request, authorized: true);

        public Task<Response<PurchaseCancelResultDto>> CancelAsync(int id, PurchaseCancelDto request) =>
            _restApi.PostAsync<PurchaseCancelDto, PurchaseCancelResultDto>($"{Endpoint}/{id}/anular", request, authorized: true);

        private static string BuildListEndpoint(PurchaseListFilterDto filter)
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
