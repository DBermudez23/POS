using UtpPos.Framework.Application.DTO.Purchase;
using UtpPos.Framework.Application.Interface.Purchase;
using UtpPos.Framework.Services.WebApi.Authorization;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/compras")]
    [ApiController]
    public sealed class PurchaseController : ControllerBase
    {
        private readonly IPurchaseApplication _application;

        public PurchaseController(IPurchaseApplication application)
        {
            _application = application;
        }

        [RequirePermission("compras", "listar")]
        [HttpGet]
        public async Task<IActionResult> ListAsync([FromQuery] PurchaseListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("compras", "consultar")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("compras", "crear")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] PurchaseCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<PurchaseWriteResultDto>("El token no contiene un usuario válido."));

            var response = await _application.CreateAsync(dto, userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("compras", "anular")]
        [HttpPost("{id:int}/anular")]
        public async Task<IActionResult> CancelAsync(int id, [FromBody] PurchaseCancelDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<PurchaseCancelResultDto>("El token no contiene un usuario válido."));

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
