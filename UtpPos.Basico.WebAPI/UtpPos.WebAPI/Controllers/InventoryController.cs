using UtpPos.Framework.Application.DTO.Inventory;
using UtpPos.Framework.Application.Interface.Inventory;
using UtpPos.Framework.Services.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/inventario")]
    [ApiController]
    public sealed class InventoryController : ControllerBase
    {
        private readonly IInventoryApplication _application;

        public InventoryController(IInventoryApplication application)
        {
            _application = application;
        }

        [RequirePermission("inventario", "listar")]
        [HttpGet]
        public async Task<IActionResult> ListAsync([FromQuery] InventoryListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("inventario", "consultar")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("inventario", "consultar")]
        [HttpGet("producto/{productoId:int}")]
        public async Task<IActionResult> ListByProductAsync(int productoId)
        {
            var response = await _application.ListByProductAsync(productoId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("inventario", "movimiento")]
        [HttpPost("movimientos")]
        public async Task<IActionResult> RegisterMovementAsync([FromBody] InventoryMovementCreateDto dto)
        {
            var response = await _application.RegisterMovementAsync(dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
