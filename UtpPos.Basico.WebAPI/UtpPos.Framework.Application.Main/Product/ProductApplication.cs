using AutoMapper;
using UtpPos.Framework.Application.DTO.Product;
using UtpPos.Framework.Application.Interface.Product;
using UtpPos.Framework.Domain.Interface.Product;
using UtpPos.Framework.Transversal.Common;
using DomainProduct = UtpPos.Framework.Domain.Entity.Product.Product;
using DomainProductFilter = UtpPos.Framework.Domain.Entity.Product.ProductFilter;
using DomainProductUpdate = UtpPos.Framework.Domain.Entity.Product.ProductUpdate;

namespace UtpPos.Framework.Application.Main.Product
{
    public class ProductApplication : IProductApplication
    {
        private readonly IProductDomain _domain;
        private readonly IMapper _mapper;

        public ProductApplication(IProductDomain domain, IMapper mapper)
        {
            _domain = domain;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ProductDto>>> ListAsync(ProductListFilterDto filter)
        {
            try
            {
                filter ??= new ProductListFilterDto();

                var domainFilter = new DomainProductFilter
                {
                    BusinessId = filter.BusinessId,
                    CategoryId = filter.CategoryId,
                    Status = filter.Status,
                    Name = filter.Name,
                    ExternalId = filter.ExternalId
                };

                var products = await _domain.ListAsync(domainFilter);
             
                return Success(_mapper.Map<IEnumerable<ProductDto>>(products), "Products listed successfully.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<ProductDto>>(ex.Message);
            }
        }

        public async Task<Response<ProductDto>> GetByIdAsync(int productId)
        {
            try
            {
                var product = await _domain.GetByIdAsync(productId);

                if (product is null)
                    return Fail<ProductDto>("Product was not found.");

                return Success(_mapper.Map<ProductDto>(product), "Product found successfully.");
            }
            catch (Exception ex)
            {
                return Fail<ProductDto>(ex.Message);
            }
        }

        public async Task<Response<int>> CreateAsync(ProductCreateDto dto)
        {
            try
            {
                var product = _mapper.Map<DomainProduct>(dto);

                var id = await _domain.CreateAsync(product);

                return Success(id, "Product created successfully.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
            }
        }

        public async Task<Response<int>> UpdateAsync(int productId, ProductUpdateDto dto)
        {
            try
            {
                var product = _mapper.Map<DomainProductUpdate>(dto);
                product.Id = productId;

                var updatedId = await _domain.UpdateAsync(product);

                return Success(updatedId, "Product updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
            }
        }

        public async Task<Response<int>> DeleteAsync(int productId)
        {
            try
            {
                var deletedId = await _domain.DeleteAsync(productId);

                return Success(deletedId, "Product deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
            }
        }

        private static Response<T> Success<T>(T data, string message)
        {
            return new Response<T>
            { IsSuccess = true, Data = data, Message = message };
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T>
            { IsSuccess = false, Data = default, Message = message };
        }
    }
}