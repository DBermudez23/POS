using UtpPos.Framework.Application.DTO.Product;
using UtpPos.Framework.Application.Interface.Product;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.WebAPI.IntegrationTests.Fakes;

public sealed class FakeProductApplication : IProductApplication
{
    public int ListCalls { get; private set; }
    public int CreateCalls { get; private set; }
    public ProductListFilterDto? LastListFilter { get; private set; }
    public ProductCreateDto? LastCreateDto { get; private set; }

    public Task<Response<IEnumerable<ProductDto>>> ListAsync(ProductListFilterDto filter)
    {
        ListCalls++;
        LastListFilter = filter;

        return Task.FromResult(Success<IEnumerable<ProductDto>>(Array.Empty<ProductDto>()));
    }

    public Task<Response<int>> CreateAsync(ProductCreateDto dto)
    {
        CreateCalls++;
        LastCreateDto = dto;

        return Task.FromResult(Success(123));
    }

    public Task<Response<ProductDto>> GetByIdAsync(int productId)
    {
        return Task.FromResult(Success(new ProductDto { Id = productId, Name = "Producto Test", Status = "A" }));
    }

    public Task<Response<int>> UpdateAsync(int productId, ProductUpdateDto dto)
    {
        return Task.FromResult(Success(productId));
    }

    public Task<Response<int>> DeleteAsync(int productId)
    {
        return Task.FromResult(Success(productId));
    }

    private static Response<T> Success<T>(T data)
    {
        return new Response<T>
        {
            IsSuccess = true,
            Data = data,
            Message = "OK"
        };
    }
}
