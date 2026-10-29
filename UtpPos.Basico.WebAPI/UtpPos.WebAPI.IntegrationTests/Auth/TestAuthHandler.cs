using System.Security.Claims;
using System.Text.Encodings.Web;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UtpPos.WebAPI.IntegrationTests.Auth;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestAuth";
    public const string UserIdHeader = "X-Test-UserId";
    public const string EmailHeader = "X-Test-Email";
    public const string NameHeader = "X-Test-Name";
    public const string SessionIdHeader = "X-Test-SessionId";
    public const string BusinessIdHeader = "X-Test-BusinessId";
    public const string PermissionsHeader = "X-Test-Permissions";
    public const string PermissionClaimType = "test_permission";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock) {}

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userIdValues) || string.IsNullOrWhiteSpace(userIdValues))
            return Task.FromResult(AuthenticateResult.NoResult());

        var userId = userIdValues.ToString();
        var claims = new List<Claim>
        {
            new(AuthClaimTypes.UserId, userId),
            new(AuthClaimTypes.Email, GetHeaderOrDefault(EmailHeader, "test@utppos.local")),
            new(AuthClaimTypes.Name, GetHeaderOrDefault(NameHeader, "Test User")),
            new(AuthClaimTypes.SessionId, GetHeaderOrDefault(SessionIdHeader, "test-session"))
        };

        if (Request.Headers.TryGetValue(BusinessIdHeader, out var businessIdValues) && !string.IsNullOrWhiteSpace(businessIdValues))
            claims.Add(new Claim(AuthClaimTypes.BusinessId, businessIdValues.ToString()));

        if (Request.Headers.TryGetValue(PermissionsHeader, out var permissionValues))
        {
            foreach (var permission in permissionValues.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                claims.Add(new Claim(PermissionClaimType, permission));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme, AuthClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string GetHeaderOrDefault(string headerName, string defaultValue)
    {
        return Request.Headers.TryGetValue(headerName, out var values) && !string.IsNullOrWhiteSpace(values)
            ? values.ToString()
            : defaultValue;
    }
}
