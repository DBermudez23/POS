using UtpPos.Framework.Application.DTO.Sale;
using UtpPos.Framework.Application.Interface.Sale;
using UtpPos.Framework.Services.WebApi.Authorization;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/ventas")]
    [ApiController]
    public sealed class SaleController : ControllerBase
    {
        private readonly ISaleApplication _application;

        public SaleController(ISaleApplication application)
        {
            _application = application;
        }

        [RequirePermission("ventas", "listar")]
        [HttpGet]
        public async Task<IActionResult> ListAsync([FromQuery] SaleListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("ventas", "consultar")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("ventas", "crear")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] SaleCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<SaleWriteResultDto>("El token no contiene un usuario válido."));

            var response = await _application.CreateAsync(dto, userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("ventas", "anular")]
        [HttpPost("{id:int}/anular")]
        public async Task<IActionResult> CancelAsync(int id, [FromBody] SaleCancelDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<SaleCancelResultDto>("El token no contiene un usuario válido."));

            var response = await _application.CancelAsync(id, dto, userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(User.FindFirst(AuthClaimTypes.UserId)?.Value, out userId) && userId > 0;
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T> { IsSuccess = false, Data = default, Message = message };
        }
    }
}
