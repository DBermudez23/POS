using UtpPos.Framework.Domain.Core.Common;
using DomainProduct = UtpPos.Framework.Domain.Entity.Product.Product;
using DomainProductFilter = UtpPos.Framework.Domain.Entity.Product.ProductFilter;
using DomainProductUpdate = UtpPos.Framework.Domain.Entity.Product.ProductUpdate;
using UtpPos.Framework.Domain.Interface.Product;
using UtpPos.Framework.InfraStructure.Interface.Product;

namespace UtpPos.Framework.Domain.Core.Product
{
    public class ProductDomain : IProductDomain
    {
        private readonly IProductRepository _repository;

        public ProductDomain(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DomainProduct>> ListAsync(DomainProductFilter filter)
        {
            filter ??= new DomainProductFilter();

            if (filter.BusinessId.HasValue)
                DomainValidation.EnsurePositiveId(filter.BusinessId.Value, "BusinessId");

            if (filter.CategoryId.HasValue)
                DomainValidation.EnsurePositiveId(filter.CategoryId.Value, "CategoryId");

            DomainValidation.EnsureActiveInactiveStatus(filter.Status);
            filter.Status = DomainValidation.NormalizeStatus(filter.Status);

            return await _repository.ListAsync(filter);
        }

        public async Task<DomainProduct?> GetByIdAsync(int productId)
        {
            DomainValidation.EnsurePositiveId(productId, "ProductId");
            return await _repository.GetByIdAsync(productId);
        }

        public async Task<int> CreateAsync(DomainProduct product)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            DomainValidation.EnsureRequired(product.Name, "Name");
            DomainValidation.EnsureRequired(product.Description, "Description");
            DomainValidation.EnsurePositiveId(product.CategoryId, "CategoryId");

            if (!product.BusinessId.HasValue)
                throw new ArgumentException("BusinessId is required.");

            DomainValidation.EnsurePositiveId(product.BusinessId.Value, "BusinessId");
            DomainValidation.EnsurePositiveId(product.CreatedByUserId, "CreatedByUserId");
            DomainValidation.EnsureNonNegativeAmount(product.VatPercentage, "VatPercentage");
            DomainValidation.EnsureNonNegativeAmount(product.ProfitMarginPercentage, "ProfitMarginPercentage");
            DomainValidation.EnsureActiveInactiveStatus(product.Status, required: true);
            product.SaleValue = null;
            product.Status = DomainValidation.NormalizeStatus(product.Status) ?? "A";

            return await _repository.CreateAsync(product);
        }

        public async Task<int> UpdateAsync(DomainProductUpdate product)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            DomainValidation.EnsurePositiveId(product.Id, "ProductId");

            if (product.CategoryId.HasValue)
                DomainValidation.EnsurePositiveId(product.CategoryId.Value, "CategoryId");

            if (product.BusinessId.HasValue)
                DomainValidation.EnsurePositiveId(product.BusinessId.Value, "BusinessId");

            if (product.ClearBusiness)
                throw new ArgumentException("BusinessId is required.");

            if (product.VatPercentage.HasValue)
                DomainValidation.EnsureNonNegativeAmount(product.VatPercentage.Value, "VatPercentage");

            if (product.ProfitMarginPercentage.HasValue)
                DomainValidation.EnsureNonNegativeAmount(product.ProfitMarginPercentage.Value, "ProfitMarginPercentage");

            DomainValidation.EnsureActiveInactiveStatus(product.Status);
            product.Status = DomainValidation.NormalizeStatus(product.Status);

            return await _repository.UpdateAsync(product);
        }

        public async Task<int> DeleteAsync(int productId)
        {
            DomainValidation.EnsurePositiveId(productId, "ProductId");
            return await _repository.DeleteAsync(productId);
        }
    }
}
