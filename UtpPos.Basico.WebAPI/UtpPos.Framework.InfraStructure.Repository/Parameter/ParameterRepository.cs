using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.Parameter;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Interface;
using DomainParameter = UtpPos.Framework.Domain.Entity.Parameter.Parameter;

namespace UtpPos.Framework.InfraStructure.Repository.Parameter
{
    public class ParameterRepository : IParameterRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public ParameterRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainParameter>> ListAsync(int? classId, string? classDescription, string? description, string? status)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PARAM_CLASE_ID", classId, DbType.Int32);
            parameters.Add("@PARAM_ESTADO", EmptyToNull(status), DbType.String);
            parameters.Add("@PARAM_DESCRIPCION", EmptyToNull(description), DbType.String);

            var results = await connection.QueryAsync<ParameterDbModel>(
                "dbo.UspListaParametros",
                parameters,
                commandType: CommandType.StoredProcedure);

            var parametersList = results.Select(MapToDomain).ToList();
            var normalizedClassDescription = EmptyToNull(classDescription);

            if (normalizedClassDescription is not null)
            {
                parametersList = parametersList
                    .Where(parameter => parameter.ClassDescription.Contains(normalizedClassDescription, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return parametersList;
        }

        public async Task<IEnumerable<DomainParameter>> ListByClassCodeAsync(string classCode, string? status)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CLA_CODIGO", classCode, DbType.String);
            parameters.Add("@PARAM_ESTADO", EmptyToNull(status), DbType.String);

            var results = await connection.QueryAsync<DomainParameter>(
                "dbo.UspListaParametrosPorClase",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.ToList();
        }

        public async Task<DomainParameter?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PARAM_ID", id, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ParameterDbModel>(
                "dbo.UspConsultaParametro",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapToDomain(result);
        }

        public async Task<DomainParameter?> GetByCodeAsync(string parameterCode)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PARAM_CODIGO", parameterCode, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<ParameterDbModel>(
                "dbo.UspConsultaParametro",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapToDomain(result);
        }

        public async Task<int> CreateAsync(DomainParameter parameter)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PARAM_CLASE_ID", parameter.ClassId, DbType.Int32);
            parameters.Add("@PARAM_DESCRIPCION", parameter.Description, DbType.String);
            parameters.Add("@PARAM_ESTADO", parameter.Status, DbType.String);

            return await ExecuteWriteAsync(connection, "dbo.UspCreaParametro", parameters);
        }

        public async Task<int> UpdateAsync(DomainParameter parameter)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PARAM_ID", parameter.Id, DbType.Int32);
            parameters.Add("@PARAM_CLASE_ID", parameter.ClassId > 0 ? parameter.ClassId : (int?)null, DbType.Int32);
            parameters.Add("@PARAM_DESCRIPCION", EmptyToNull(parameter.Description), DbType.String);
            parameters.Add("@PARAM_ESTADO", EmptyToNull(parameter.Status), DbType.String);

            return await ExecuteWriteAsync(connection, "dbo.UspActualizaParametro", parameters);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PARAM_ID", id, DbType.Int32);

            return await ExecuteWriteAsync(connection, "dbo.UspEliminaParametro", parameters);
        }

        private static async Task<int> ExecuteWriteAsync(IDbConnection connection, string procedureName, DynamicParameters parameters)
        {
            var result = await connection.QueryFirstOrDefaultAsync<ParameterWriteResult>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result is null)
                throw new InvalidOperationException("The stored procedure did not return an operation result.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? "The stored procedure reported a failed operation.");

            return result.PARAM_ID ?? 0;
        }

        private static DomainParameter MapToDomain(ParameterDbModel model)
        {
            return new DomainParameter
            {
                Id = model.PARAM_ID,
                ClassId = model.PARAM_CLASE_ID,
                ClassDescription = model.CLASE_DESCRIPCION ?? string.Empty,
                Description = model.PARAM_DESCRIPCION ?? string.Empty,
                Status = model.PARAM_ESTADO ?? string.Empty,
                CreatedAt = model.PARAM_FECHA_CREACION
            };
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        
        private sealed class ParameterDbModel
        {
            public int PARAM_ID { get; set; }
            public int PARAM_CLASE_ID { get; set; }
            public string? CLASE_DESCRIPCION { get; set; }
            public string? PARAM_DESCRIPCION { get; set; }
            public string? PARAM_ESTADO { get; set; }
            public DateTime PARAM_FECHA_CREACION { get; set; }
        }

        private sealed class ParameterWriteResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? PARAM_ID { get; set; }
        }
    }
}
