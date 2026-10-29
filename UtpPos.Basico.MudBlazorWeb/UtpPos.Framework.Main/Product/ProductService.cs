using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Product;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Product;

namespace UtpPos.Framework.Main.Product
{
    public class ProductService : IProductService
    {
        private const string Endpoint = "api/productos";

        private readonly IRestApi _restApi;

        public ProductService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<ProductDto>>> ListAsync(ProductListFilterDto? filter = null)
        {
            filter ??= new ProductListFilterDto();

            var response = await _restApi.GetAsync<IEnumerable<ProductDto>>(
                BuildListEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<ProductDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                StatusCode = response.StatusCode,
                Data = response.Data?.ToList() ?? new List<ProductDto>()
            };
        }

        public Task<Response<ProductDto>> GetByIdAsync(int id)
        {
            return _restApi.GetAsync<ProductDto>(
                $"{Endpoint}/{id}",
                authorized: true);
        }

        public Task<Response<int>> CreateAsync(ProductCreateDto request)
        {
            return _restApi.PostAsync<ProductCreateDto, int>(
                Endpoint,
                request,
                authorized: true);
        }

        public Task<Response<int>> UpdateAsync(int id, ProductUpdateDto request)
        {
            return _restApi.PatchAsync<ProductUpdateDto, int>(
                $"{Endpoint}/{id}",
                request,
                authorized: true);
        }

        public Task<Response<int>> DeleteAsync(int id)
        {
            return _restApi.DeleteAsync<int>(
                $"{Endpoint}/{id}",
                authorized: true);
        }

        private static string BuildListEndpoint(ProductListFilterDto filter)
        {
            var query = new List<string>();

            AddQueryValue(query, nameof(filter.BusinessId), filter.BusinessId);
            AddQueryValue(query, nameof(filter.CategoryId), filter.CategoryId);
            AddQueryValue(query, nameof(filter.Status), filter.Status);
            AddQueryValue(query, nameof(filter.Name), filter.Name);
            AddQueryValue(query, nameof(filter.ExternalId), filter.ExternalId);

            return query.Count == 0
                ? Endpoint
                : $"{Endpoint}?{string.Join("&", query)}";
        }

        private static void AddQueryValue(ICollection<string> query, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
            }
        }

        private static void AddQueryValue(ICollection<string> query, string name, int? value)
        {
            if (value.HasValue)
            {
                query.Add($"{name}={value.Value}");
            }
        }
    }
}
