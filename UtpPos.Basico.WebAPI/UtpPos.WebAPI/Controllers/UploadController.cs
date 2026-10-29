using UtpPos.Framework.Application.Interface.File;
using UtpPos.Framework.Services.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/uploads")]
    [ApiController]
    public sealed class UploadController : ControllerBase
    {
        private readonly IFileUploadApplication _application;

        public UploadController(IFileUploadApplication application)
        {
            _application = application;
        }

        [HttpPost("product-images")]
        // TODO: Support productos.crear OR productos.actualizar when OR permissions are available.
        [RequirePermission("productos", "crear")]
        [RequestSizeLimit(3 * 1024 * 1024)]
        public async Task<IActionResult> UploadProductImageAsync([FromForm] IFormFile file)
        {
            var response = await _application.UploadProductImageAsync(file);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
