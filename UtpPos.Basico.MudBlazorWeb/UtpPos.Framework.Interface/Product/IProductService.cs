using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Product;

namespace UtpPos.Framework.Interface.Product
{
    public interface IProductService
    {
        Task<Response<IReadOnlyList<ProductDto>>> ListAsync(ProductListFilterDto filter);
        Task<Response<ProductDto>> GetByIdAsync(int id);
        Task<Response<int>> CreateAsync(ProductCreateDto request);
        Task<Response<int>> UpdateAsync(int id, ProductUpdateDto request);
        Task<Response<int>> DeleteAsync(int id);
    }
}