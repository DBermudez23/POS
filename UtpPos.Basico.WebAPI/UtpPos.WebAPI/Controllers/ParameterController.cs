using UtpPos.Framework.Application.DTO.Parameter;
using UtpPos.Framework.Application.Interface.Parameter;
using UtpPos.Framework.Services.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/parametros")]
    [ApiController]
    public class ParameterController : ControllerBase
    {
        private readonly IParameterApplication _application;

        public ParameterController(IParameterApplication application)
        {
            _application = application;
        }

        [RequirePermission("parametros", "listar")]
        [HttpGet]
        public async Task<IActionResult> ListAsync([FromQuery] ParameterListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("parametros", "listar")]
        [HttpGet("por-clase/{codigoClase}")]
        public async Task<IActionResult> ListByClassCodeAsync(string codigoClase)
        {
            var response = await _application.ListByClassCodeAsync(codigoClase);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("parametros", "consultar")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("parametros", "consultar")]
        [HttpGet("por-codigo/{codigoParametro}")]
        public async Task<IActionResult> GetByCodeAsync(string codigoParametro)
        {
            var response = await _application.GetByCodeAsync(codigoParametro);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("parametros", "crear")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ParameterCreateDto dto)
        {
            var response = await _application.CreateAsync(dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("parametros", "actualizar")]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ParameterUpdateDto dto)
        {
            var response = await _application.UpdateAsync(id, dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("parametros", "eliminar")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var response = await _application.DeleteAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("parametros", "listar")]
        [HttpGet("~/api/clases/{classId:int}/parametros")]
        public async Task<IActionResult> ListByClassAsync(int classId, [FromQuery] ParameterListFilterDto filter)
        {
            filter.ClassId = classId;
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
