using DomainSale = UtpPos.Framework.Domain.Entity.Sale.Sale;
using DomainSaleCancel = UtpPos.Framework.Domain.Entity.Sale.SaleCancel;
using DomainSaleCancelResult = UtpPos.Framework.Domain.Entity.Sale.SaleCancelResult;
using DomainSaleCreate = UtpPos.Framework.Domain.Entity.Sale.SaleCreate;
using DomainSaleFilter = UtpPos.Framework.Domain.Entity.Sale.SaleFilter;
using DomainSaleWriteResult = UtpPos.Framework.Domain.Entity.Sale.SaleWriteResult;

namespace UtpPos.Framework.InfraStructure.Interface.Sale
{
    public interface ISaleRepository
    {
        Task<IEnumerable<DomainSale>> ListAsync(DomainSaleFilter filter);
        Task<DomainSale?> GetByIdAsync(int saleId);
        Task<DomainSaleWriteResult> CreateAsync(DomainSaleCreate sale);
        Task<DomainSaleCancelResult> CancelAsync(DomainSaleCancel cancellation);
    }
}
