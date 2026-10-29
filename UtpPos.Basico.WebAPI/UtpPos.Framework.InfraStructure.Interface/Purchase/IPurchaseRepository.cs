using DomainPurchase = UtpPos.Framework.Domain.Entity.Purchase.Purchase;
using DomainPurchaseCancel = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCancel;
using DomainPurchaseCancelResult = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCancelResult;
using DomainPurchaseCreate = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreate;
using DomainPurchaseFilter = UtpPos.Framework.Domain.Entity.Purchase.PurchaseFilter;
using DomainPurchaseWriteResult = UtpPos.Framework.Domain.Entity.Purchase.PurchaseWriteResult;

namespace UtpPos.Framework.InfraStructure.Interface.Purchase
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<DomainPurchase>> ListAsync(DomainPurchaseFilter filter);
        Task<DomainPurchase?> GetByIdAsync(int purchaseId);
        Task<DomainPurchaseWriteResult> CreateAsync(DomainPurchaseCreate purchase);
        Task<DomainPurchaseCancelResult> CancelAsync(DomainPurchaseCancel cancellation);
    }
}
