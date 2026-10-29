using UtpPos.Framework.Application.DTO.ThirdParty;
using UtpPos.Framework.Application.Interface.ThirdParty;
using UtpPos.Framework.Services.WebApi.Authorization;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/terceros")]
    [ApiController]
    public sealed class ThirdPartyController : ControllerBase
    {
        private readonly IThirdPartyApplication _application;

        public ThirdPartyController(IThirdPartyApplication application)
        {
            _application = application;
        }

        [RequirePermission("terceros-global.listar")]
        [HttpGet]
        public async Task<IActionResult> ListAsync([FromQuery] ThirdPartyListFilterDto filter)
        {
            var response = await _application.ListAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("terceros", "consultar")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (TryGetUserBusinessId(out var userBusinessId)
                && !await IsThirdPartyAssociatedAsync(userBusinessId, id))
            {
                return Forbid();
            }

            var response = await _application.GetByIdAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("terceros", "crear")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ThirdPartyCreateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<ThirdPartyResultDto>("El token no contiene un usuario válido."));

            var response = await _application.CreateAsync(dto, userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("terceros", "crear")]
        [HttpPost("por-negocio")]
        public async Task<IActionResult> CreateForBusinessAsync([FromBody] ThirdPartyCreateForBusinessDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<ThirdPartyBusinessResultDto>("El token no contiene un usuario válido."));

            if (!TryResolveBusinessId(dto.BusinessId, out var businessId))
                return Forbid();

            dto.BusinessId = businessId;
            var response = await _application.CreateForBusinessAsync(dto, userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("terceros", "actualizar")]
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] int id, [FromBody] ThirdPartyUpdateDto dto)
        {
            if (TryGetUserBusinessId(out var userBusinessId)
                && !await IsThirdPartyAssociatedAsync(userBusinessId, id))
            {
                return Forbid();
            }

            var response = await _application.UpdateAsync(id, dto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("terceros", "eliminar")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (TryGetUserBusinessId(out var userBusinessId))
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized(Fail<ThirdPartyResultDto>("El token no contiene un usuario válido."));

                var relationResponse = await _application.InactivateBusinessAsync(userBusinessId, id, userId);
                var responseForBusinessUser = new Response<ThirdPartyResultDto>
                {
                    IsSuccess = relationResponse.IsSuccess,
                    Message = relationResponse.Message,
                    Data = relationResponse.Data is null
                        ? null
                        : new ThirdPartyResultDto { ThirdPartyId = relationResponse.Data.ThirdPartyId }
                };

                return responseForBusinessUser.IsSuccess ? Ok(responseForBusinessUser) : BadRequest(responseForBusinessUser);
            }

            var response = await _application.DeleteAsync(id);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("terceros", "listar")]
        [HttpGet("por-negocio/{businessId:int}")]
        public async Task<IActionResult> ListByBusinessAsync([FromRoute] int businessId, [FromQuery] ThirdPartyBusinessListFilterDto filter)
        {
            if (!TryResolveBusinessId(businessId, out var resolvedBusinessId))
                return Forbid();

            var response = await _application.ListByBusinessAsync(resolvedBusinessId, filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("terceros", "crear")]
        [HttpPost("por-negocio/{businessId:int}/{thirdPartyId:int}")]
        public async Task<IActionResult> AssociateBusinessAsync([FromRoute] int businessId, [FromRoute] int thirdPartyId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<ThirdPartyBusinessResultDto>("El token no contiene un usuario válido."));

            if (!TryResolveBusinessId(businessId, out var resolvedBusinessId))
                return Forbid();

            var response = await _application.AssociateBusinessAsync(resolvedBusinessId, thirdPartyId, userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("terceros", "eliminar")]
        [HttpDelete("por-negocio/{businessId:int}/{thirdPartyId:int}")]
        public async Task<IActionResult> InactivateBusinessAsync([FromRoute] int businessId, [FromRoute] int thirdPartyId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(Fail<ThirdPartyBusinessResultDto>("El token no contiene un usuario válido."));

            if (!TryResolveBusinessId(businessId, out var resolvedBusinessId))
                return Forbid();

            var response = await _application.InactivateBusinessAsync(resolvedBusinessId, thirdPartyId, userId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("terceros", "consultar")]
        [HttpGet("por-negocio/{businessId:int}/{thirdPartyId:int}/validar")]
        public async Task<IActionResult> ValidateBusinessAsync([FromRoute] int businessId, [FromRoute] int thirdPartyId)
        {
            if (!TryResolveBusinessId(businessId, out var resolvedBusinessId))
                return Forbid();

            var response = await _application.ValidateBusinessAsync(resolvedBusinessId, thirdPartyId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(User.FindFirst(AuthClaimTypes.UserId)?.Value, out userId) && userId > 0;
        }

        private bool TryResolveBusinessId(int requestedBusinessId, out int businessId)
        {
            businessId = requestedBusinessId;

            if (int.TryParse(User.FindFirst(AuthClaimTypes.BusinessId)?.Value, out var userBusinessId)
                && userBusinessId > 0)
            {
                businessId = userBusinessId;
                return requestedBusinessId == userBusinessId;
            }

            return requestedBusinessId > 0;
        }

        private bool TryGetUserBusinessId(out int businessId)
        {
            return int.TryParse(User.FindFirst(AuthClaimTypes.BusinessId)?.Value, out businessId) && businessId > 0;
        }

        private async Task<bool> IsThirdPartyAssociatedAsync(int businessId, int thirdPartyId)
        {
            var validation = await _application.ValidateBusinessAsync(businessId, thirdPartyId);
            return validation.IsSuccess && validation.Data?.ExistsActiveRelation == true;
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T> { IsSuccess = false, Data = default, Message = message };
        }
    }
}
