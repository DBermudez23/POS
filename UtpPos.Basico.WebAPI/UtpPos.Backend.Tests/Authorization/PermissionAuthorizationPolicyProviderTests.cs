using FluentAssertions;
using UtpPos.Framework.Services.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;

namespace UtpPos.Backend.Tests.Authorization;

public sealed class PermissionAuthorizationPolicyProviderTests
{
    [Theory]
    [InlineData("Permission:terceros.crear", "terceros.crear")]
    [InlineData("Permission:terceros-global.listar", "terceros-global.listar")]
    public async Task GetPolicyAsync_WhenPolicyIsPermissionPolicy_ShouldCreatePolicyWithPermissionRequirement(
        string policyName,
        string expectedPermission)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var policy = await sut.GetPolicyAsync(policyName);

        // Assert
        policy.Should().NotBeNull();
        policy!.Requirements
            .OfType<PermissionRequirement>()
            .Should()
            .ContainSingle(requirement => requirement.Permission == expectedPermission);
        policy.Requirements
            .OfType<DenyAnonymousAuthorizationRequirement>()
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task GetPolicyAsync_WhenPolicyIsNotPermissionPolicy_ShouldDelegateToBaseProvider()
    {
        // Arrange
        var options = new AuthorizationOptions();
        options.AddPolicy("ExistingPolicy", builder => builder.RequireClaim("scope", "test"));
        var sut = new PermissionAuthorizationPolicyProvider(Options.Create(options));

        // Act
        var policy = await sut.GetPolicyAsync("ExistingPolicy");

        // Assert
        policy.Should().NotBeNull();
        policy!.Requirements
            .OfType<ClaimsAuthorizationRequirement>()
            .Should()
            .ContainSingle();
    }

    [Fact]
    public async Task GetPolicyAsync_WhenUnknownPolicyIsNotPermissionPolicy_ShouldReturnNull()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var policy = await sut.GetPolicyAsync("UnknownPolicy");

        // Assert
        policy.Should().BeNull();
    }

    private static PermissionAuthorizationPolicyProvider CreateSut()
    {
        return new PermissionAuthorizationPolicyProvider(Options.Create(new AuthorizationOptions()));
    }
}
