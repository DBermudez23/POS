using DomainKardex = UtpPos.Framework.Domain.Entity.Kardex.Kardex;
using DomainKardexFilter = UtpPos.Framework.Domain.Entity.Kardex.KardexFilter;

namespace UtpPos.Framework.InfraStructure.Interface.Kardex
{
    public interface IKardexRepository
    {
        Task<IEnumerable<DomainKardex>> ListAsync(DomainKardexFilter filter);

        Task<DomainKardex?> GetByIdAsync(int kardexId);
    }
}
