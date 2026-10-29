using UtpPos.Framework.Application.DTO.Kardex;
using UtpPos.Framework.Application.Interface.Kardex;
using UtpPos.Framework.Services.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/kardex")]
    [ApiController]
    public sealed class KardexController : ControllerBase
    {
        private readonly IKardexApplication _application;

        public KardexController(IKardexApplication application)
        {
            _application = application;
        }

        [RequirePermission("kardex", "listar")]
        [HttpGet]
        public async Task<IActionResult> ListAsync([FromQuery] KardexListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("kardex", "consultar")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("kardex", "consultar")]
        [HttpGet("producto/{productoId:int}")]
        public async Task<IActionResult> ListByProductAsync(int productoId)
        {
            var response = await _application.ListByProductAsync(productoId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
