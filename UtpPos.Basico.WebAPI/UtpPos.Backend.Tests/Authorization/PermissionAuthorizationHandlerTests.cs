using System.Security.Claims;
using FluentAssertions;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Services.WebApi.Authorization;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;

namespace UtpPos.Backend.Tests.Authorization;

public sealed class PermissionAuthorizationHandlerTests
{
    private readonly IRbacApplication _rbacApplication = Substitute.For<IRbacApplication>();
    private readonly IAppLogger<PermissionAuthorizationHandler> _logger = Substitute.For<IAppLogger<PermissionAuthorizationHandler>>();

    [Fact]
    public async Task HandleAsync_WhenUserIsNotAuthenticated_ShouldNotSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("terceros.crear");
        var context = CreateContext(new ClaimsPrincipal(new ClaimsIdentity()), requirement);
        var sut = CreateSut();

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        await _rbacApplication.DidNotReceive().UserHasPermissionAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenAuthenticatedUserHasNoUserIdClaim_ShouldNotSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("terceros.crear");
        var user = AuthenticatedUser(new Claim(ClaimTypes.Name, "Test User"));
        var context = CreateContext(user, requirement);
        var sut = CreateSut();

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        await _rbacApplication.DidNotReceive().UserHasPermissionAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task HandleAsync_WhenUserIdClaimIsInvalid_ShouldNotSucceed(string userId)
    {
        // Arrange
        var requirement = new PermissionRequirement("terceros.crear");
        var user = AuthenticatedUser(new Claim(AuthClaimTypes.UserId, userId));
        var context = CreateContext(user, requirement);
        var sut = CreateSut();

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        await _rbacApplication.DidNotReceive().UserHasPermissionAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Theory]
    [InlineData("terceros.crear")]
    [InlineData("terceros-global.listar")]
    public async Task HandleAsync_WhenUserHasPermission_ShouldSucceed(string permission)
    {
        // Arrange
        var requirement = new PermissionRequirement(permission);
        var user = AuthenticatedUser(new Claim(AuthClaimTypes.UserId, "7"));
        var context = CreateContext(user, requirement);
        _rbacApplication.UserHasPermissionAsync(7, permission).Returns(new Response<bool>
        {
            IsSuccess = true,
            Data = true
        });
        var sut = CreateSut();

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        await _rbacApplication.Received(1).UserHasPermissionAsync(7, permission);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotHavePermission_ShouldNotSucceed()
    {
        // Arrange
        var requirement = new PermissionRequirement("terceros.crear");
        var user = AuthenticatedUser(new Claim(AuthClaimTypes.UserId, "7"));
        var context = CreateContext(user, requirement);
        _rbacApplication.UserHasPermissionAsync(7, "terceros.crear").Returns(new Response<bool>
        {
            IsSuccess = true,
            Data = false
        });
        var sut = CreateSut();

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        await _rbacApplication.Received(1).UserHasPermissionAsync(7, "terceros.crear");
    }

    [Fact]
    public async Task HandleAsync_WhenPermissionValidationFails_ShouldNotSucceedAndLogWarning()
    {
        // Arrange
        var requirement = new PermissionRequirement("terceros.crear");
        var user = AuthenticatedUser(new Claim(AuthClaimTypes.UserId, "7"));
        var context = CreateContext(user, requirement);
        _rbacApplication.UserHasPermissionAsync(7, "terceros.crear").Returns(new Response<bool>
        {
            IsSuccess = false,
            Data = false,
            Message = "validation failed"
        });
        var sut = CreateSut();

        // Act
        await sut.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
        _logger.Received(1).LogWarning(
            Arg.Any<string>(),
            Arg.Is<object[]>(args =>
                args.Length == 3 &&
                args[0].Equals("terceros.crear") &&
                args[1].Equals(7) &&
                args[2].Equals("validation failed")));
    }

    private PermissionAuthorizationHandler CreateSut()
    {
        return new PermissionAuthorizationHandler(_rbacApplication, _logger);
    }

    private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal user, PermissionRequirement requirement)
    {
        return new AuthorizationHandlerContext(new[] { requirement }, user, resource: null);
    }

    private static ClaimsPrincipal AuthenticatedUser(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "jwt"));
    }
}
