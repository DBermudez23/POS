using DomainProduct = UtpPos.Framework.Domain.Entity.Product.Product;
using DomainProductFilter = UtpPos.Framework.Domain.Entity.Product.ProductFilter;
using DomainProductUpdate = UtpPos.Framework.Domain.Entity.Product.ProductUpdate;

namespace UtpPos.Framework.InfraStructure.Interface.Product
{
    public interface IProductRepository
    {
        Task<IEnumerable<DomainProduct>> ListAsync(DomainProductFilter filter);
        Task<DomainProduct?> GetByIdAsync(int productId);
        Task<int> CreateAsync(DomainProduct product);
        Task<int> UpdateAsync(DomainProductUpdate product);
        Task<int> DeleteAsync(int productId);
    }
}