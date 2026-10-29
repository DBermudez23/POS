using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Authorization;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Authorization;

namespace UtpPos.Framework.Main.Authorization
{
    public sealed class CurrentUserRoleService : ICurrentUserRoleService
    {
        private readonly IRestApi _restApi;

        public CurrentUserRoleService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<CurrentUserRoleDto>>> GetCurrentUserRolesAsync()
        {
            var response = await _restApi.GetAsync<IEnumerable<CurrentUserRoleDto>>(
                "api/auth/me/roles",
                authorized: true);

            return new Response<IReadOnlyList<CurrentUserRoleDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<CurrentUserRoleDto>()
            };
        }
    }
}
