using Microsoft.Data.SqlClient;
using Xunit;

namespace UtpPos.Database.IntegrationTests.Infrastructure;

public sealed class DatabaseIntegrationFixture
{
    public const string ConnectionStringEnvironmentVariable = "UTPPOS_TEST_SQL_CONNECTION_STRING";

    private readonly string? _connectionString;
    private readonly Exception? _configurationError;

    public DatabaseIntegrationFixture()
    {
        RunId = TestDataRunId.Create();

        var configuredConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            MissingConfigurationMessage = $"{ConnectionStringEnvironmentVariable} no esta configurada. Los DB integration tests requieren una BD de pruebas explicita.";
            return;
        }

        try
        {
            var databaseName = DatabaseTestGuard.Validate(configuredConnectionString);
            _connectionString = configuredConnectionString;
            DatabaseName = databaseName;
        }
        catch (Exception exception)
        {
            _configurationError = exception;
        }
    }

    public string RunId { get; }

    public string? DatabaseName { get; }

    public string? MissingConfigurationMessage { get; }

    public bool HasConfiguredConnection => MissingConfigurationMessage is null && _configurationError is null;

    public void SkipIfNotConfigured()
    {
        Skip.If(MissingConfigurationMessage is not null, MissingConfigurationMessage);

        if (_configurationError is not null)
            throw new InvalidOperationException(_configurationError.Message, _configurationError);
    }

    public SqlConnection CreateConnection()
    {
        EnsureReady();
        return new SqlConnection(_connectionString);
    }

    public TestConnectionFactory CreateConnectionFactory()
    {
        EnsureReady();
        return new TestConnectionFactory(_connectionString!);
    }

    private void EnsureReady()
    {
        if (MissingConfigurationMessage is not null)
            throw new InvalidOperationException(MissingConfigurationMessage);

        if (_configurationError is not null)
            throw new InvalidOperationException(_configurationError.Message, _configurationError);
    }
}
