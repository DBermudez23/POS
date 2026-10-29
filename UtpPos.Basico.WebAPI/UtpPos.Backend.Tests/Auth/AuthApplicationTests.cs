using FluentAssertions;
using UtpPos.Framework.Application.DTO.Auth;
using UtpPos.Framework.Application.Main.Auth;
using UtpPos.Framework.Domain.Entity.Auth;
using UtpPos.Framework.InfraStructure.Interface.Auth;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using UtpPos.Framework.Transversal.Interface.Auth;
using NSubstitute;

namespace UtpPos.Backend.Tests.Auth;

public sealed class AuthApplicationTests
{
    private readonly IAuthRepository _repository = Substitute.For<IAuthRepository>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IRefreshTokenService _refreshTokenService = Substitute.For<IRefreshTokenService>();
    private readonly IPasswordHasherService _passwordHasherService = Substitute.For<IPasswordHasherService>();
    private readonly IRefreshTokenHashService _refreshTokenHashService = Substitute.For<IRefreshTokenHashService>();
    private readonly IAppLogger<AuthApplication> _logger = Substitute.For<IAppLogger<AuthApplication>>();

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ShouldReturnTokens()
    {
        // Arrange
        var user = CreateActiveUser();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

        _repository.GetUserForAuthenticationAsync(user.Email).Returns(user);
        _passwordHasherService.Verify("secret", user.PasswordHash).Returns(true);
        _refreshTokenService.GenerateToken().Returns(new RefreshTokenResult
        {
            Token = "refresh-token",
            ExpiresAtUtc = refreshTokenExpiresAt
        });
        _refreshTokenHashService.Hash("refresh-token").Returns("hashed-refresh-token");
        _repository.RegisterUserSessionAsync(
                user.Id,
                "hashed-refresh-token",
                refreshTokenExpiresAt,
                "127.0.0.1",
                "unit-test")
            .Returns(55);
        _jwtTokenService.GenerateAccessToken(user, 55).Returns(new AccessTokenResult
        {
            Token = "access-token",
            ExpiresAtUtc = accessTokenExpiresAt,
            ExpiresIn = 900
        });

        var sut = CreateSut();

        // Act
        var response = await sut.LoginAsync(
            new LoginRequestDto { Email = user.Email, Password = "secret" },
            "127.0.0.1",
            "unit-test");

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.AccessToken.Should().Be("access-token");
        response.Data.RefreshToken.Should().Be("refresh-token");
        response.Data.ExpiresIn.Should().Be(900);
        response.Data.Session.Should().NotBeNull();
        response.Data.Session!.Id.Should().Be(55);
        response.Data.User.Should().NotBeNull();
        response.Data.User!.Id.Should().Be(user.Id);

        _refreshTokenHashService.Received(1).Hash("refresh-token");
        await _repository.Received(1).RegisterUserSessionAsync(
            user.Id,
            "hashed-refresh-token",
            refreshTokenExpiresAt,
            "127.0.0.1",
            "unit-test");
        _jwtTokenService.Received(1).GenerateAccessToken(user, 55);
    }

    [Theory]
    [MemberData(nameof(InvalidLoginRequests))]
    public async Task LoginAsync_WhenRequestIsInvalid_ShouldReturnControlledError(LoginRequestDto? request, string expectedMessage)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var response = await sut.LoginAsync(request!, null, null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be(expectedMessage);
        await _repository.DidNotReceive().GetUserForAuthenticationAsync(Arg.Any<string>());
        _refreshTokenService.DidNotReceive().GenerateToken();
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ShouldNotGenerateTokensOrRegisterSession()
    {
        // Arrange
        _repository.GetUserForAuthenticationAsync("missing@test.com").Returns((AuthUser?)null);
        var sut = CreateSut();

        // Act
        var response = await sut.LoginAsync(
            new LoginRequestDto { Email = "missing@test.com", Password = "secret" },
            null,
            null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Correo o contraseña incorrectos.");
        _passwordHasherService.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        _refreshTokenService.DidNotReceive().GenerateToken();
        await _repository.DidNotReceive().RegisterUserSessionAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<string?>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsInactive_ShouldNotGenerateTokensOrRegisterSession()
    {
        // Arrange
        var user = CreateActiveUser(status: "I");
        _repository.GetUserForAuthenticationAsync(user.Email).Returns(user);
        var sut = CreateSut();

        // Act
        var response = await sut.LoginAsync(
            new LoginRequestDto { Email = user.Email, Password = "secret" },
            null,
            null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Correo o contraseña incorrectos.");
        _passwordHasherService.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        _refreshTokenService.DidNotReceive().GenerateToken();
        await _repository.DidNotReceive().RegisterUserSessionAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<string?>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ShouldNotGenerateTokensOrRegisterSession()
    {
        // Arrange
        var user = CreateActiveUser();
        _repository.GetUserForAuthenticationAsync(user.Email).Returns(user);
        _passwordHasherService.Verify("bad-password", user.PasswordHash).Returns(false);
        var sut = CreateSut();

        // Act
        var response = await sut.LoginAsync(
            new LoginRequestDto { Email = user.Email, Password = "bad-password" },
            null,
            null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("Correo o contraseña incorrectos.");
        _refreshTokenService.DidNotReceive().GenerateToken();
        await _repository.DidNotReceive().RegisterUserSessionAsync(
            Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<string?>(), Arg.Any<string?>());
    }

    [Fact]
    public async Task LoginAsync_WhenIpAndUserAgentAreNullOrLong_ShouldNormalizeWithoutFailing()
    {
        // Arrange
        var user = CreateActiveUser();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        var longIp = new string('1', 80);
        var longUserAgent = new string('a', 300);

        _repository.GetUserForAuthenticationAsync(user.Email).Returns(user);
        _passwordHasherService.Verify("secret", user.PasswordHash).Returns(true);
        _refreshTokenService.GenerateToken().Returns(new RefreshTokenResult
        {
            Token = "refresh-token",
            ExpiresAtUtc = refreshTokenExpiresAt
        });
        _refreshTokenHashService.Hash("refresh-token").Returns("hashed-refresh-token");
        _repository.RegisterUserSessionAsync(
                user.Id,
                "hashed-refresh-token",
                refreshTokenExpiresAt,
                Arg.Is<string?>(value => value != null && value.Length == 45),
                Arg.Is<string?>(value => value != null && value.Length == 250))
            .Returns(55);
        _jwtTokenService.GenerateAccessToken(user, 55).Returns(new AccessTokenResult
        {
            Token = "access-token",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
            ExpiresIn = 900
        });

        var sut = CreateSut();

        // Act
        var response = await sut.LoginAsync(
            new LoginRequestDto { Email = user.Email, Password = "secret" },
            longIp,
            longUserAgent);

        // Assert
        response.IsSuccess.Should().BeTrue();
        await _repository.Received(1).RegisterUserSessionAsync(
            user.Id,
            "hashed-refresh-token",
            refreshTokenExpiresAt,
            Arg.Is<string?>(value => value != null && value.Length == 45),
            Arg.Is<string?>(value => value != null && value.Length == 250));
    }

    [Fact]
    public async Task RefreshAsync_WhenRefreshTokenIsEmpty_ShouldReturnError()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var response = await sut.RefreshAsync(new RefreshTokenRequestDto { RefreshToken = " " }, null, null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("El refresh token es obligatorio.");
        _refreshTokenHashService.DidNotReceive().Hash(Arg.Any<string>());
    }

    [Fact]
    public async Task RefreshAsync_WhenSessionDoesNotExist_ShouldReturnError()
    {
        // Arrange
        _refreshTokenHashService.Hash("old-refresh-token").Returns("old-refresh-hash");
        _repository.GetSessionByRefreshTokenHashAsync("old-refresh-hash").Returns((UserSession?)null);
        var sut = CreateSut();

        // Act
        var response = await sut.RefreshAsync(
            new RefreshTokenRequestDto { RefreshToken = "old-refresh-token" },
            null,
            null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("El refresh token no es válido, está revocado o expiró.");
        _refreshTokenService.DidNotReceive().GenerateToken();
    }

    [Theory]
    [InlineData(true, 10)]
    [InlineData(false, -10)]
    public async Task RefreshAsync_WhenSessionIsRevokedOrExpired_ShouldReturnError(bool revoked, int expirationOffsetMinutes)
    {
        // Arrange
        var session = CreateValidSession();
        session.Revoked = revoked;
        session.ExpirationDate = DateTime.UtcNow.AddMinutes(expirationOffsetMinutes);

        _refreshTokenHashService.Hash("old-refresh-token").Returns("old-refresh-hash");
        _repository.GetSessionByRefreshTokenHashAsync("old-refresh-hash").Returns(session);
        var sut = CreateSut();

        // Act
        var response = await sut.RefreshAsync(
            new RefreshTokenRequestDto { RefreshToken = "old-refresh-token" },
            null,
            null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("El refresh token no es válido, está revocado o expiró.");
        _refreshTokenService.DidNotReceive().GenerateToken();
    }

    [Fact]
    public async Task RefreshAsync_WhenSessionIsValid_ShouldRotateRefreshTokenAndReturnNewTokens()
    {
        // Arrange
        var session = CreateValidSession();
        var newRefreshExpiresAt = DateTime.UtcNow.AddDays(7);
        var newAccessExpiresAt = DateTime.UtcNow.AddMinutes(15);

        _refreshTokenHashService.Hash("old-refresh-token").Returns("old-refresh-hash");
        _refreshTokenHashService.Hash("new-refresh-token").Returns("new-refresh-hash");
        _repository.GetSessionByRefreshTokenHashAsync("old-refresh-hash").Returns(session);
        _refreshTokenService.GenerateToken().Returns(new RefreshTokenResult
        {
            Token = "new-refresh-token",
            ExpiresAtUtc = newRefreshExpiresAt
        });
        _jwtTokenService.GenerateAccessToken(Arg.Any<AuthUser>(), session.Id).Returns(new AccessTokenResult
        {
            Token = "new-access-token",
            ExpiresAtUtc = newAccessExpiresAt,
            ExpiresIn = 900
        });
        _repository.RotateRefreshTokenAsync(
                session.Id,
                "new-refresh-hash",
                newRefreshExpiresAt,
                "127.0.0.1",
                "unit-test")
            .Returns(true);

        var sut = CreateSut();

        // Act
        var response = await sut.RefreshAsync(
            new RefreshTokenRequestDto { RefreshToken = "old-refresh-token" },
            "127.0.0.1",
            "unit-test");

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.AccessToken.Should().Be("new-access-token");
        response.Data.RefreshToken.Should().Be("new-refresh-token");
        await _repository.Received(1).RotateRefreshTokenAsync(
            session.Id,
            "new-refresh-hash",
            newRefreshExpiresAt,
            "127.0.0.1",
            "unit-test");
        _jwtTokenService.Received(1).GenerateAccessToken(Arg.Any<AuthUser>(), session.Id);
    }

    [Fact]
    public async Task LogoutAsync_WhenRefreshTokenIsEmpty_ShouldReturnError()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var response = await sut.LogoutAsync(new LogoutRequestDto { RefreshToken = " " });

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("El refresh token es obligatorio.");
        _refreshTokenHashService.DidNotReceive().Hash(Arg.Any<string>());
    }

    [Fact]
    public async Task LogoutAsync_WhenRefreshTokenIsValid_ShouldRevokeSessionByHash()
    {
        // Arrange
        _refreshTokenHashService.Hash("refresh-token").Returns("refresh-hash");
        _repository.RevokeSessionByRefreshTokenHashAsync("refresh-hash").Returns(true);
        var sut = CreateSut();

        // Act
        var response = await sut.LogoutAsync(new LogoutRequestDto { RefreshToken = "refresh-token" });

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeTrue();
        await _repository.Received(1).RevokeSessionByRefreshTokenHashAsync("refresh-hash");
    }

    [Fact]
    public async Task LogoutAllAsync_WhenUserIdIsInvalid_ShouldReturnError()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var response = await sut.LogoutAllAsync(0);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("El usuario autenticado no es válido.");
        await _repository.DidNotReceive().RevokeAllUserSessionsAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task LogoutAllAsync_WhenUserIdIsValid_ShouldRevokeAllUserSessions()
    {
        // Arrange
        _repository.RevokeAllUserSessionsAsync(7).Returns(3);
        var sut = CreateSut();

        // Act
        var response = await sut.LogoutAllAsync(7);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeTrue();
        response.Message.Should().Be("Sesiones cerradas correctamente: 3.");
        await _repository.Received(1).RevokeAllUserSessionsAsync(7);
    }

    [Fact]
    public async Task GetMeAsync_WhenUserIdIsInvalid_ShouldReturnError()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var response = await sut.GetMeAsync(0);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("El usuario autenticado no es válido.");
        await _repository.DidNotReceive().GetUserProfileAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task GetMeAsync_WhenUserDoesNotExist_ShouldReturnError()
    {
        // Arrange
        _repository.GetUserProfileAsync(7).Returns((AuthUser?)null);
        var sut = CreateSut();

        // Act
        var response = await sut.GetMeAsync(7);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("El usuario no existe o se encuentra inactivo.");
    }

    [Fact]
    public async Task GetMeAsync_WhenUserIsInactive_ShouldReturnError()
    {
        // Arrange
        _repository.GetUserProfileAsync(7).Returns(CreateActiveUser(id: 7, status: "I"));
        var sut = CreateSut();

        // Act
        var response = await sut.GetMeAsync(7);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be("El usuario no existe o se encuentra inactivo.");
    }

    [Fact]
    public async Task GetMeAsync_WhenUserIsValid_ShouldReturnProfile()
    {
        // Arrange
        var user = CreateActiveUser(id: 7, name: "Valid User");
        _repository.GetUserProfileAsync(7).Returns(user);
        var sut = CreateSut();

        // Act
        var response = await sut.GetMeAsync(7);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(7);
        response.Data.Name.Should().Be("Valid User");
        response.Data.Email.Should().Be(user.Email);
    }

    public static IEnumerable<object?[]> InvalidLoginRequests()
    {
        yield return new object?[] { null, "El correo es obligatorio." };
        yield return new object?[] { new LoginRequestDto { Email = " ", Password = "secret" }, "El correo es obligatorio." };
        yield return new object?[] { new LoginRequestDto { Email = "user@test.com", Password = " " }, "La contraseña es obligatoria." };
        yield return new object?[] { new LoginRequestDto { Email = new string('a', 81), Password = "secret" }, "El correo no puede superar 80 caracteres." };
    }

    private AuthApplication CreateSut()
    {
        return new AuthApplication(
            _repository,
            _jwtTokenService,
            _refreshTokenService,
            _passwordHasherService,
            _refreshTokenHashService,
            _logger);
    }

    private static AuthUser CreateActiveUser(int id = 1, string name = "Test User", string status = "A")
    {
        return new AuthUser
        {
            Id = id,
            BusinessId = 10,
            BusinessName = "Main Business",
            DocumentTypeId = 2,
            DocumentType = "CC",
            DocumentNumber = "123",
            Name = name,
            Email = "user@test.com",
            PasswordHash = "hashed-password",
            Phone = "555",
            Status = status,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
    }

    private static UserSession CreateValidSession()
    {
        return new UserSession
        {
            Id = 99,
            UserId = 1,
            BusinessId = 10,
            BusinessName = "Main Business",
            UserName = "Test User",
            Email = "user@test.com",
            UserStatus = "A",
            RefreshTokenHash = "old-refresh-hash",
            ExpirationDate = DateTime.UtcNow.AddMinutes(30),
            Revoked = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }
}
