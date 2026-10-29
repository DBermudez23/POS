using UtpPos.Framework.Domain.Entity.Rbac;

namespace UtpPos.Framework.InfraStructure.Interface.Rbac
{
    public interface IRbacRepository
    {
        Task<IEnumerable<RbacModule>> ListModulesAsync(string? code, string? name, string? status);
        Task<int> CreateModuleAsync(RbacModule module);
        Task<IEnumerable<RbacAction>> ListActionsAsync(string? code, string? name, string? status);
        Task<int> CreateActionAsync(RbacAction action);
        Task<IEnumerable<RbacPermission>> ListPermissionsAsync(
            string? permissionName,
            string? moduleCode,
            string? actionCode,
            string? status);
        Task<(int PermissionId, string PermissionName)> CreatePermissionAsync(RbacPermission permission);
        Task<IEnumerable<RbacRole>> GetRolesAsync(
            int? businessId,
            string? name,
            string? status,
            bool? onlyGlobal);
        Task<RbacRole?> GetRoleByIdAsync(int roleId);
        Task<int> CreateRoleAsync(RbacRole role);
        Task<bool> UpdateRoleAsync(RbacRole role);
        Task<bool> DeactivateRoleAsync(int roleId);
        Task<IEnumerable<RbacRolePermission>> GetRolePermissionsAsync(
            int? roleId,
            int? permissionId,
            string? permissionName,
            string? status);
        Task<int> AssignPermissionToRoleAsync(int roleId, int permissionId, int? assignedByUserId);
        Task<bool> RevokePermissionFromRoleAsync(int roleId, int permissionId);
        Task<IEnumerable<RbacUserRole>> GetUserRolesAsync(
            int? userId,
            int? roleId,
            string? userEmail,
            string? roleName,
            string? status);
        Task<int> AssignRoleToUserAsync(int userId, int roleId, int? assignedByUserId);
        Task<bool> RevokeRoleFromUserAsync(int userId, int roleId);
        Task<IEnumerable<RbacUserPermission>> GetUserPermissionsAsync(int userId);
    }
}
