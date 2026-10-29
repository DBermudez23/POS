using Microsoft.AspNetCore.Authorization;

namespace UtpPos.Framework.Services.WebApi.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string module, string action)
        {
            Policy = PermissionPolicy.BuildPolicyName(module, action);
        }

        public RequirePermissionAttribute(string permission)
        {
            Policy = PermissionPolicy.BuildPolicyName(permission);
        }
    }
}
