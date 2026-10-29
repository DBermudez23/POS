using Dapper;

namespace UtpPos.Database.IntegrationTests.Infrastructure;

public sealed class TestDataCleaner
{
    private readonly DatabaseIntegrationFixture _fixture;

    public TestDataCleaner(DatabaseIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task CleanThirdPartyDataAsync()
    {
        if (!_fixture.HasConfiguredConnection)
            return;

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();

        if (!await TableExistsAsync(connection, "TBL_TERCEROS") ||
            !await TableExistsAsync(connection, "TBL_NEGOCIO_TERCEROS"))
        {
            return;
        }

        var pattern = $"%{_fixture.RunId}%";

        await connection.ExecuteAsync(
            @"DELETE NT
              FROM dbo.TBL_NEGOCIO_TERCEROS NT
              INNER JOIN dbo.TBL_TERCEROS T ON T.TER_ID = NT.NEGTER_TER_ID
              WHERE T.TER_NUMERO_DOCUMENTO LIKE @Pattern
                 OR T.TER_NOMBRE LIKE @Pattern;",
            new { Pattern = pattern });

        await connection.ExecuteAsync(
            @"DELETE FROM dbo.TBL_TERCEROS
              WHERE TER_NUMERO_DOCUMENTO LIKE @Pattern
                 OR TER_NOMBRE LIKE @Pattern;",
            new { Pattern = pattern });
    }

    private static async Task<bool> TableExistsAsync(System.Data.IDbConnection connection, string tableName)
    {
        var count = await connection.QuerySingleAsync<int>(
            "SELECT COUNT(1) FROM sys.tables WHERE name = @TableName;",
            new { TableName = tableName });

        return count > 0;
    }
}
