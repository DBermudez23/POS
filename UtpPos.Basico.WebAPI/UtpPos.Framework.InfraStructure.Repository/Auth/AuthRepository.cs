using System.Data;
using Dapper;
using UtpPos.Framework.Domain.Entity.Auth;
using UtpPos.Framework.InfraStructure.Interface.Auth;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Interface;

namespace UtpPos.Framework.InfraStructure.Repository.Auth
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public AuthRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<AuthUser?> GetUserForAuthenticationAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USR_CORREO", email, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<AuthUserDbModel>(
                "dbo.UspConsultaUsuarioAutenticacion",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapUser(result);
        }

        public async Task<AuthUser?> GetUserProfileAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USR_ID", userId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<AuthUserDbModel>(
                "dbo.UspConsultaPerfilUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapUser(result);
        }

        public async Task<int> RegisterUserSessionAsync(
            int userId,
            string refreshTokenHash,
            DateTime expirationDate,
            string? ip,
            string? userAgent)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SES_USR_ID", userId, DbType.Int32);
            parameters.Add("@SES_REFRESH_TOKEN_HASH", refreshTokenHash, DbType.String);
            parameters.Add("@SES_FECHA_EXPIRACION", expirationDate, DbType.DateTime);
            parameters.Add("@SES_IP", EmptyToNull(ip), DbType.String);
            parameters.Add("@SES_USER_AGENT", EmptyToNull(userAgent), DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<SessionOperationResult>(
                "dbo.UspRegistraSesionUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return result!.SES_ID;
        }

        public async Task<UserSession?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SES_REFRESH_TOKEN_HASH", refreshTokenHash, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<UserSessionDbModel>(
                "dbo.UspConsultaSesionPorRefreshToken",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapSession(result);
        }

        public async Task<bool> RotateRefreshTokenAsync(
            int sessionId,
            string newRefreshTokenHash,
            DateTime newExpirationDate,
            string? ip,
            string? userAgent)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SES_ID", sessionId, DbType.Int32);
            parameters.Add("@SES_REFRESH_TOKEN_HASH_NUEVO", newRefreshTokenHash, DbType.String);
            parameters.Add("@SES_FECHA_EXPIRACION_NUEVA", newExpirationDate, DbType.DateTime);
            parameters.Add("@SES_IP", EmptyToNull(ip), DbType.String);
            parameters.Add("@SES_USER_AGENT", EmptyToNull(userAgent), DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<SessionOperationResult>(
                "dbo.UspRotaRefreshTokenSesion",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return true;
        }

        public async Task<bool> RevokeSessionByRefreshTokenHashAsync(string refreshTokenHash)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SES_REFRESH_TOKEN_HASH", refreshTokenHash, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<SessionOperationResult>(
                "dbo.UspRevocaSesionPorRefreshToken",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return true;
        }

        public async Task<bool> RevokeUserSessionAsync(int sessionId, int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SES_ID", sessionId, DbType.Int32);
            parameters.Add("@SES_USR_ID", userId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<SessionOperationResult>(
                "dbo.UspRevocaSesionUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return true;
        }

        public async Task<int> RevokeAllUserSessionsAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@SES_USR_ID", userId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<SessionOperationResult>(
                "dbo.UspRevocaSesionesUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return result!.SESIONES_REVOCADAS;
        }

        public async Task<int> CleanupExpiredSessionsAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QueryFirstOrDefaultAsync<SessionOperationResult>(
                "dbo.UspLimpiaSesionesExpiradas",
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return result!.SESIONES_REVOCADAS;
        }

        private static AuthUser MapUser(AuthUserDbModel model)
        {
            return new AuthUser
            {
                Id = model.USR_ID,
                BusinessId = model.USR_NEG_ID,
                BusinessName = model.NEG_NOMBRE,
                DocumentTypeId = model.USR_TIPO_DOC_ID,
                DocumentType = model.TIPO_DOCUMENTO ?? string.Empty,
                DocumentNumber = model.USR_NUMERO_DOCUMENTO ?? string.Empty,
                Name = model.USR_NOMBRE ?? string.Empty,
                Email = model.USR_CORREO ?? string.Empty,
                PasswordHash = model.USR_PASSWORD_HASH ?? string.Empty,
                Phone = model.USR_TELEFONO,
                Status = model.USR_ESTADO ?? string.Empty,
                CreatedAt = model.USR_FECHA_CREACION
            };
        }

        private static UserSession MapSession(UserSessionDbModel model)
        {
            return new UserSession
            {
                Id = model.SES_ID,
                UserId = model.SES_USR_ID,
                BusinessId = model.USR_NEG_ID,
                BusinessName = model.NEG_NOMBRE,
                UserName = model.USR_NOMBRE ?? string.Empty,
                Email = model.USR_CORREO ?? string.Empty,
                UserStatus = model.USR_ESTADO ?? string.Empty,
                RefreshTokenHash = model.SES_REFRESH_TOKEN_HASH ?? string.Empty,
                ExpirationDate = model.SES_FECHA_EXPIRACION,
                Ip = model.SES_IP,
                UserAgent = model.SES_USER_AGENT,
                Revoked = string.Equals(model.SES_REVOCADA, "S", StringComparison.OrdinalIgnoreCase),
                CreatedAt = model.SES_FECHA_CREACION,
                RevokedAt = model.SES_FECHA_REVOCACION
            };
        }

        private static void EnsureSuccessful(int? successful, string? message)
        {
            if (successful is null)
                throw new InvalidOperationException("El stored procedure no retornó un resultado de operación.");

            if (successful != 1)
                throw new InvalidOperationException(message ?? "El stored procedure reportó una operación fallida.");
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class AuthUserDbModel
        {
            public int USR_ID { get; set; }
            public int? USR_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public int USR_TIPO_DOC_ID { get; set; }
            public string? TIPO_DOCUMENTO { get; set; }
            public string? USR_NUMERO_DOCUMENTO { get; set; }
            public string? USR_NOMBRE { get; set; }
            public string? USR_CORREO { get; set; }
            public string? USR_PASSWORD_HASH { get; set; }
            public string? USR_TELEFONO { get; set; }
            public string? USR_ESTADO { get; set; }
            public DateTime USR_FECHA_CREACION { get; set; }
        }

        private sealed class UserSessionDbModel
        {
            public int SES_ID { get; set; }
            public int SES_USR_ID { get; set; }
            public int? USR_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public string? USR_NOMBRE { get; set; }
            public string? USR_CORREO { get; set; }
            public string? USR_ESTADO { get; set; }
            public string? SES_REFRESH_TOKEN_HASH { get; set; }
            public DateTime SES_FECHA_EXPIRACION { get; set; }
            public string? SES_IP { get; set; }
            public string? SES_USER_AGENT { get; set; }
            public string? SES_REVOCADA { get; set; }
            public DateTime SES_FECHA_CREACION { get; set; }
            public DateTime? SES_FECHA_REVOCACION { get; set; }
        }

        private sealed class SessionOperationResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int SES_ID { get; set; }
            public int SESIONES_REVOCADAS { get; set; }
        }
    }
}
