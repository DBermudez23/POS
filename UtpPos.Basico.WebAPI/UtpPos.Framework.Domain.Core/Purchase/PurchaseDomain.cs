using UtpPos.Framework.Domain.Core.Common;
using UtpPos.Framework.Domain.Interface.Purchase;
using UtpPos.Framework.InfraStructure.Interface.Product;
using UtpPos.Framework.InfraStructure.Interface.Purchase;
using UtpPos.Framework.InfraStructure.Interface.ThirdParty;
using DomainPurchase = UtpPos.Framework.Domain.Entity.Purchase.Purchase;
using DomainPurchaseCancel = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCancel;
using DomainPurchaseCancelResult = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCancelResult;
using DomainPurchaseCreate = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreate;
using DomainPurchaseFilter = UtpPos.Framework.Domain.Entity.Purchase.PurchaseFilter;
using DomainPurchaseWriteResult = UtpPos.Framework.Domain.Entity.Purchase.PurchaseWriteResult;

namespace UtpPos.Framework.Domain.Core.Purchase
{
    public sealed class PurchaseDomain : IPurchaseDomain
    {
        private readonly IPurchaseRepository _repository;
        private readonly IThirdPartyRepository _thirdPartyRepository;
        private readonly IProductRepository _productRepository;

        public PurchaseDomain(IPurchaseRepository repository, IThirdPartyRepository thirdPartyRepository, IProductRepository productRepository)
        {
            _repository = repository;
            _thirdPartyRepository = thirdPartyRepository;
            _productRepository = productRepository;
        }

        public Task<IEnumerable<DomainPurchase>> ListAsync(DomainPurchaseFilter filter)
        {
            filter ??= new DomainPurchaseFilter();

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

        public Task<DomainPurchase?> GetByIdAsync(int purchaseId)
        {
            DomainValidation.EnsurePositiveId(purchaseId, "PurchaseId");
            return _repository.GetByIdAsync(purchaseId);
        }

        public async Task<DomainPurchaseWriteResult> CreateAsync(DomainPurchaseCreate purchase)
        {
            if (purchase is null)
                throw new ArgumentNullException(nameof(purchase));

            DomainValidation.EnsurePositiveId(purchase.BusinessId, "BusinessId");
            DomainValidation.EnsurePositiveId(purchase.ThirdPartyId, "ThirdPartyId");
            DomainValidation.EnsurePositiveId(purchase.UserId, "UserId");
            DomainValidation.EnsurePositiveId(purchase.KardexMovementTypeId, "KardexMovementTypeId");

            if (!await _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId))
                throw new ArgumentException("El proveedor no está asociado al negocio de la compra.");

            if (purchase.Details is null || purchase.Details.Count == 0)
                throw new ArgumentException("Purchase must include at least one detail.");

            if (purchase.Payments is null || purchase.Payments.Count == 0)
                throw new ArgumentException("Purchase must include at least one payment.");

            foreach (var detail in purchase.Details)
            {
                DomainValidation.EnsurePositiveId(detail.ProductId, "ProductId");
                DomainValidation.EnsurePositiveAmount(detail.Quantity, "Quantity");
                DomainValidation.EnsurePositiveAmount(detail.UnitValue, "UnitValue");
                DomainValidation.EnsureNonNegativeAmount(detail.VatPercentage, "VatPercentage");

                var product = await _productRepository.GetByIdAsync(detail.ProductId)
                    ?? throw new ArgumentException("Producto no encontrado para el negocio de la compra.");

                if (product.BusinessId != purchase.BusinessId)
                    throw new ArgumentException("Producto no encontrado para el negocio de la compra.");
            }

            foreach (var payment in purchase.Payments)
            {
                DomainValidation.EnsurePositiveId(payment.PaymentMethodId, "PaymentMethodId");
                DomainValidation.EnsureNonNegativeAmount(payment.ReceivedValue, "ReceivedValue");
                DomainValidation.EnsureNonNegativeAmount(payment.ChangeValue, "ChangeValue");

                if (payment.ChangeValue > payment.ReceivedValue)
                    throw new ArgumentException("ChangeValue cannot be greater than ReceivedValue.");
            }

            return await _repository.CreateAsync(purchase);
        }

        public Task<DomainPurchaseCancelResult> CancelAsync(DomainPurchaseCancel cancellation)
        {
            if (cancellation is null)
                throw new ArgumentNullException(nameof(cancellation));

            DomainValidation.EnsurePositiveId(cancellation.PurchaseId, "PurchaseId");
            DomainValidation.EnsurePositiveId(cancellation.UserId, "UserId");
            DomainValidation.EnsurePositiveId(cancellation.CancellationKardexMovementTypeId, "CancellationKardexMovementTypeId");

            return _repository.CancelAsync(cancellation);
        }
    }
}
