using UtpPos.Framework.Application.DTO.Product;
using UtpPos.Framework.Application.Interface.Product;
using UtpPos.Framework.Services.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/productos")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductApplication _application;

        public ProductController(IProductApplication application)
        {
            _application = application;
        }

        [RequirePermission("productos", "listar")]
        [HttpGet]
        public async Task<IActionResult> ListAsync([FromQuery] ProductListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("productos", "consultar")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("productos", "crear")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ProductCreateDto dto)
        {
            var response = await _application.CreateAsync(dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("productos", "actualizar")]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ProductUpdateDto dto)
        {
            var response = await _application.UpdateAsync(id, dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("productos", "eliminar")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var response = await _application.DeleteAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}