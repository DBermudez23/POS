using AutoMapper;
using FluentAssertions;
using UtpPos.Framework.Application.Main.Rbac;
using UtpPos.Framework.Domain.Entity.Rbac;
using UtpPos.Framework.InfraStructure.Interface.Rbac;
using NSubstitute;

namespace UtpPos.Backend.Tests.Rbac;

public sealed class RbacApplicationUserHasPermissionTests
{
    private readonly IRbacRepository _repository = Substitute.For<IRbacRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    [Fact]
    public async Task UserHasPermissionAsync_WhenUserIdIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var response = await sut.UserHasPermissionAsync(0, "terceros.crear");

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeFalse();
        response.Message.Should().Be("El usuario no es válido.");
        await _repository.DidNotReceive().GetUserPermissionsAsync(Arg.Any<int>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("terceros")]
    [InlineData("terceros.crear.extra")]
    [InlineData("terceros.*")]
    public async Task UserHasPermissionAsync_WhenPermissionFormatIsInvalid_ShouldReturnFailure(string? permission)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var response = await sut.UserHasPermissionAsync(1, permission!);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeFalse();
        response.Message.Should().Be("El permiso debe tener el formato modulo.accion.");
        await _repository.DidNotReceive().GetUserPermissionsAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task UserHasPermissionAsync_WhenPermissionIsAssignedAndActive_ShouldReturnTrue()
    {
        // Arrange
        _repository.GetUserPermissionsAsync(1).Returns(new[]
        {
            Permission("terceros.crear", "A")
        });
        var sut = CreateSut();

        // Act
        var response = await sut.UserHasPermissionAsync(1, "terceros.crear");

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeTrue();
        response.Message.Should().Be("El usuario tiene el permiso requerido.");
    }

    [Fact]
    public async Task UserHasPermissionAsync_WhenPermissionIsNotAssigned_ShouldReturnFalse()
    {
        // Arrange
        _repository.GetUserPermissionsAsync(1).Returns(new[]
        {
            Permission("terceros.consultar", "A")
        });
        var sut = CreateSut();

        // Act
        var response = await sut.UserHasPermissionAsync(1, "terceros.crear");

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeFalse();
        response.Message.Should().Be("El usuario no tiene el permiso requerido.");
    }

    [Fact]
    public async Task UserHasPermissionAsync_WhenPermissionExistsButIsInactive_ShouldReturnFalse()
    {
        // Arrange
        _repository.GetUserPermissionsAsync(1).Returns(new[]
        {
            Permission("terceros.crear", "I")
        });
        var sut = CreateSut();

        // Act
        var response = await sut.UserHasPermissionAsync(1, "terceros.crear");

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeFalse();
    }

    [Fact]
    public async Task UserHasPermissionAsync_WhenPermissionUsesDifferentCase_ShouldReturnTrue()
    {
        // Arrange
        _repository.GetUserPermissionsAsync(1).Returns(new[]
        {
            Permission("terceros.crear", "A")
        });
        var sut = CreateSut();

        // Act
        var response = await sut.UserHasPermissionAsync(1, "Terceros.Crear");

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasPermissionAsync_WhenPermissionContainsHyphen_ShouldReturnTrue()
    {
        // Arrange
        _repository.GetUserPermissionsAsync(1).Returns(new[]
        {
            Permission("terceros-global.listar", "A")
        });
        var sut = CreateSut();

        // Act
        var response = await sut.UserHasPermissionAsync(1, "terceros-global.listar");

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeTrue();
    }

    [Fact]
    public async Task UserHasPermissionAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        _repository.GetUserPermissionsAsync(1).Returns<Task<IEnumerable<RbacUserPermission>>>(_ => throw new InvalidOperationException("database unavailable"));
        var sut = CreateSut();

        // Act
        var response = await sut.UserHasPermissionAsync(1, "terceros.crear");

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeFalse();
        response.Message.Should().Be("database unavailable");
    }

    private RbacApplication CreateSut()
    {
        return new RbacApplication(_repository, _mapper);
    }

    private static RbacUserPermission Permission(string permissionName, string status)
    {
        var parts = permissionName.Split('.');
        return new RbacUserPermission
        {
            UserId = 1,
            PermissionId = 10,
            PermissionName = permissionName,
            ModuleCode = parts[0],
            ActionCode = parts[1],
            PermissionStatus = status
        };
    }
}
