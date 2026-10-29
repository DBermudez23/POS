using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.Class;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Interface;
using DomainClass = UtpPos.Framework.Domain.Entity.Class.Class;

namespace UtpPos.Framework.InfraStructure.Repository.Class
{
    public class ClassRepository : IClassRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public ClassRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainClass>> ListAsync(string? description, string? status, string? isEditable)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CLA_ESTADO", EmptyToNull(status), DbType.String);
            parameters.Add("@CLA_ES_EDITABLE", EmptyToNull(isEditable), DbType.String);
            parameters.Add("@CLA_DESCRIPCION", EmptyToNull(description), DbType.String);

            var results = await connection.QueryAsync<ClassDbModel>(
                "dbo.UspListaClases",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapToDomain).ToList();
        }

        public async Task<DomainClass?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CLA_ID", id, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ClassDbModel>(
                "dbo.UspConsultaClase",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapToDomain(result);
        }

        public async Task<int> CreateAsync(DomainClass classEntity)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CLA_DESCRIPCION", classEntity.Description, DbType.String);
            parameters.Add("@CLA_ES_EDITABLE", classEntity.IsEditable, DbType.String);
            parameters.Add("@CLA_ESTADO", classEntity.Status, DbType.String);

            return await ExecuteWriteAsync(connection, "dbo.UspCreaClase", parameters);
        }

        public async Task<int> UpdateAsync(DomainClass classEntity)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CLA_ID", classEntity.Id, DbType.Int32);
            parameters.Add("@CLA_DESCRIPCION", EmptyToNull(classEntity.Description), DbType.String);
            parameters.Add("@CLA_ES_EDITABLE", EmptyToNull(classEntity.IsEditable), DbType.String);
            parameters.Add("@CLA_ESTADO", EmptyToNull(classEntity.Status), DbType.String);

            return await ExecuteWriteAsync(connection, "dbo.UspActualizaClase", parameters);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CLA_ID", id, DbType.Int32);

            return await ExecuteWriteAsync(connection, "dbo.UspEliminaClase", parameters);
        }

        private static async Task<int> ExecuteWriteAsync(IDbConnection connection, string procedureName, DynamicParameters parameters)
        {
            var result = await connection.QueryFirstOrDefaultAsync<ClassWriteResult>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result is null)
                throw new InvalidOperationException("The stored procedure did not return an operation result.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? "The stored procedure reported a failed operation.");

            return result.CLA_ID;
        }

        private static DomainClass MapToDomain(ClassDbModel model)
        {
            return new DomainClass
            {
                Id = model.CLA_ID,
                Description = model.CLA_DESCRIPCION ?? string.Empty,
                IsEditable = model.CLA_ES_EDITABLE ?? string.Empty,
                Status = model.CLA_ESTADO ?? string.Empty,
                CreatedAt = model.CLA_FECHA_CREACION,
                TotalParameters = model.TOTAL_PARAMETROS
            };
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class ClassDbModel
        {
            public int CLA_ID { get; set; }
            public string? CLA_DESCRIPCION { get; set; }
            public string? CLA_ES_EDITABLE { get; set; }
            public string? CLA_ESTADO { get; set; }
            public DateTime CLA_FECHA_CREACION { get; set; }
            public int TOTAL_PARAMETROS { get; set; }
        }

        private sealed class ClassWriteResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int CLA_ID { get; set; }
        }
    }
}
