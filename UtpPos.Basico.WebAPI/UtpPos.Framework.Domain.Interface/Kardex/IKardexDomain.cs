using DomainKardex = UtpPos.Framework.Domain.Entity.Kardex.Kardex;
using DomainKardexFilter = UtpPos.Framework.Domain.Entity.Kardex.KardexFilter;

namespace UtpPos.Framework.Domain.Interface.Kardex
{
    public interface IKardexDomain
    {
        Task<IEnumerable<DomainKardex>> ListAsync(DomainKardexFilter filter);

        Task<DomainKardex?> GetByIdAsync(int kardexId);
    }
}
