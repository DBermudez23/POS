using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authorization;

namespace UtpPos.Framework.Services.WebApi.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IRbacApplication _rbacApplication;
        private readonly IAppLogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            IRbacApplication rbacApplication,
            IAppLogger<PermissionAuthorizationHandler> logger)
        {
            _rbacApplication = rbacApplication;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true ||
                !int.TryParse(context.User.FindFirst(AuthClaimTypes.UserId)?.Value, out var userId) ||
                userId <= 0)
            {
                return;
            }

            var response = await _rbacApplication.UserHasPermissionAsync(userId, requirement.Permission);
            if (response.IsSuccess && response.Data)
            {
                context.Succeed(requirement);
                return;
            }

            if (!response.IsSuccess)
            {
                _logger.LogWarning(
                    "No fue posible validar el permiso {Permission} para el usuario {UserId}: {Message}",
                    requirement.Permission,
                    userId,
                    response.Message ?? "Sin detalle.");
            }
        }
    }
}
