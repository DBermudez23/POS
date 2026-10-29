using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Authorization;

namespace UtpPos.Framework.Interface.Authorization
{
    public interface IPermissionService
    {
        Task<Response<IReadOnlyList<UserPermissionDto>>> GetCurrentUserPermissionsAsync();
    }
}
