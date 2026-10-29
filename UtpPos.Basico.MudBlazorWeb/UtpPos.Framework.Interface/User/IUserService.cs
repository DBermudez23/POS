using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.User;

namespace UtpPos.Framework.Interface.User
{
    public interface IUserService
    {
        Task<Response<IReadOnlyList<UserDto>>> ListAsync(UserListFilterDto filter);
        Task<Response<UserDto>> GetByIdAsync(int id);
        Task<Response<UserCreateResultDto>> CreateAsync(UserCreateRequestDto request);
        Task<Response<bool>> UpdateAsync(int id, UserUpdateRequestDto request);
        Task<Response<bool>> DeleteAsync(int id);
    }
}
