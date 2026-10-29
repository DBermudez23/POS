using Dapper;
using Microsoft.Data.SqlClient;
using Xunit;

namespace UtpPos.Database.IntegrationTests.ThirdParty;

public static class ThirdPartySchemaGuard
{
    private static readonly string[] RequiredTables =
    {
        "TBL_TERCEROS",
        "TBL_NEGOCIO_TERCEROS",
        "TBL_NEGOCIO",
        "TBL_USUARIOS",
        "TBL_PARAMETROS",
        "TBL_CLASE"
    };

    private static readonly string[] RequiredProcedures =
    {
        "UspCreaTercero",
        "UspAsociaTerceroNegocio",
        "UspListaTercerosPorNegocio",
        "UspValidaTerceroNegocio",
        "UspInactivaTerceroNegocio"
    };

    public static async Task SkipIfRequiredSchemaIsMissingAsync(SqlConnection connection)
    {
        var missingTables = await MissingTablesAsync(connection, RequiredTables);
        var missingProcedures = await MissingProceduresAsync(connection, RequiredProcedures);

        var missing = missingTables
            .Select(table => $"table:{table}")
            .Concat(missingProcedures.Select(procedure => $"procedure:{procedure}"))
            .ToArray();

        Skip.If(missing.Length > 0, "No se ejecutan tests funcionales de terceros porque faltan objetos de esquema: " + string.Join(", ", missing));
    }

    private static async Task<IReadOnlyCollection<string>> MissingTablesAsync(SqlConnection connection, IEnumerable<string> tableNames)
    {
        var missing = new List<string>();
        foreach (var tableName in tableNames)
        {
            var exists = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(1) FROM sys.tables WHERE name = @TableName;",
                new { TableName = tableName });

            if (exists == 0)
                missing.Add(tableName);
        }

        return missing;
    }

    private static async Task<IReadOnlyCollection<string>> MissingProceduresAsync(SqlConnection connection, IEnumerable<string> procedureNames)
    {
        var missing = new List<string>();
        foreach (var procedureName in procedureNames)
        {
            var exists = await connection.QuerySingleAsync<int>(
                "SELECT COUNT(1) FROM sys.objects WHERE type = 'P' AND name = @ProcedureName;",
                new { ProcedureName = procedureName });

            if (exists == 0)
                missing.Add(procedureName);
        }

        return missing;
    }
}
