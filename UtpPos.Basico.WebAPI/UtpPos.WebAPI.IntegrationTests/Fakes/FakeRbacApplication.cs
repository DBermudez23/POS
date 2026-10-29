using System.Collections.Concurrent;
using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Transversal.Common;
using UtpPos.WebAPI.IntegrationTests.Auth;
using Microsoft.AspNetCore.Http;

namespace UtpPos.WebAPI.IntegrationTests.Fakes;

public sealed class FakeRbacApplication : IRbacApplication
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ConcurrentQueue<string> _requestedPermissions = new();

    public FakeRbacApplication(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IReadOnlyCollection<string> RequestedPermissions => _requestedPermissions.ToArray();

    public Task<Response<bool>> UserHasPermissionAsync(int userId, string permissionName)
    {
        _requestedPermissions.Enqueue(permissionName);

        var hasPermission = _httpContextAccessor.HttpContext?.User
            .FindAll(TestAuthHandler.PermissionClaimType)
            .Any(claim => string.Equals(claim.Value, permissionName, StringComparison.OrdinalIgnoreCase)) == true;

        return Task.FromResult(new Response<bool>
        {
            IsSuccess = true,
            Data = hasPermission,
            Message = hasPermission ? "OK" : "Forbidden"
        });
    }

    public Task<Response<IEnumerable<RbacModuleDto>>> ListModulesAsync(RbacModuleFilterDto filter) => NotUsed<IEnumerable<RbacModuleDto>>();
    public Task<Response<int>> CreateModuleAsync(RbacModuleCreateDto dto) => NotUsed<int>();
    public Task<Response<IEnumerable<RbacActionDto>>> ListActionsAsync(RbacActionFilterDto filter) => NotUsed<IEnumerable<RbacActionDto>>();
    public Task<Response<int>> CreateActionAsync(RbacActionCreateDto dto) => NotUsed<int>();
    public Task<Response<IEnumerable<RbacPermissionDto>>> ListPermissionsAsync(RbacPermissionFilterDto filter) => NotUsed<IEnumerable<RbacPermissionDto>>();
    public Task<Response<RbacPermissionCreateResultDto>> CreatePermissionAsync(RbacPermissionCreateDto dto) => NotUsed<RbacPermissionCreateResultDto>();
    public Task<Response<IEnumerable<RbacRoleDto>>> GetRolesAsync(RbacRoleFilterDto filter) => NotUsed<IEnumerable<RbacRoleDto>>();
    public Task<Response<RbacRoleDto>> GetRoleByIdAsync(int roleId) => NotUsed<RbacRoleDto>();
    public Task<Response<RbacRoleCreateResultDto>> CreateRoleAsync(RbacRoleCreateDto request) => NotUsed<RbacRoleCreateResultDto>();
    public Task<Response<bool>> UpdateRoleAsync(RbacRoleUpdateDto request) => NotUsed<bool>();
    public Task<Response<bool>> DeactivateRoleAsync(int roleId) => NotUsed<bool>();
    public Task<Response<IEnumerable<RbacRolePermissionDto>>> GetRolePermissionsAsync(RbacRolePermissionFilterDto filter) => NotUsed<IEnumerable<RbacRolePermissionDto>>();
    public Task<Response<bool>> AssignPermissionToRoleAsync(RbacAssignPermissionToRoleDto request) => NotUsed<bool>();
    public Task<Response<bool>> RevokePermissionFromRoleAsync(RbacRevokePermissionFromRoleDto request) => NotUsed<bool>();
    public Task<Response<IEnumerable<RbacUserRoleDto>>> GetUserRolesAsync(RbacUserRoleFilterDto filter) => NotUsed<IEnumerable<RbacUserRoleDto>>();
    public Task<Response<bool>> AssignRoleToUserAsync(RbacAssignRoleToUserDto request) => NotUsed<bool>();
    public Task<Response<bool>> RevokeRoleFromUserAsync(RbacRevokeRoleFromUserDto request) => NotUsed<bool>();
    public Task<Response<IEnumerable<RbacUserPermissionDto>>> GetUserPermissionsAsync(int userId) => NotUsed<IEnumerable<RbacUserPermissionDto>>();

    private static Task<Response<T>> NotUsed<T>()
    {
        return Task.FromResult(new Response<T>
        {
            IsSuccess = false,
            Data = default,
            Message = "Not used by authorization integration tests."
        });
    }
}
