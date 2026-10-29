using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Rbac
{
    public interface IRbacApplication
    {
        Task<Response<IEnumerable<RbacModuleDto>>> ListModulesAsync(RbacModuleFilterDto filter);
        Task<Response<int>> CreateModuleAsync(RbacModuleCreateDto dto);
        Task<Response<IEnumerable<RbacActionDto>>> ListActionsAsync(RbacActionFilterDto filter);
        Task<Response<int>> CreateActionAsync(RbacActionCreateDto dto);
        Task<Response<IEnumerable<RbacPermissionDto>>> ListPermissionsAsync(RbacPermissionFilterDto filter);
        Task<Response<RbacPermissionCreateResultDto>> CreatePermissionAsync(RbacPermissionCreateDto dto);
        Task<Response<IEnumerable<RbacRoleDto>>> GetRolesAsync(RbacRoleFilterDto filter);
        Task<Response<RbacRoleDto>> GetRoleByIdAsync(int roleId);
        Task<Response<RbacRoleCreateResultDto>> CreateRoleAsync(RbacRoleCreateDto request);
        Task<Response<bool>> UpdateRoleAsync(RbacRoleUpdateDto request);
        Task<Response<bool>> DeactivateRoleAsync(int roleId);
        Task<Response<IEnumerable<RbacRolePermissionDto>>> GetRolePermissionsAsync(RbacRolePermissionFilterDto filter);
        Task<Response<bool>> AssignPermissionToRoleAsync(RbacAssignPermissionToRoleDto request);
        Task<Response<bool>> RevokePermissionFromRoleAsync(RbacRevokePermissionFromRoleDto request);
        Task<Response<IEnumerable<RbacUserRoleDto>>> GetUserRolesAsync(RbacUserRoleFilterDto filter);
        Task<Response<bool>> AssignRoleToUserAsync(RbacAssignRoleToUserDto request);
        Task<Response<bool>> RevokeRoleFromUserAsync(RbacRevokeRoleFromUserDto request);
        Task<Response<IEnumerable<RbacUserPermissionDto>>> GetUserPermissionsAsync(int userId);
        Task<Response<bool>> UserHasPermissionAsync(int userId, string permissionName);
    }
}
