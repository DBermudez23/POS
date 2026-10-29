using UtpPos.Framework.Domain.Core.Common;
using UtpPos.Framework.Domain.Interface.Sale;
using UtpPos.Framework.InfraStructure.Interface.Product;
using UtpPos.Framework.InfraStructure.Interface.Sale;
using UtpPos.Framework.InfraStructure.Interface.ThirdParty;
using DomainSale = UtpPos.Framework.Domain.Entity.Sale.Sale;
using DomainSaleCancel = UtpPos.Framework.Domain.Entity.Sale.SaleCancel;
using DomainSaleCancelResult = UtpPos.Framework.Domain.Entity.Sale.SaleCancelResult;
using DomainSaleCreate = UtpPos.Framework.Domain.Entity.Sale.SaleCreate;
using DomainSaleCreateDetail = UtpPos.Framework.Domain.Entity.Sale.SaleCreateDetail;
using DomainSaleFilter = UtpPos.Framework.Domain.Entity.Sale.SaleFilter;
using DomainSaleWriteResult = UtpPos.Framework.Domain.Entity.Sale.SaleWriteResult;

namespace UtpPos.Framework.Domain.Core.Sale
{
    public sealed class SaleDomain : ISaleDomain
    {
        private readonly ISaleRepository _repository;
        private readonly IProductRepository _productRepository;
        private readonly IThirdPartyRepository _thirdPartyRepository;

        public SaleDomain(ISaleRepository repository, IProductRepository productRepository, IThirdPartyRepository thirdPartyRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
            _thirdPartyRepository = thirdPartyRepository;
        }

        public Task<IEnumerable<DomainSale>> ListAsync(DomainSaleFilter filter)
        {
            filter ??= new DomainSaleFilter();

            if (filter.BusinessId.HasValue)
                DomainValidation.EnsurePositiveId(filter.BusinessId.Value, "BusinessId");

            if (filter.ThirdPartyId.HasValue)
                DomainValidation.EnsurePositiveId(filter.ThirdPartyId.Value, "ThirdPartyId");

            if (filter.UserId.HasValue)
                DomainValidation.EnsurePositiveId(filter.UserId.Value, "UserId");

            DomainValidation.EnsureActiveInactiveStatus(filter.Status);
            filter.Status = DomainValidation.NormalizeStatus(filter.Status);

            return _repository.ListAsync(filter);
        }

        public Task<DomainSale?> GetByIdAsync(int saleId)
        {
            DomainValidation.EnsurePositiveId(saleId, "SaleId");
            return _repository.GetByIdAsync(saleId);
        }

        public async Task<DomainSaleWriteResult> CreateAsync(DomainSaleCreate sale)
        {
            if (sale is null)
                throw new ArgumentNullException(nameof(sale));

            DomainValidation.EnsurePositiveId(sale.BusinessId, "BusinessId");
            DomainValidation.EnsurePositiveId(sale.ThirdPartyId, "ThirdPartyId");
            DomainValidation.EnsurePositiveId(sale.UserId, "UserId");
            DomainValidation.EnsurePositiveId(sale.KardexMovementTypeId, "KardexMovementTypeId");

            if (!await _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId))
                throw new ArgumentException("El cliente no está asociado al negocio de la venta.");

            if (sale.Details is null || sale.Details.Count == 0)
                throw new ArgumentException("Sale must include at least one detail.");

            if (sale.Payments is null || sale.Payments.Count == 0)
                throw new ArgumentException("Sale must include at least one payment.");

            foreach (var detail in sale.Details)
            {
                DomainValidation.EnsurePositiveId(detail.ProductId, "ProductId");
                DomainValidation.EnsurePositiveAmount(detail.Quantity, "Quantity");
            }

            sale.Details = await ResolveCalculatedDetailsAsync(sale);

            foreach (var payment in sale.Payments)
            {
                DomainValidation.EnsurePositiveId(payment.PaymentMethodId, "PaymentMethodId");
                DomainValidation.EnsureNonNegativeAmount(payment.ReceivedValue, "ReceivedValue");
                DomainValidation.EnsureNonNegativeAmount(payment.ChangeValue, "ChangeValue");

                if (payment.ChangeValue > payment.ReceivedValue)
                    throw new ArgumentException("ChangeValue cannot be greater than ReceivedValue.");
            }

            return await _repository.CreateAsync(sale);
        }

        private async Task<IReadOnlyList<DomainSaleCreateDetail>> ResolveCalculatedDetailsAsync(DomainSaleCreate sale)
        {
            var products = new Dictionary<int, UtpPos.Framework.Domain.Entity.Product.Product>();
            var details = new List<DomainSaleCreateDetail>(sale.Details.Count);

            foreach (var detail in sale.Details)
            {
                if (!products.TryGetValue(detail.ProductId, out var product))
                {
                    product = await _productRepository.GetByIdAsync(detail.ProductId)
                        ?? throw new ArgumentException("Producto no encontrado.");

                    products.Add(detail.ProductId, product);
                }

                if (product.BusinessId != sale.BusinessId)
                    throw new ArgumentException("Producto no encontrado para el negocio de la venta.");

                var saleValue = product.SaleValue.GetValueOrDefault();
                if (saleValue <= 0)
                    throw new ArgumentException("El producto no tiene valor de venta calculado. Registre una compra o revise el margen del producto.");

                DomainValidation.EnsureNonNegativeAmount(product.VatPercentage, "VatPercentage");

                details.Add(new DomainSaleCreateDetail
                {
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    UnitValue = saleValue,
                    VatPercentage = product.VatPercentage
                });
            }

            return details;
        }

        public Task<DomainSaleCancelResult> CancelAsync(DomainSaleCancel cancellation)
        {
            if (cancellation is null)
                throw new ArgumentNullException(nameof(cancellation));

            DomainValidation.EnsurePositiveId(cancellation.SaleId, "SaleId");
            DomainValidation.EnsurePositiveId(cancellation.UserId, "UserId");
            DomainValidation.EnsurePositiveId(cancellation.CancellationKardexMovementTypeId, "CancellationKardexMovementTypeId");

            return _repository.CancelAsync(cancellation);
        }
    }
}
