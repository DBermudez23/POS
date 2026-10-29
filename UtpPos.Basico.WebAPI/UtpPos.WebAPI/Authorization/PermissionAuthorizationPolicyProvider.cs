using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace UtpPos.Framework.Services.WebApi.Authorization
{
    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options)
        {
        }

        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!PermissionPolicy.TryGetPermission(policyName, out var permission))
                return base.GetPolicyAsync(policyName);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
    }
}
