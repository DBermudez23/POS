using UtpPos.Framework.Application.DTO.Class;
using UtpPos.Framework.Application.Interface.Class;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/clases")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassApplication _application;

        public ClassController(IClassApplication application)
        {
            _application = application;
        }

        [HttpGet]
        public async Task<IActionResult> ListAsync([FromQuery] ClassListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ClassCreateDto dto)
        {
            var response = await _application.CreateAsync(dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ClassUpdateDto dto)
        {
            var response = await _application.UpdateAsync(id, dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var response = await _application.DeleteAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
