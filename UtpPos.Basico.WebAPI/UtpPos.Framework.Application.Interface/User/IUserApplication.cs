using UtpPos.Framework.Application.DTO.User;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.User
{
    public interface IUserApplication
    {
        Task<Response<IEnumerable<UserDto>>> ListAsync(UserListFilterDto filter);
        Task<Response<UserDto>> GetByIdAsync(int userId);
        Task<Response<UserCreateResultDto>> CreateAsync(UserCreateDto request);
        Task<Response<bool>> UpdateAsync(UserUpdateDto request);
        Task<Response<bool>> DeactivateAsync(int userId);
    }
}
