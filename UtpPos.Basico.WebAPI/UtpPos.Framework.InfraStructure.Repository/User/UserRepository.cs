using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.User;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Interface;
using DomainUser = UtpPos.Framework.Domain.Entity.User.User;

namespace UtpPos.Framework.InfraStructure.Repository.User
{
    public class UserRepository : IUserRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public UserRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainUser>> ListAsync(
            int? businessId,
            int? documentTypeId,
            string? status,
            string? documentNumber,
            string? name,
            string? email)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USR_NEG_ID", businessId, DbType.Int32);
            parameters.Add("@USR_TIPO_DOC_ID", documentTypeId, DbType.Int32);
            parameters.Add("@USR_ESTADO", EmptyToNull(status), DbType.String);
            parameters.Add("@USR_NUMERO_DOCUMENTO", EmptyToNull(documentNumber), DbType.String);
            parameters.Add("@USR_NOMBRE", EmptyToNull(name), DbType.String);
            parameters.Add("@USR_CORREO", EmptyToNull(email), DbType.String);

            var results = await connection.QueryAsync<UserDbModel>(
                "dbo.UspListaUsuarios",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapToDomain).ToList();
        }

        public async Task<DomainUser?> GetByIdAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USR_ID", userId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<UserDbModel>(
                "dbo.UspConsultaUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapToDomain(result);
        }

        public async Task<int> CreateAsync(DomainUser user)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USR_NEG_ID", user.BusinessId, DbType.Int32);
            parameters.Add("@USR_TIPO_DOC_ID", user.DocumentTypeId, DbType.Int32);
            parameters.Add("@USR_NUMERO_DOCUMENTO", user.DocumentNumber, DbType.String);
            parameters.Add("@USR_NOMBRE", user.Name, DbType.String);
            parameters.Add("@USR_CORREO", user.Email, DbType.String);
            parameters.Add("@USR_PASSWORD", user.PasswordHash, DbType.String);
            parameters.Add("@USR_TELEFONO", EmptyToNull(user.Phone), DbType.String);
            parameters.Add("@USR_ESTADO", user.Status, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<UserWriteResult>(
                "dbo.UspCreaUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result is null)
                throw new InvalidOperationException("El stored procedure no retornó un resultado de operación.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(
                    result.Mensaje ?? "El stored procedure reportó una operación fallida.");

            return result.USR_ID
                ?? throw new InvalidOperationException("El stored procedure no retornó USR_ID.");
        }

        public async Task<int> UpdateAsync(DomainUser user, bool clearBusinessId, bool clearPhone)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USR_ID", user.Id, DbType.Int32);
            parameters.Add("@USR_NEG_ID", user.BusinessId, DbType.Int32);
            parameters.Add("@USR_TIPO_DOC_ID", user.DocumentTypeId == 0 ? null : user.DocumentTypeId, DbType.Int32);
            parameters.Add("@USR_NUMERO_DOCUMENTO", EmptyToNull(user.DocumentNumber), DbType.String);
            parameters.Add("@USR_NOMBRE", EmptyToNull(user.Name), DbType.String);
            parameters.Add("@USR_CORREO", EmptyToNull(user.Email), DbType.String);
            parameters.Add("@USR_PASSWORD", EmptyToNull(user.PasswordHash), DbType.String);
            parameters.Add("@USR_TELEFONO", EmptyToNull(user.Phone), DbType.String);
            parameters.Add("@USR_ESTADO", EmptyToNull(user.Status), DbType.String);
            parameters.Add("@LIMPIAR_NEGOCIO", clearBusinessId, DbType.Boolean);
            parameters.Add("@LIMPIAR_TELEFONO", clearPhone, DbType.Boolean);

            var result = await connection.QueryFirstOrDefaultAsync<UserWriteResult>(
                "dbo.UspActualizaUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result is null)
                throw new InvalidOperationException("El stored procedure no retorno un resultado de operacion.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(
                    result.Mensaje ?? "El stored procedure reporto una operacion fallida.");

            return result.USR_ID
                ?? throw new InvalidOperationException("El stored procedure no retorno USR_ID.");
        }

        public async Task<int> DeactivateAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USR_ID", userId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<UserWriteResult>(
                "dbo.UspEliminaUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result is null)
                throw new InvalidOperationException("El stored procedure no retorno un resultado de operacion.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(
                    result.Mensaje ?? "El stored procedure reporto una operacion fallida.");

            return result.USR_ID
                ?? throw new InvalidOperationException("El stored procedure no retorno USR_ID.");
        }

        private static DomainUser MapToDomain(UserDbModel model)
        {
            return new DomainUser
            {
                Id = model.USR_ID,
                BusinessId = model.USR_NEG_ID,
                BusinessName = model.NEG_NOMBRE,
                DocumentTypeId = model.USR_TIPO_DOC_ID,
                DocumentType = model.TIPO_DOCUMENTO ?? string.Empty,
                DocumentNumber = model.USR_NUMERO_DOCUMENTO ?? string.Empty,
                Name = model.USR_NOMBRE ?? string.Empty,
                Email = model.USR_CORREO ?? string.Empty,
                Phone = model.USR_TELEFONO,
                Status = model.USR_ESTADO ?? string.Empty,
                CreatedAt = model.USR_FECHA_CREACION
            };
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class UserDbModel
        {
            public int USR_ID { get; set; }
            public int? USR_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public int USR_TIPO_DOC_ID { get; set; }
            public string? TIPO_DOCUMENTO { get; set; }
            public string? USR_NUMERO_DOCUMENTO { get; set; }
            public string? USR_NOMBRE { get; set; }
            public string? USR_CORREO { get; set; }
            public string? USR_TELEFONO { get; set; }
            public string? USR_ESTADO { get; set; }
            public DateTime USR_FECHA_CREACION { get; set; }
        }

        private sealed class UserWriteResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? USR_ID { get; set; }
        }
    }
}
