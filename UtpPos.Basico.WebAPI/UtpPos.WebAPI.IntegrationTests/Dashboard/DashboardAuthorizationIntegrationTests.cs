using System.Net;
using FluentAssertions;
using UtpPos.WebAPI.IntegrationTests.Auth;
using UtpPos.WebAPI.IntegrationTests.Infrastructure;

namespace UtpPos.WebAPI.IntegrationTests.Dashboard;

public sealed class DashboardAuthorizationIntegrationTests
{
    private const string BasicPermission = "dashboard.ver";
    private const string FinancialPermission = "dashboard-finanzas.ver";
    private const string GlobalPermission = "dashboard-global.ver";

    [Fact]
    public async Task BasicOverview_WhenRequestHasNoAuthentication_ShouldReturn401()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/basic-overview");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.DashboardRepository.BasicOverviewCalls.Should().Be(0);
    }

    [Fact]
    public async Task BasicOverview_WhenAuthenticatedUserDoesNotHavePermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/dashboard/basic-overview", businessId: 7, permissions: FinancialPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.DashboardRepository.BasicOverviewCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(BasicPermission);
    }

    [Fact]
    public async Task BasicOverview_WhenAuthenticatedUserHasPermission_ShouldReturn200AndUseBusinessClaim()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(
            HttpMethod.Get,
            "/api/dashboard/basic-overview?businessId=999&startDate=2026-01-01&endDate=2026-01-31&top=8",
            businessId: 7,
            permissions: BasicPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.DashboardRepository.BasicOverviewCalls.Should().Be(1);
        factory.DashboardRepository.LastBusinessId.Should().Be(7);
        factory.DashboardRepository.LastStartDate.Should().Be(new DateTime(2026, 1, 1));
        factory.DashboardRepository.LastEndDate.Should().Be(new DateTime(2026, 1, 31));
        factory.DashboardRepository.LastTop.Should().Be(8);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(BasicPermission);
        factory.RbacApplication.RequestedPermissions.Should().NotContain("dashboard-ver");
    }

    [Fact]
    public async Task BasicOverview_WhenGlobalUserDoesNotSendBusinessId_ShouldReturnControlledErrorWithoutCallingRepository()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/dashboard/basic-overview", permissions: BasicPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        factory.DashboardRepository.BasicOverviewCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(BasicPermission);
    }

    [Fact]
    public async Task BasicOverview_WhenGlobalUserSendsBusinessIdWithoutGlobalPermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/dashboard/basic-overview?businessId=5", permissions: BasicPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.DashboardRepository.BasicOverviewCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().Contain(BasicPermission);
        factory.RbacApplication.RequestedPermissions.Should().Contain(GlobalPermission);
    }

    [Fact]
    public async Task BasicOverview_WhenGlobalUserSendsBusinessIdWithGlobalPermission_ShouldReturn200()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(
            HttpMethod.Get,
            "/api/dashboard/basic-overview?businessId=5&top=8",
            permissions: new[] { BasicPermission, GlobalPermission });

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.DashboardRepository.BasicOverviewCalls.Should().Be(1);
        factory.DashboardRepository.LastBusinessId.Should().Be(5);
        factory.DashboardRepository.LastTop.Should().Be(8);
        factory.RbacApplication!.RequestedPermissions.Should().Contain(BasicPermission);
        factory.RbacApplication.RequestedPermissions.Should().Contain(GlobalPermission);
    }

    [Fact]
    public async Task FinancialOverview_WhenRequestHasNoAuthentication_ShouldReturn401()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/dashboard/financial-overview");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.DashboardRepository.FinancialOverviewCalls.Should().Be(0);
    }

    [Fact]
    public async Task FinancialOverview_WhenAuthenticatedUserDoesNotHavePermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/dashboard/financial-overview", businessId: 7, permissions: BasicPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.DashboardRepository.FinancialOverviewCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(FinancialPermission);
        factory.RbacApplication.RequestedPermissions.Should().NotContain("dashboard.finanzas.ver");
        factory.RbacApplication.RequestedPermissions.Should().NotContain("dashboard-finanzas-ver");
    }

    [Fact]
    public async Task FinancialOverview_WhenAuthenticatedUserHasPermission_ShouldReturn200AndUseBusinessClaim()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(
            HttpMethod.Get,
            "/api/dashboard/financial-overview?businessId=999&startDate=2026-01-01&endDate=2026-01-31",
            businessId: 7,
            permissions: FinancialPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.DashboardRepository.FinancialOverviewCalls.Should().Be(1);
        factory.DashboardRepository.LastBusinessId.Should().Be(7);
        factory.DashboardRepository.LastStartDate.Should().Be(new DateTime(2026, 1, 1));
        factory.DashboardRepository.LastEndDate.Should().Be(new DateTime(2026, 1, 31));
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(FinancialPermission);
    }

    [Fact]
    public async Task FinancialOverview_WhenGlobalUserHasGlobalPermissionButNotFinancialPermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/dashboard/financial-overview?businessId=5", permissions: GlobalPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.DashboardRepository.FinancialOverviewCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(FinancialPermission);
    }

    [Fact]
    public async Task FinancialOverview_WhenGlobalUserHasFinancialPermissionButNotGlobalPermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/dashboard/financial-overview?businessId=5", permissions: FinancialPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.DashboardRepository.FinancialOverviewCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().Contain(FinancialPermission);
        factory.RbacApplication.RequestedPermissions.Should().Contain(GlobalPermission);
    }

    [Fact]
    public async Task FinancialOverview_WhenGlobalUserHasBothPermissions_ShouldReturn200()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(
            HttpMethod.Get,
            "/api/dashboard/financial-overview?businessId=5",
            permissions: new[] { FinancialPermission, GlobalPermission });

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.DashboardRepository.FinancialOverviewCalls.Should().Be(1);
        factory.DashboardRepository.LastBusinessId.Should().Be(5);
        factory.RbacApplication!.RequestedPermissions.Should().Contain(FinancialPermission);
        factory.RbacApplication.RequestedPermissions.Should().Contain(GlobalPermission);
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
