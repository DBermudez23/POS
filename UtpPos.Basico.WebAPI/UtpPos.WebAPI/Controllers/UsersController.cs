using UtpPos.Framework.Application.DTO.User;
using UtpPos.Framework.Application.Interface.User;
using UtpPos.Framework.Services.WebApi.Authorization;
using UtpPos.Framework.Services.WebApi.Contracts.Users;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/usuarios")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserApplication _application;
        private readonly IPasswordHasherService _passwordHasherService;

        public UsersController(
            IUserApplication application,
            IPasswordHasherService passwordHasherService)
        {
            _application = application;
            _passwordHasherService = passwordHasherService;
        }

        [HttpGet]
        [RequirePermission("usuarios", "listar")]
        public async Task<IActionResult> ListAsync([FromQuery] UserListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id:int}")]
        [RequirePermission("usuarios", "consultar")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        [RequirePermission("usuarios", "crear")]
        public async Task<IActionResult> CreateAsync([FromBody] UserCreateRequestDto request)
        {
            try
            {
                var response = await _application.CreateAsync(new UserCreateDto
                {
                    BusinessId = request.BusinessId,
                    DocumentTypeId = request.DocumentTypeId,
                    DocumentNumber = request.DocumentNumber,
                    Name = request.Name,
                    Email = request.Email,
                    PasswordHash = _passwordHasherService.Hash(request.Password),
                    Phone = request.Phone,
                    Status = request.Status
                });

                return response.IsSuccess ? Ok(response) : BadRequest(response);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(Fail<UserCreateResultDto>(exception.Message));
            }
        }

        [HttpPatch("{id:int}")]
        [RequirePermission("usuarios", "actualizar")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UserUpdateRequestDto request)
        {
            try
            {
                var response = await _application.UpdateAsync(new UserUpdateDto
                {
                    UserId = id,
                    BusinessId = request.BusinessId,
                    DocumentTypeId = request.DocumentTypeId,
                    DocumentNumber = request.DocumentNumber,
                    Name = request.Name,
                    Email = request.Email,
                    PasswordHash = string.IsNullOrWhiteSpace(request.Password)
                        ? null
                        : _passwordHasherService.Hash(request.Password),
                    Phone = request.Phone,
                    Status = request.Status,
                    ClearBusinessId = request.ClearBusinessId,
                    ClearPhone = request.ClearPhone
                });

                return response.IsSuccess ? Ok(response) : BadRequest(response);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(Fail<bool>(exception.Message));
            }
        }

        [HttpDelete("{id:int}")]
        [RequirePermission("usuarios", "eliminar")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var response = await _application.DeactivateAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        private static Response<T> Fail<T>(string message) => new()
        {
            IsSuccess = false,
            Message = message
        };
    }
}
