using System.Data;
using UtpPos.Framework.Transversal.Interface;
using Microsoft.Data.SqlClient;

namespace UtpPos.Database.IntegrationTests.Infrastructure;

public sealed class TestConnectionFactory : IConnectionFactory
{
    private readonly string _connectionString;

    public TestConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string GetCadenaConexion => _connectionString;

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}
