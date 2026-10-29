using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Authorization;

namespace UtpPos.Framework.Interface.Authorization
{
    public interface ICurrentUserRoleService
    {
        Task<Response<IReadOnlyList<CurrentUserRoleDto>>> GetCurrentUserRolesAsync();
    }
}
