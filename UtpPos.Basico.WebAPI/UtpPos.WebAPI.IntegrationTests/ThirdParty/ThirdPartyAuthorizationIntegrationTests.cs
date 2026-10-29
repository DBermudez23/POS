using System.Net;
using FluentAssertions;
using UtpPos.WebAPI.IntegrationTests.Auth;
using UtpPos.WebAPI.IntegrationTests.Infrastructure;

namespace UtpPos.WebAPI.IntegrationTests.ThirdParty;

public sealed class ThirdPartyAuthorizationIntegrationTests
{
    private const string GlobalPermission = "terceros-global.listar";
    private const string BusinessListPermission = "terceros.listar";
    private const string BusinessCreatePermission = "terceros.crear";
    private const string BusinessDeletePermission = "terceros.eliminar";

    [Fact]
    public async Task ListGlobal_WhenRequestHasNoAuthentication_ShouldReturn401()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/terceros");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ThirdPartyApplication.ListCalls.Should().Be(0);
    }

    [Fact]
    public async Task ListGlobal_WhenAuthenticatedUserDoesNotHaveGlobalPermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/terceros", permissions: BusinessListPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.ThirdPartyApplication.ListCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(GlobalPermission);
        factory.RbacApplication.RequestedPermissions.Should().NotContain("terceros.global.listar");
    }

    [Fact]
    public async Task ListGlobal_WhenAuthenticatedUserHasHyphenatedGlobalPermission_ShouldReturn200()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/terceros", permissions: GlobalPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ThirdPartyApplication.ListCalls.Should().Be(1);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(GlobalPermission);
        factory.RbacApplication.RequestedPermissions.Should().NotContain("terceros.global.listar");
    }

    [Fact]
    public async Task ListByBusiness_WhenUserHasSameBusinessIdAndOperationalPermission_ShouldReturn200()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/terceros/por-negocio/1", businessId: 1, permissions: BusinessListPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ThirdPartyApplication.ListByBusinessCalls.Should().Be(1);
        factory.ThirdPartyApplication.LastBusinessId.Should().Be(1);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(BusinessListPermission);
        factory.RbacApplication.RequestedPermissions.Should().NotContain(GlobalPermission);
    }

    [Fact]
    public async Task ListByBusiness_WhenBusinessUserRequestsAnotherBusiness_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/terceros/por-negocio/2", businessId: 1, permissions: BusinessListPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.ThirdPartyApplication.ListByBusinessCalls.Should().Be(0);
    }

    [Fact]
    public async Task ListByBusiness_WhenUserLacksGlobalPermissionButHasOperationalPermission_ShouldReturn200()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/terceros/por-negocio/1", businessId: 1, permissions: BusinessListPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(BusinessListPermission);
        factory.RbacApplication.RequestedPermissions.Should().NotContain(GlobalPermission);
    }

    [Fact]
    public async Task ListByBusiness_WhenGlobalAdminHasNoBusinessIdAndOperationalPermission_ShouldReturn200()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/terceros/por-negocio/2", permissions: BusinessListPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ThirdPartyApplication.ListByBusinessCalls.Should().Be(1);
        factory.ThirdPartyApplication.LastBusinessId.Should().Be(2);
    }

    [Fact]
    public async Task AssociateBusiness_WhenRequestHasNoAuthentication_ShouldReturn401()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/api/terceros/por-negocio/1/10", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ThirdPartyApplication.AssociateBusinessCalls.Should().Be(0);
    }

    [Fact]
    public async Task AssociateBusiness_WhenAuthenticatedUserDoesNotHaveCreatePermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Post, "/api/terceros/por-negocio/1/10", businessId: 1, permissions: BusinessListPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.ThirdPartyApplication.AssociateBusinessCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(BusinessCreatePermission);
    }

    [Fact]
    public async Task AssociateBusiness_WhenUserHasPermissionAndAllowedBusiness_ShouldReturn200()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Post, "/api/terceros/por-negocio/1/10", businessId: 1, permissions: BusinessCreatePermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ThirdPartyApplication.AssociateBusinessCalls.Should().Be(1);
        factory.ThirdPartyApplication.LastBusinessId.Should().Be(1);
        factory.ThirdPartyApplication.LastThirdPartyId.Should().Be(10);
        factory.ThirdPartyApplication.LastUserId.Should().Be(1);
    }

    [Fact]
    public async Task AssociateBusiness_WhenBusinessUserRequestsAnotherBusiness_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Post, "/api/terceros/por-negocio/2/10", businessId: 1, permissions: BusinessCreatePermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.ThirdPartyApplication.AssociateBusinessCalls.Should().Be(0);
    }

    [Fact]
    public async Task InactivateBusiness_WhenRequestHasNoAuthentication_ShouldReturn401()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/terceros/por-negocio/1/10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ThirdPartyApplication.InactivateBusinessCalls.Should().Be(0);
        factory.ThirdPartyApplication.DeleteCalls.Should().Be(0);
    }

    [Fact]
    public async Task InactivateBusiness_WhenAuthenticatedUserDoesNotHaveDeletePermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Delete, "/api/terceros/por-negocio/1/10", businessId: 1, permissions: BusinessListPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.ThirdPartyApplication.InactivateBusinessCalls.Should().Be(0);
        factory.ThirdPartyApplication.DeleteCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(BusinessDeletePermission);
    }

    [Fact]
    public async Task InactivateBusiness_WhenUserHasPermissionAndAllowedBusiness_ShouldReturn200AndNotDeleteGlobalThirdParty()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Delete, "/api/terceros/por-negocio/1/10", businessId: 1, permissions: BusinessDeletePermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ThirdPartyApplication.InactivateBusinessCalls.Should().Be(1);
        factory.ThirdPartyApplication.DeleteCalls.Should().Be(0);
        factory.ThirdPartyApplication.LastBusinessId.Should().Be(1);
        factory.ThirdPartyApplication.LastThirdPartyId.Should().Be(10);
        factory.ThirdPartyApplication.LastUserId.Should().Be(1);
    }

    [Fact]
    public async Task InactivateBusiness_WhenBusinessUserRequestsAnotherBusiness_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Delete, "/api/terceros/por-negocio/2/10", businessId: 1, permissions: BusinessDeletePermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.ThirdPartyApplication.InactivateBusinessCalls.Should().Be(0);
        factory.ThirdPartyApplication.DeleteCalls.Should().Be(0);
    }

    private static HttpRequestMessage AuthenticatedRequest(
        HttpMethod method,
        string uri,
        int userId = 1,
        int? businessId = null,
        params string[] permissions)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        request.Headers.Add(TestAuthHandler.EmailHeader, "integration@test.com");
        request.Headers.Add(TestAuthHandler.NameHeader, "Integration Test");
        request.Headers.Add(TestAuthHandler.SessionIdHeader, "integration-session");

        if (businessId.HasValue)
            request.Headers.Add(TestAuthHandler.BusinessIdHeader, businessId.Value.ToString());

        if (permissions.Length > 0)
            request.Headers.Add(TestAuthHandler.PermissionsHeader, string.Join(',', permissions));

        return request;
    }
}
