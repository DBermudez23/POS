using UtpPos.Framework.Domain.Core.Common;
using UtpPos.Framework.Domain.Interface.Kardex;
using UtpPos.Framework.InfraStructure.Interface.Kardex;
using DomainKardex = UtpPos.Framework.Domain.Entity.Kardex.Kardex;
using DomainKardexFilter = UtpPos.Framework.Domain.Entity.Kardex.KardexFilter;

namespace UtpPos.Framework.Domain.Core.Kardex
{
    public sealed class KardexDomain : IKardexDomain
    {
        private readonly IKardexRepository _repository;

        public KardexDomain(IKardexRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<DomainKardex>> ListAsync(DomainKardexFilter filter)
        {
            filter ??= new DomainKardexFilter();

            if (filter.BusinessId.HasValue)
                DomainValidation.EnsurePositiveId(filter.BusinessId.Value, "BusinessId");

            if (filter.InventoryId.HasValue)
                DomainValidation.EnsurePositiveId(filter.InventoryId.Value, "InventoryId");

            if (filter.ProductId.HasValue)
                DomainValidation.EnsurePositiveId(filter.ProductId.Value, "ProductId");

            if (filter.MovementTypeId.HasValue)
                DomainValidation.EnsurePositiveId(filter.MovementTypeId.Value, "MovementTypeId");

            if (filter.StartDate.HasValue && filter.EndDate.HasValue && filter.StartDate.Value > filter.EndDate.Value)
                throw new ArgumentException("StartDate cannot be greater than EndDate.");

            DomainValidation.EnsureActiveInactiveStatus(filter.Status);
            filter.Status = DomainValidation.NormalizeStatus(filter.Status);

            return _repository.ListAsync(filter);
        }

        public Task<DomainKardex?> GetByIdAsync(int kardexId)
        {
            DomainValidation.EnsurePositiveId(kardexId, "KardexId");
            return _repository.GetByIdAsync(kardexId);
        }
    }
}
