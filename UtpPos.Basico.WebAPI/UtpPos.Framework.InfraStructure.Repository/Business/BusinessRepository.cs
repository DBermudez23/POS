using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.Business;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Interface;
using DomainBusiness = UtpPos.Framework.Domain.Entity.Business.Business;

namespace UtpPos.Framework.InfraStructure.Repository.Business
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public BusinessRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainBusiness>> ListAsync(string? nit, string? name, string? status)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@NEG_NIT", EmptyToNull(nit), DbType.String);
            parameters.Add("@NEG_NOMBRE", EmptyToNull(name), DbType.String);
            parameters.Add("@NEG_ESTADO", EmptyToNull(status), DbType.String);

            var results = await connection.QueryAsync<BusinessDbModel>(
                "dbo.UspListaNegocios",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapToDomain);
        }

        public async Task<DomainBusiness?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@NEG_ID", id, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<BusinessDbModel>(
                "dbo.UspConsultaNegocio",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapToDomain(result);
        }

        public async Task<int> CreateAsync(DomainBusiness business)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@NEG_NIT", business.Nit, DbType.String);
            parameters.Add("@NEG_NOMBRE", business.Name, DbType.String);
            parameters.Add("@NEG_DIRECCION", business.Address, DbType.String);
            parameters.Add("@NEG_TELEFONO", EmptyToNull(business.Phone), DbType.String);
            parameters.Add("@NEG_ESTADO", business.Status, DbType.String);

            return await ExecuteWriteAsync(connection, "dbo.UspCreaNegocio", parameters);
        }

        public async Task<int> UpdateAsync(DomainBusiness business, bool clearPhone)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@NEG_ID", business.Id, DbType.Int32);
            parameters.Add("@NEG_NIT", EmptyToNull(business.Nit), DbType.String);
            parameters.Add("@NEG_NOMBRE", EmptyToNull(business.Name), DbType.String);
            parameters.Add("@NEG_DIRECCION", EmptyToNull(business.Address), DbType.String);
            parameters.Add("@NEG_TELEFONO", EmptyToNull(business.Phone), DbType.String);
            parameters.Add("@NEG_ESTADO", EmptyToNull(business.Status), DbType.String);
            parameters.Add("@LIMPIAR_TELEFONO", clearPhone, DbType.Boolean);

            return await ExecuteWriteAsync(connection, "dbo.UspActualizaNegocio", parameters);
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@NEG_ID", id, DbType.Int32);

            return await ExecuteWriteAsync(connection, "dbo.UspEliminaNegocio", parameters);
        }

        private static async Task<int> ExecuteWriteAsync(IDbConnection connection, string procedureName, DynamicParameters parameters)
        {
            var result = await connection.QueryFirstOrDefaultAsync<BusinessWriteResult>(
                procedureName,
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result is null)
                throw new InvalidOperationException("The stored procedure did not return an operation result.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? "The stored procedure reported a failed operation.");

            return result.NEG_ID;
        }

        private static DomainBusiness MapToDomain(BusinessDbModel model)
        {
            return new DomainBusiness
            {
                Id = model.NEG_ID,
                Nit = model.NEG_NIT ?? string.Empty,
                Name = model.NEG_NOMBRE ?? string.Empty,
                Address = model.NEG_DIRECCION ?? string.Empty,
                Phone = model.NEG_TELEFONO,
                Status = model.NEG_ESTADO ?? string.Empty
            };
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class BusinessDbModel
        {
            public int NEG_ID { get; set; }
            public string? NEG_NIT { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public string? NEG_DIRECCION { get; set; }
            public string? NEG_TELEFONO { get; set; }
            public string? NEG_ESTADO { get; set; }
        }

        private sealed class BusinessWriteResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int NEG_ID { get; set; }
        }
    }
}
