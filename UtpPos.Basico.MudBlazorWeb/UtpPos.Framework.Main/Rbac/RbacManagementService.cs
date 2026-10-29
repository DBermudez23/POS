using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Rbac;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Rbac;

namespace UtpPos.Framework.Main.Rbac
{
    public sealed class RbacManagementService : IRbacManagementService
    {
        private const string Endpoint = "api/rbac";

        private readonly IRestApi _restApi;

        public RbacManagementService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<RbacModuleDto>>> ListModulesAsync()
        {
            var response = await _restApi.GetAsync<IEnumerable<RbacModuleDto>>(
                $"{Endpoint}/modules?Status=A",
                authorized: true);

            return ToReadOnlyResponse(response);
        }

        public async Task<Response<IReadOnlyList<RbacActionDto>>> ListActionsAsync()
        {
            var response = await _restApi.GetAsync<IEnumerable<RbacActionDto>>(
                $"{Endpoint}/actions?Status=A",
                authorized: true);

            return new Response<IReadOnlyList<RbacActionDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<RbacActionDto>()
            };
        }

        public async Task<Response<IReadOnlyList<RbacPermissionDto>>> ListPermissionsAsync(RbacPermissionFilterDto filter)
        {
            var response = await _restApi.GetAsync<IEnumerable<RbacPermissionDto>>(
                BuildPermissionEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<RbacPermissionDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<RbacPermissionDto>()
            };
        }

        public async Task<Response<IReadOnlyList<RbacRoleDto>>> ListRolesAsync(RbacRoleFilterDto filter)
        {
            var response = await _restApi.GetAsync<IEnumerable<RbacRoleDto>>(
                BuildRoleEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<RbacRoleDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<RbacRoleDto>()
            };
        }

        public Task<Response<RbacRoleDto>> GetRoleByIdAsync(int roleId) =>
            _restApi.GetAsync<RbacRoleDto>($"{Endpoint}/roles/{roleId}", authorized: true);

        public Task<Response<RbacRoleCreateResultDto>> CreateRoleAsync(RbacRoleCreateDto request) =>
            _restApi.PostAsync<RbacRoleCreateDto, RbacRoleCreateResultDto>(
                $"{Endpoint}/roles",
                request,
                authorized: true);

        public Task<Response<bool>> UpdateRoleAsync(int roleId, RbacRoleUpdateDto request) =>
            _restApi.PatchAsync<RbacRoleUpdateDto, bool>($"{Endpoint}/roles/{roleId}", request, authorized: true);

        public Task<Response<bool>> DeleteRoleAsync(int roleId) =>
            _restApi.DeleteAsync<bool>($"{Endpoint}/roles/{roleId}", authorized: true);

        public async Task<Response<IReadOnlyList<RbacRolePermissionDto>>> ListRolePermissionsAsync(int roleId)
        {
            var response = await _restApi.GetAsync<IEnumerable<RbacRolePermissionDto>>(
                $"{Endpoint}/role-permissions?RoleId={roleId}",
                authorized: true);

            return new Response<IReadOnlyList<RbacRolePermissionDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<RbacRolePermissionDto>()
            };
        }

        public Task<Response<bool>> AssignPermissionToRoleAsync(int roleId, int permissionId) =>
            _restApi.PostAsync<RbacAssignPermissionToRoleDto, bool>(
                $"{Endpoint}/roles/{roleId}/permissions",
                new RbacAssignPermissionToRoleDto { RoleId = roleId, PermissionId = permissionId },
                authorized: true);

        public Task<Response<bool>> RevokePermissionFromRoleAsync(int roleId, int permissionId) =>
            _restApi.DeleteAsync<bool>($"{Endpoint}/roles/{roleId}/permissions/{permissionId}", authorized: true);

        public async Task<Response<IReadOnlyList<RbacUserRoleDto>>> ListUserRolesAsync(int userId)
        {
            var response = await _restApi.GetAsync<IEnumerable<RbacUserRoleDto>>(
                $"{Endpoint}/user-roles?UserId={userId}",
                authorized: true);

            return new Response<IReadOnlyList<RbacUserRoleDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<RbacUserRoleDto>()
            };
        }

        public Task<Response<bool>> AssignRoleToUserAsync(int userId, int roleId) =>
            _restApi.PostAsync<RbacAssignRoleToUserDto, bool>(
                $"{Endpoint}/users/{userId}/roles",
                new RbacAssignRoleToUserDto { UserId = userId, RoleId = roleId },
                authorized: true);

        public Task<Response<bool>> RevokeRoleFromUserAsync(int userId, int roleId) =>
            _restApi.DeleteAsync<bool>($"{Endpoint}/users/{userId}/roles/{roleId}", authorized: true);

        private static string BuildRoleEndpoint(RbacRoleFilterDto filter)
        {
            var query = new List<string>();
            AddQueryValue(query, "BusinessId", filter.BusinessId);
            AddQueryValue(query, "Name", filter.Name);
            AddQueryValue(query, "Status", filter.Status);

            if (filter.OnlyGlobal.HasValue)
            {
                query.Add($"OnlyGlobal={filter.OnlyGlobal.Value.ToString().ToLowerInvariant()}");
            }

            return query.Count == 0 ? $"{Endpoint}/roles" : $"{Endpoint}/roles?{string.Join("&", query)}";
        }

        private static string BuildPermissionEndpoint(RbacPermissionFilterDto filter)
        {
            var query = new List<string>();
            AddQueryValue(query, "PermissionName", filter.PermissionName);
            AddQueryValue(query, "ModuleCode", filter.ModuleCode);
            AddQueryValue(query, "ActionCode", filter.ActionCode);
            AddQueryValue(query, "Status", filter.Status);

            return query.Count == 0
                ? $"{Endpoint}/permissions"
                : $"{Endpoint}/permissions?{string.Join("&", query)}";
        }

        private static Response<IReadOnlyList<RbacModuleDto>> ToReadOnlyResponse(Response<IEnumerable<RbacModuleDto>> response)
        {
            return new Response<IReadOnlyList<RbacModuleDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<RbacModuleDto>()
            };
        }

        private static void AddQueryValue(ICollection<string> query, string name, int? value)
        {
            if (value.HasValue)
            {
                query.Add($"{name}={value.Value}");
            }
        }

        private static void AddQueryValue(ICollection<string> query, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
            }
        }
    }
}
