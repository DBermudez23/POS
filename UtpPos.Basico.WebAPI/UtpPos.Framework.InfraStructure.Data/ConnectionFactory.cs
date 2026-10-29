using System.Data;
using UtpPos.Framework.Transversal.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace UtpPos.Framework.InfraStructure.Data
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public ConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(GetCadenaConexion);
        }

        public async Task<SqlConnection> GetConnection()
        {
            var connection = new SqlConnection(GetCadenaConexion);
            await connection.OpenAsync();
            return connection;
        }

        public string GetCadenaConexion
        {
            get
            {
                return _configuration.GetConnectionString("DefaultConnection")
                    ?? _configuration.GetConnectionString("UtpPosConnection")
                    ?? string.Empty;
            }
        }
    }
}
