using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Inventory;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Inventory;

namespace UtpPos.Framework.Main.Inventory
{
    public sealed class InventoryService : IInventoryService
    {
        private const string Endpoint = "api/inventario";

        private readonly IRestApi _restApi;

        public InventoryService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<InventoryDto>>> ListAsync(InventoryListFilterDto? filter = null)
        {
            filter ??= new InventoryListFilterDto();

            var response = await _restApi.GetAsync<IEnumerable<InventoryDto>>(
                BuildListEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<InventoryDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<InventoryDto>()
            };
        }

        public Task<Response<InventoryDto>> GetByIdAsync(int id) =>
            _restApi.GetAsync<InventoryDto>($"{Endpoint}/{id}", authorized: true);

        public async Task<Response<IReadOnlyList<InventoryDto>>> ListByProductAsync(int productId)
        {
            var response = await _restApi.GetAsync<IEnumerable<InventoryDto>>(
                $"{Endpoint}/producto/{productId}",
                authorized: true);

            return new Response<IReadOnlyList<InventoryDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<InventoryDto>()
            };
        }

        public Task<Response<InventoryMovementResultDto>> RegisterMovementAsync(InventoryMovementCreateDto request) =>
            _restApi.PostAsync<InventoryMovementCreateDto, InventoryMovementResultDto>(
                $"{Endpoint}/movimientos",
                request,
                authorized: true);

        private static string BuildListEndpoint(InventoryListFilterDto filter)
        {
            var query = new List<string>();

            AddQueryValue(query, nameof(filter.BusinessId), filter.BusinessId);
            AddQueryValue(query, nameof(filter.ProductId), filter.ProductId);
            AddQueryValue(query, nameof(filter.Status), filter.Status);
            AddQueryValue(query, nameof(filter.ProductName), filter.ProductName);

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
    }
}
