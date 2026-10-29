using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Services.WebApi.Authorization;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UtpPos.Framework.Services.WebApi.Controllers
{
    [Authorize]
    [Route("api/rbac")]
    [ApiController]
    public class RbacController : ControllerBase
    {
        private readonly IRbacApplication _application;

        public RbacController(IRbacApplication application)
        {
            _application = application;
        }

        [HttpGet("modules")]
        [RequirePermission("permisos", "listar")]
        public async Task<IActionResult> ListModulesAsync([FromQuery] RbacModuleFilterDto filter)
        {
            var response = await _application.ListModulesAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("modules")]
        [RequirePermission("permisos", "crear")]
        public async Task<IActionResult> CreateModuleAsync([FromBody] RbacModuleCreateDto request)
        {
            var response = await _application.CreateModuleAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("actions")]
        [RequirePermission("permisos", "listar")]
        public async Task<IActionResult> ListActionsAsync([FromQuery] RbacActionFilterDto filter)
        {
            var response = await _application.ListActionsAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("actions")]
        [RequirePermission("permisos", "crear")]
        public async Task<IActionResult> CreateActionAsync([FromBody] RbacActionCreateDto request)
        {
            var response = await _application.CreateActionAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("permissions")]
        [RequirePermission("permisos", "listar")]
        public async Task<IActionResult> ListPermissionsAsync([FromQuery] RbacPermissionFilterDto filter)
        {
            var response = await _application.ListPermissionsAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("permissions")]
        [RequirePermission("permisos", "crear")]
        public async Task<IActionResult> CreatePermissionAsync([FromBody] RbacPermissionCreateDto request)
        {
            var response = await _application.CreatePermissionAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("roles")]
        [RequirePermission("roles", "listar")]
        public async Task<IActionResult> ListRolesAsync([FromQuery] RbacRoleFilterDto filter)
        {
            var response = await _application.GetRolesAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("roles/{roleId:int}")]
        [RequirePermission("roles", "consultar")]
        public async Task<IActionResult> GetRoleByIdAsync(int roleId)
        {
            var response = await _application.GetRoleByIdAsync(roleId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("roles")]
        [RequirePermission("roles", "crear")]
        public async Task<IActionResult> CreateRoleAsync([FromBody] RbacRoleCreateDto request)
        {
            var response = await _application.CreateRoleAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPatch("roles/{roleId:int}")]
        [RequirePermission("roles", "actualizar")]
        public async Task<IActionResult> UpdateRoleAsync(int roleId, [FromBody] RbacRoleUpdateDto request)
        {
            request.RoleId = roleId;
            var response = await _application.UpdateRoleAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("roles/{roleId:int}")]
        [RequirePermission("roles", "eliminar")]
        public async Task<IActionResult> DeleteRoleAsync(int roleId)
        {
            var response = await _application.DeactivateRoleAsync(roleId);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("role-permissions")]
        [RequirePermission("roles", "consultar")]
        public async Task<IActionResult> ListRolePermissionsAsync([FromQuery] RbacRolePermissionFilterDto filter)
        {
            var response = await _application.GetRolePermissionsAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("roles/{roleId:int}/permissions")]
        [RequirePermission("roles", "actualizar")]
        public async Task<IActionResult> AssignPermissionToRoleAsync(
            int roleId,
            [FromBody] RbacAssignPermissionToRoleDto request)
        {
            request.RoleId = roleId;
            request.AssignedByUserId = TryGetCurrentUserId();
            var response = await _application.AssignPermissionToRoleAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("roles/{roleId:int}/permissions/{permissionId:int}")]
        [RequirePermission("roles", "actualizar")]
        public async Task<IActionResult> RevokePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var response = await _application.RevokePermissionFromRoleAsync(new RbacRevokePermissionFromRoleDto
            {
                RoleId = roleId,
                PermissionId = permissionId
            });

            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpGet("user-roles")]
        [RequirePermission("usuarios", "consultar")]
        public async Task<IActionResult> ListUserRolesAsync([FromQuery] RbacUserRoleFilterDto filter)
        {
            var response = await _application.GetUserRolesAsync(filter);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("users/{userId:int}/roles")]
        [RequirePermission("usuarios", "actualizar")]
        public async Task<IActionResult> AssignRoleToUserAsync(
            int userId,
            [FromBody] RbacAssignRoleToUserDto request)
        {
            request.UserId = userId;
            request.AssignedByUserId = TryGetCurrentUserId();
            var response = await _application.AssignRoleToUserAsync(request);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("users/{userId:int}/roles/{roleId:int}")]
        [RequirePermission("usuarios", "actualizar")]
        public async Task<IActionResult> RevokeRoleFromUserAsync(int userId, int roleId)
        {
            var response = await _application.RevokeRoleFromUserAsync(new RbacRevokeRoleFromUserDto
            {
                UserId = userId,
                RoleId = roleId
            });

            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        private int? TryGetCurrentUserId()
        {
            return int.TryParse(User.FindFirst(AuthClaimTypes.UserId)?.Value, out var userId) && userId > 0
                ? userId
                : null;
        }
    }
}
