using UtpPos.Framework.Application.DTO.Product;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Product
{
    public interface IProductApplication
    {
        Task<Response<IEnumerable<ProductDto>>> ListAsync(ProductListFilterDto filter);
        Task<Response<ProductDto>> GetByIdAsync(int productId);
        Task<Response<int>> CreateAsync(ProductCreateDto dto);
        Task<Response<int>> UpdateAsync(int productId, ProductUpdateDto dto);
        Task<Response<int>> DeleteAsync(int productId);
    }
}