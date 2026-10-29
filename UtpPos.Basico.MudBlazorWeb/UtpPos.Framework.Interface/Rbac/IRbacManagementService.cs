using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Rbac;

namespace UtpPos.Framework.Interface.Rbac
{
    public interface IRbacManagementService
    {
        Task<Response<IReadOnlyList<RbacModuleDto>>> ListModulesAsync();
        Task<Response<IReadOnlyList<RbacActionDto>>> ListActionsAsync();
        Task<Response<IReadOnlyList<RbacPermissionDto>>> ListPermissionsAsync(RbacPermissionFilterDto filter);
        Task<Response<IReadOnlyList<RbacRoleDto>>> ListRolesAsync(RbacRoleFilterDto filter);
        Task<Response<RbacRoleDto>> GetRoleByIdAsync(int roleId);
        Task<Response<RbacRoleCreateResultDto>> CreateRoleAsync(RbacRoleCreateDto request);
        Task<Response<bool>> UpdateRoleAsync(int roleId, RbacRoleUpdateDto request);
        Task<Response<bool>> DeleteRoleAsync(int roleId);
        Task<Response<IReadOnlyList<RbacRolePermissionDto>>> ListRolePermissionsAsync(int roleId);
        Task<Response<bool>> AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task<Response<bool>> RevokePermissionFromRoleAsync(int roleId, int permissionId);
        Task<Response<IReadOnlyList<RbacUserRoleDto>>> ListUserRolesAsync(int userId);
        Task<Response<bool>> AssignRoleToUserAsync(int userId, int roleId);
        Task<Response<bool>> RevokeRoleFromUserAsync(int userId, int roleId);
    }
}
