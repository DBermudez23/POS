using Microsoft.AspNetCore.Authorization;

namespace UtpPos.Framework.Services.WebApi.Authorization
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission)
        {
            Permission = PermissionPolicy.TryGetPermission(
                PermissionPolicy.BuildPolicyName(permission),
                out var normalizedPermission)
                    ? normalizedPermission
                    : throw new ArgumentException("El permiso no es válido.", nameof(permission));
        }

        public string Permission { get; }
    }
}
