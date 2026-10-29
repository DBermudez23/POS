using UtpPos.Framework.Application.DTO.Kardex;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Kardex
{
    public interface IKardexApplication
    {
        Task<Response<IEnumerable<KardexDto>>> ListAsync(KardexListFilterDto filter);

        Task<Response<IEnumerable<KardexDto>>> ListByProductAsync(int productId);

        Task<Response<KardexDto>> GetByIdAsync(int kardexId);
    }
}
