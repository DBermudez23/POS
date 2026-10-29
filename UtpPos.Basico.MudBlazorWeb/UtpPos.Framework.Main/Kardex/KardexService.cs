using System.Globalization;
using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Kardex;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Kardex;

namespace UtpPos.Framework.Main.Kardex
{
    public sealed class KardexService : IKardexService
    {
        private const string Endpoint = "api/kardex";

        private readonly IRestApi _restApi;

        public KardexService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<KardexDto>>> ListAsync(KardexListFilterDto? filter = null)
        {
            filter ??= new KardexListFilterDto();

            var response = await _restApi.GetAsync<IEnumerable<KardexDto>>(
                BuildListEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<KardexDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<KardexDto>()
            };
        }

        public Task<Response<KardexDto>> GetByIdAsync(int id) =>
            _restApi.GetAsync<KardexDto>($"{Endpoint}/{id}", authorized: true);

        public async Task<Response<IReadOnlyList<KardexDto>>> ListByProductAsync(int productId)
        {
            var response = await _restApi.GetAsync<IEnumerable<KardexDto>>(
                $"{Endpoint}/producto/{productId}",
                authorized: true);

            return new Response<IReadOnlyList<KardexDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<KardexDto>()
            };
        }

        private static string BuildListEndpoint(KardexListFilterDto filter)
        {
            var query = new List<string>();

            AddQueryValue(query, nameof(filter.BusinessId), filter.BusinessId);
            AddQueryValue(query, nameof(filter.InventoryId), filter.InventoryId);
            AddQueryValue(query, nameof(filter.ProductId), filter.ProductId);
            AddQueryValue(query, nameof(filter.MovementTypeId), filter.MovementTypeId);
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
                query.Add($"{name}={value.Value.ToString("yyyy-MM-dd", new CultureInfo("es-CO"))}");
            }
        }
    }
}
