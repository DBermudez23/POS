using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.User;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.User;

namespace UtpPos.Framework.Main.User
{
    public sealed class UserService : IUserService
    {
        private const string Endpoint = "api/usuarios";

        private readonly IRestApi _restApi;

        public UserService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public async Task<Response<IReadOnlyList<UserDto>>> ListAsync(UserListFilterDto filter)
        {
            var response = await _restApi.GetAsync<IEnumerable<UserDto>>(
                BuildListEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<UserDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                Data = response.Data?.ToArray() ?? Array.Empty<UserDto>()
            };
        }

        public Task<Response<UserDto>> GetByIdAsync(int id) =>
            _restApi.GetAsync<UserDto>($"{Endpoint}/{id}", authorized: true);

        public Task<Response<UserCreateResultDto>> CreateAsync(UserCreateRequestDto request) =>
            _restApi.PostAsync<UserCreateRequestDto, UserCreateResultDto>(Endpoint, request, authorized: true);

        public Task<Response<bool>> UpdateAsync(int id, UserUpdateRequestDto request) =>
            _restApi.PatchAsync<UserUpdateRequestDto, bool>($"{Endpoint}/{id}", request, authorized: true);

        public Task<Response<bool>> DeleteAsync(int id) =>
            _restApi.DeleteAsync<bool>($"{Endpoint}/{id}", authorized: true);

        private static string BuildListEndpoint(UserListFilterDto filter)
        {
            var query = new List<string>();
            AddQueryValue(query, "BusinessId", filter.BusinessId);
            AddQueryValue(query, "DocumentTypeId", filter.DocumentTypeId);
            AddQueryValue(query, "Status", filter.Status);
            AddQueryValue(query, "DocumentNumber", filter.DocumentNumber);
            AddQueryValue(query, "Name", filter.Name);
            AddQueryValue(query, "Email", filter.Email);

            return query.Count == 0 ? Endpoint : $"{Endpoint}?{string.Join("&", query)}";
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
