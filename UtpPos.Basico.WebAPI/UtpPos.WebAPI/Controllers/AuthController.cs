using UtpPos.Framework.Application.DTO.Auth;
using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.DTO.User;
using UtpPos.Framework.Application.Interface.Auth;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthApplication _application;
        private readonly IRbacApplication _rbacApplication;

        public AuthController(IAuthApplication application, IRbacApplication rbacApplication)
        {
            _application = application;
            _rbacApplication = rbacApplication;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto request)
        {
            var response = await _application.LoginAsync(request, GetIp(), GetUserAgent());
            if (response.IsSuccess)
                return Ok(response);

            return IsIncomplete(request)
                ? BadRequest(response)
                : Unauthorized(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequestDto request)
        {
            var response = await _application.RefreshAsync(request, GetIp(), GetUserAgent());
            if (response.IsSuccess)
                return Ok(response);

            return string.IsNullOrWhiteSpace(request?.RefreshToken)
                ? BadRequest(response)
                : Unauthorized(response);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequestDto request)
        {
            var response = await _application.LogoutAsync(request);
            if (response.IsSuccess)
                return Ok(response);

            return string.IsNullOrWhiteSpace(request?.RefreshToken)
                ? BadRequest(response)
                : Unauthorized(response);
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAllAsync()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<bool>("El token no contiene un usuario válido."));

            var response = await _application.LogoutAllAsync(userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMeAsync()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<UserDto>("El token no contiene un usuario válido."));

            var response = await _application.GetMeAsync(userId);
            return response.IsSuccess ? Ok(response) : Unauthorized(response);
        }

        [Authorize]
        [HttpGet("me/permissions")]
        public async Task<IActionResult> GetMyPermissionsAsync()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(Fail<IEnumerable<RbacUserPermissionDto>>("El token no contiene un usuario valido."));
            }

            var response = await _rbacApplication.GetUserPermissionsAsync(userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize]
        [HttpGet("me/roles")]
        public async Task<IActionResult> GetMyRolesAsync()
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized(Fail<IEnumerable<RbacUserRoleDto>>(
                    "El token no contiene un usuario válido."));
            }

            var response = await _rbacApplication.GetUserRolesAsync(new RbacUserRoleFilterDto
            {
                UserId = userId,
                Status = "A"
            });

            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(User.FindFirst(AuthClaimTypes.UserId)?.Value, out userId) && userId > 0;
        }

        private string? GetIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string? GetUserAgent()
        {
            return Request.Headers.UserAgent.FirstOrDefault();
        }

        private static bool IsIncomplete(LoginRequestDto? request)
        {
            return request is null ||
                   string.IsNullOrWhiteSpace(request.Email) ||
                   string.IsNullOrWhiteSpace(request.Password) ||
                   request.Email.Trim().Length > 80;
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T> { IsSuccess = false, Data = default, Message = message };
        }
    }
}
