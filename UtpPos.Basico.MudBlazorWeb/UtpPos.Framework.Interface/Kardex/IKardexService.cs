using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Kardex;

namespace UtpPos.Framework.Interface.Kardex
{
    public interface IKardexService
    {
        Task<Response<IReadOnlyList<KardexDto>>> ListAsync(KardexListFilterDto filter);
        Task<Response<KardexDto>> GetByIdAsync(int id);
        Task<Response<IReadOnlyList<KardexDto>>> ListByProductAsync(int productId);
    }
}
