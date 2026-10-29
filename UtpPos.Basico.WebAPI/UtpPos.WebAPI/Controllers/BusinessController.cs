using UtpPos.Framework.Application.DTO.Business;
using UtpPos.Framework.Application.Interface.Business;
using UtpPos.Framework.Services.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/negocios")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessApplication _application;

        public BusinessController(IBusinessApplication application)
        {
            _application = application;
        }

        [RequirePermission("negocios", "listar")]
        [HttpGet]
        public async Task<IActionResult> ListAsync([FromQuery] BusinessListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("negocios", "consultar")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("negocios", "crear")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] BusinessCreateDto dto)
        {
            var response = await _application.CreateAsync(dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("negocios", "actualizar")]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] BusinessUpdateDto dto)
        {
            var response = await _application.UpdateAsync(id, dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("negocios", "eliminar")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var response = await _application.DeleteAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
