using UtpPos.Framework.Application.DTO.Auth;
using UtpPos.Framework.Application.DTO.User;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Auth
{
    public interface IAuthApplication
    {
        Task<Response<LoginResponseDto>> LoginAsync
        (
            LoginRequestDto request,
            string? ip,
            string? userAgent
        );

        Task<Response<RefreshTokenResponseDto>> RefreshAsync
        (
            RefreshTokenRequestDto request,
            string? ip,
            string? userAgent
        );
        
        Task<Response<bool>> LogoutAsync(LogoutRequestDto request);
        Task<Response<bool>> LogoutAllAsync(int userId);
        Task<Response<UserDto>> GetMeAsync(int userId);
    }
}
