using UtpPos.Framework.Application.DTO.Auth;
using UtpPos.Framework.Application.DTO.User;
using UtpPos.Framework.Application.Interface.Auth;
using UtpPos.Framework.Domain.Entity.Auth;
using UtpPos.Framework.InfraStructure.Interface.Auth;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using UtpPos.Framework.Transversal.Interface.Auth;

namespace UtpPos.Framework.Application.Main.Auth
{
    public class AuthApplication : IAuthApplication
    {
        private const string InvalidCredentialsMessage = "Correo o contraseña incorrectos.";
        private const string InvalidSessionMessage = "El refresh token no es válido, está revocado o expiró.";
        private const string UnexpectedErrorMessage = "No fue posible completar la operación de autenticación.";

        private readonly IAuthRepository _repository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IPasswordHasherService _passwordHasherService;
        private readonly IRefreshTokenHashService _refreshTokenHashService;
        private readonly IAppLogger<AuthApplication> _logger;

        public AuthApplication(
            IAuthRepository repository,
            IJwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            IPasswordHasherService passwordHasherService,
            IRefreshTokenHashService refreshTokenHashService,
            IAppLogger<AuthApplication> logger)
        {
            _repository = repository;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _passwordHasherService = passwordHasherService;
            _refreshTokenHashService = refreshTokenHashService;
            _logger = logger;
        }

        public async Task<Response<LoginResponseDto>> LoginAsync(
            LoginRequestDto request,
            string? ip,
            string? userAgent)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Email))
                return Fail<LoginResponseDto>("El correo es obligatorio.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return Fail<LoginResponseDto>("La contraseña es obligatoria.");

            var email = request.Email.Trim();
            if (email.Length > 80)
                return Fail<LoginResponseDto>("El correo no puede superar 80 caracteres.");

            try
            {
                var user = await _repository.GetUserForAuthenticationAsync(email);
                if (user is null || !IsActive(user.Status))
                    return Fail<LoginResponseDto>(InvalidCredentialsMessage);

                if (!_passwordHasherService.Verify(request.Password, user.PasswordHash))
                    return Fail<LoginResponseDto>(InvalidCredentialsMessage);

                var normalizedIp = NormalizeClientValue(ip, 45);
                var normalizedUserAgent = NormalizeClientValue(userAgent, 250);
                var refreshToken = _refreshTokenService.GenerateToken();
                var refreshTokenHash = _refreshTokenHashService.Hash(refreshToken.Token);
                var sessionId = await _repository.RegisterUserSessionAsync(
                    user.Id,
                    refreshTokenHash,
                    refreshToken.ExpiresAtUtc,
                    normalizedIp,
                    normalizedUserAgent);

                var accessToken = _jwtTokenService.GenerateAccessToken(user, sessionId);
                var session = new AuthSessionDto
                {
                    Id = sessionId,
                    UserId = user.Id,
                    ExpirationDate = refreshToken.ExpiresAtUtc,
                    Ip = normalizedIp,
                    UserAgent = normalizedUserAgent,
                    CreatedAt = DateTime.UtcNow
                };

                return Success(
                    new LoginResponseDto
                    {
                        AccessToken = accessToken.Token,
                        RefreshToken = refreshToken.Token,
                        ExpiresIn = accessToken.ExpiresIn,
                        AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
                        RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
                        User = MapUser(user),
                        Session = session
                    },
                    "Inicio de sesión exitoso.");
            }
            catch (InvalidOperationException ex)
            {
                return Fail<LoginResponseDto>(ex.Message);
            }
            catch (Exception ex)
            {
                LogUnexpectedError(ex);
                return Fail<LoginResponseDto>(UnexpectedErrorMessage);
            }
        }

        public async Task<Response<RefreshTokenResponseDto>> RefreshAsync(
            RefreshTokenRequestDto request,
            string? ip,
            string? userAgent)
        {
            var validation = ValidateRefreshToken(request?.RefreshToken);
            if (validation is not null)
                return Fail<RefreshTokenResponseDto>(validation);

            try
            {
                var currentHash = _refreshTokenHashService.Hash(request!.RefreshToken.Trim());
                var session = await _repository.GetSessionByRefreshTokenHashAsync(currentHash);
                if (!IsValidSession(session))
                    return Fail<RefreshTokenResponseDto>(InvalidSessionMessage);

                var user = MapSessionUser(session!);
                var newRefreshToken = _refreshTokenService.GenerateToken();
                var newRefreshTokenHash = _refreshTokenHashService.Hash(newRefreshToken.Token);
                var accessToken = _jwtTokenService.GenerateAccessToken(user, session!.Id);
                var normalizedIp = NormalizeClientValue(ip, 45);
                var normalizedUserAgent = NormalizeClientValue(userAgent, 250);

                var rotated = await _repository.RotateRefreshTokenAsync(
                    session.Id,
                    newRefreshTokenHash,
                    newRefreshToken.ExpiresAtUtc,
                    normalizedIp,
                    normalizedUserAgent);

                if (!rotated)
                    return Fail<RefreshTokenResponseDto>(InvalidSessionMessage);

                return Success(
                    new RefreshTokenResponseDto
                    {
                        AccessToken = accessToken.Token,
                        RefreshToken = newRefreshToken.Token,
                        ExpiresIn = accessToken.ExpiresIn,
                        AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
                        RefreshTokenExpiresAtUtc = newRefreshToken.ExpiresAtUtc,
                        Session = new AuthSessionDto
                        {
                            Id = session.Id,
                            UserId = session.UserId,
                            ExpirationDate = newRefreshToken.ExpiresAtUtc,
                            Ip = normalizedIp,
                            UserAgent = normalizedUserAgent,
                            CreatedAt = session.CreatedAt
                        }
                    },
                    "Tokens renovados correctamente.");
            }
            catch (InvalidOperationException)
            {
                return Fail<RefreshTokenResponseDto>(InvalidSessionMessage);
            }
            catch (Exception ex)
            {
                LogUnexpectedError(ex);
                return Fail<RefreshTokenResponseDto>(UnexpectedErrorMessage);
            }
        }

        public async Task<Response<bool>> LogoutAsync(LogoutRequestDto request)
        {
            var validation = ValidateRefreshToken(request?.RefreshToken);
            if (validation is not null)
                return Fail<bool>(validation);

            try
            {
                var hash = _refreshTokenHashService.Hash(request!.RefreshToken.Trim());
                var revoked = await _repository.RevokeSessionByRefreshTokenHashAsync(hash);
                return revoked
                    ? Success(true, "Sesión cerrada correctamente.")
                    : Fail<bool>(InvalidSessionMessage);
            }
            catch (InvalidOperationException)
            {
                return Fail<bool>(InvalidSessionMessage);
            }
            catch (Exception ex)
            {
                LogUnexpectedError(ex);
                return Fail<bool>(UnexpectedErrorMessage);
            }
        }

        public async Task<Response<bool>> LogoutAllAsync(int userId)
        {
            if (userId <= 0)
                return Fail<bool>("El usuario autenticado no es válido.");

            try
            {
                var revokedSessions = await _repository.RevokeAllUserSessionsAsync(userId);
                return Success(true, $"Sesiones cerradas correctamente: {revokedSessions}.");
            }
            catch (InvalidOperationException ex)
            {
                return Fail<bool>(ex.Message);
            }
            catch (Exception ex)
            {
                LogUnexpectedError(ex);
                return Fail<bool>(UnexpectedErrorMessage);
            }
        }

        public async Task<Response<UserDto>> GetMeAsync(int userId)
        {
            if (userId <= 0)
                return Fail<UserDto>("El usuario autenticado no es válido.");

            try
            {
                var user = await _repository.GetUserProfileAsync(userId);
                if (user is null || !IsActive(user.Status))
                    return Fail<UserDto>("El usuario no existe o se encuentra inactivo.");

                return Success(MapUser(user), "Perfil consultado correctamente.");
            }
            catch (Exception ex)
            {
                LogUnexpectedError(ex);
                return Fail<UserDto>(UnexpectedErrorMessage);
            }
        }

        private static string? ValidateRefreshToken(string? refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return "El refresh token es obligatorio.";

            return refreshToken.Trim().Length > 1024
                ? "El refresh token no es válido."
                : null;
        }

        private static bool IsValidSession(UserSession? session)
        {
            return session is not null &&
                   session.Id > 0 &&
                   session.UserId > 0 &&
                   !session.Revoked &&
                   session.ExpirationDate > DateTime.UtcNow &&
                   IsActive(session.UserStatus);
        }

        private static bool IsActive(string? status)
        {
            return string.Equals(status?.Trim(), "A", StringComparison.OrdinalIgnoreCase);
        }

        private static AuthUser MapSessionUser(UserSession session)
        {
            return new AuthUser
            {
                Id = session.UserId,
                BusinessId = session.BusinessId,
                BusinessName = session.BusinessName,
                Name = session.UserName,
                Email = session.Email,
                Status = session.UserStatus
            };
        }

        private static UserDto MapUser(AuthUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                BusinessId = user.BusinessId,
                BusinessName = user.BusinessName,
                DocumentTypeId = user.DocumentTypeId,
                DocumentType = user.DocumentType,
                DocumentNumber = user.DocumentNumber,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status,
                CreatedAt = user.CreatedAt
            };
        }

        private static string? NormalizeClientValue(string? value, int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var normalized = value.Trim();
            return normalized.Length <= maximumLength
                ? normalized
                : normalized[..maximumLength];
        }

        private void LogUnexpectedError(Exception exception)
        {
            _logger.LogError("Error inesperado en autenticación: {Error}", exception.ToString());
        }

        private static Response<T> Success<T>(T data, string message)
        {
            return new Response<T> { IsSuccess = true, Data = data, Message = message };
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T> { IsSuccess = false, Data = default, Message = message };
        }
    }
}
