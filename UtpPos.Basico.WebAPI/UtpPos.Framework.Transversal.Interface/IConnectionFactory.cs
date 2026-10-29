using System.Data;
using Microsoft.Data.SqlClient;

namespace UtpPos.Framework.Transversal.Interface
{
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<SqlConnection> GetConnection();
        string GetCadenaConexion { get; }
    }
}
