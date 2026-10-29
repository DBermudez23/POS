using Dapper;
using FluentAssertions;
using UtpPos.Database.IntegrationTests.Infrastructure;
using Xunit;

namespace UtpPos.Database.IntegrationTests.Smoke;

[Collection(DatabaseIntegrationCollection.Name)]
public sealed class DatabaseSmokeTests
{
    private readonly DatabaseIntegrationFixture _fixture;

    public DatabaseSmokeTests(DatabaseIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [SkippableFact]
    public async Task DatabaseConnection_WhenConfigured_ShouldOpenSuccessfully()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();

        var result = await connection.QuerySingleAsync<int>("SELECT 1");

        result.Should().Be(1);
    }

    [SkippableFact]
    public async Task DatabaseName_ShouldLookLikeTestDatabase()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();

        var databaseName = await connection.QuerySingleAsync<string>("SELECT DB_NAME()");

        DatabaseTestGuard.IsForbiddenDatabaseName(databaseName).Should().BeFalse();
        DatabaseTestGuard.LooksLikeTestDatabase(databaseName).Should().BeTrue(
            "DB integration tests must run only against an explicit test database");
    }

    [SkippableFact]
    public async Task CriticalTables_ShouldExist()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();

        var missing = new List<string>();
        foreach (var tableName in TestDatabaseMetadata.CriticalTables)
        {
            var exists = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(1) FROM sys.tables WHERE name = @TableName",
                new { TableName = tableName });

            if (exists == 0)
                missing.Add(tableName);
        }

        missing.Should().BeEmpty($"the test database must contain the critical POS tables. Missing: {string.Join(", ", missing)}");
    }

    [SkippableFact]
    public async Task CriticalStoredProcedures_ShouldExist()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();

        var missing = new List<string>();
        foreach (var procedureName in TestDatabaseMetadata.CriticalStoredProcedures)
        {
            var exists = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(1) FROM sys.objects WHERE type = 'P' AND name = @ProcedureName",
                new { ProcedureName = procedureName });

            if (exists == 0)
                missing.Add(procedureName);
        }

        missing.Should().BeEmpty($"the test database must contain the critical POS stored procedures. Missing: {string.Join(", ", missing)}");
    }

    [SkippableFact]
    public async Task CriticalTableTypes_ShouldExist()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();

        var missing = new List<string>();
        foreach (var typeName in TestDatabaseMetadata.CriticalTableTypes)
        {
            var exists = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(1) FROM sys.types WHERE is_table_type = 1 AND name = @TypeName",
                new { TypeName = typeName });

            if (exists == 0)
                missing.Add(typeName);
        }

        missing.Should().BeEmpty($"the test database must contain the critical table-valued parameter types. Missing: {string.Join(", ", missing)}");
    }
}
