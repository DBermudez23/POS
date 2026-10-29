using UtpPos.Framework.Application.DTO.Dashboard;
using UtpPos.Framework.Application.Interface.Dashboard;
using UtpPos.Framework.Services.WebApi.Authorization;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/dashboard")]
    [ApiController]
    public sealed class DashboardController : ControllerBase
    {
        private readonly IDashboardApplication _application;

        public DashboardController(IDashboardApplication application)
        {
            _application = application;
        }

        [RequirePermission("dashboard.ver")]
        [HttpGet("basic-overview")]
        public async Task<IActionResult> GetBasicOverviewAsync(
            [FromQuery] int? businessId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? top)
        {
            var resolution = await ResolveDashboardBusinessIdAsync(businessId);
            var failure = BuildBusinessResolutionFailure<DashboardBasicOverviewDto>(resolution);
            if (failure is not null)
                return failure;

            var response = await _application.GetBasicOverviewAsync(resolution.BusinessId, startDate, endDate, top);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [RequirePermission("dashboard-finanzas.ver")]
        [HttpGet("financial-overview")]
        public async Task<IActionResult> GetFinancialOverviewAsync(
            [FromQuery] int? businessId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var resolution = await ResolveDashboardBusinessIdAsync(businessId);
            var failure = BuildBusinessResolutionFailure<DashboardFinancialOverviewDto>(resolution);
            if (failure is not null)
                return failure;

            var response = await _application.GetFinancialOverviewAsync(resolution.BusinessId, startDate, endDate);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        private async Task<DashboardBusinessResolutionDto> ResolveDashboardBusinessIdAsync(int? requestedBusinessId)
        {
            var userId = TryGetUserId(out var parsedUserId) ? parsedUserId : 0;
            var userBusinessId = TryGetBusinessId(out var parsedBusinessId) ? parsedBusinessId : (int?)null;

            return await _application.ResolveDashboardBusinessIdAsync(userId, userBusinessId, requestedBusinessId);
        }

        private IActionResult? BuildBusinessResolutionFailure<T>(DashboardBusinessResolutionDto resolution)
        {
            if (resolution.IsSuccess)
                return null;

            if (resolution.IsForbidden)
                return Forbid();

            if (resolution.IsUnauthorized)
                return Unauthorized(Fail<T>(resolution.Message));

            return BadRequest(Fail<T>(resolution.Message));
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(User.FindFirst(AuthClaimTypes.UserId)?.Value, out userId) && userId > 0;
        }

        private bool TryGetBusinessId(out int businessId)
        {
            return int.TryParse(User.FindFirst(AuthClaimTypes.BusinessId)?.Value, out businessId) && businessId > 0;
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T> { IsSuccess = false, Data = default, Message = message };
        }
    }
}
