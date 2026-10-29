using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using UtpPos.Framework.Application.DTO.Product;
using UtpPos.WebAPI.IntegrationTests.Auth;
using UtpPos.WebAPI.IntegrationTests.Infrastructure;

namespace UtpPos.WebAPI.IntegrationTests.Product;

public sealed class ProductAuthorizationIntegrationTests
{
    private const string ListPermission = "productos.listar";
    private const string CreatePermission = "productos.crear";

    [Fact]
    public async Task ListProducts_WhenRequestHasNoAuthentication_ShouldReturn401()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/productos");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ProductApplication.ListCalls.Should().Be(0);
    }

    [Fact]
    public async Task ListProducts_WhenAuthenticatedUserDoesNotHavePermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/productos", permissions: CreatePermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.ProductApplication.ListCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(ListPermission);
    }

    [Fact]
    public async Task ListProducts_WhenAuthenticatedUserHasPermission_ShouldReturn200AndPassBusinessFilter()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Get, "/api/productos?BusinessId=7&Status=A", businessId: 7, permissions: ListPermission);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ProductApplication.ListCalls.Should().Be(1);
        factory.ProductApplication.LastListFilter!.BusinessId.Should().Be(7);
        factory.ProductApplication.LastListFilter.Status.Should().Be("A");
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(ListPermission);
    }

    [Fact]
    public async Task CreateProduct_WhenRequestHasNoAuthentication_ShouldReturn401()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/productos", ValidCreateDto());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        factory.ProductApplication.CreateCalls.Should().Be(0);
    }

    [Fact]
    public async Task CreateProduct_WhenAuthenticatedUserDoesNotHavePermission_ShouldReturn403()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Post, "/api/productos", businessId: 7, permissions: ListPermission);
        request.Content = JsonContent.Create(ValidCreateDto());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.ProductApplication.CreateCalls.Should().Be(0);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(CreatePermission);
    }

    [Fact]
    public async Task CreateProduct_WhenAuthenticatedUserHasPermission_ShouldReturn200()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = AuthenticatedRequest(HttpMethod.Post, "/api/productos", businessId: 7, permissions: CreatePermission);
        request.Content = JsonContent.Create(ValidCreateDto());

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.ProductApplication.CreateCalls.Should().Be(1);
        factory.ProductApplication.LastCreateDto!.BusinessId.Should().Be(7);
        factory.RbacApplication!.RequestedPermissions.Should().ContainSingle(CreatePermission);
    }

    private static ProductCreateDto ValidCreateDto()
    {
        return new ProductCreateDto
        {
            Name = "Producto Test",
            Description = "Producto Test",
            CategoryId = 1,
            VatPercentage = 19,
            ProfitMarginPercentage = 10,
            BusinessId = 7,
            CreatedByUserId = 1,
            Status = "A"
        };
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
