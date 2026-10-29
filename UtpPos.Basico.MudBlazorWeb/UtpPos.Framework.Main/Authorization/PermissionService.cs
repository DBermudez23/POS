using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Authorization;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Authorization;

namespace UtpPos.Framework.Main.Authorization
{
    public sealed class PermissionService : IPermissionService
    {
        private const string CurrentUserPermissionsEndpoint = "api/auth/me/permissions";

        private readonly IRestApi _restApi;

        public PermissionService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<UserPermissionDto>>> GetCurrentUserPermissionsAsync()
        {
            var response = await _restApi.GetAsync<IEnumerable<UserPermissionDto>>(
                CurrentUserPermissionsEndpoint,
                authorized: true);

            return new Response<IReadOnlyList<UserPermissionDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                StatusCode = response.StatusCode,
                Data = response.Data?.ToArray() ?? Array.Empty<UserPermissionDto>()
            };
        }
    }
}
