using Dapper;
using Microsoft.Data.SqlClient;
using Xunit;

namespace UtpPos.Database.IntegrationTests.ThirdParty;

public static class ThirdPartySeedResolver
{
    public static async Task<ThirdPartySeedData> ResolveRequiredAsync(SqlConnection connection)
    {
        var businessId = await QueryRequiredIdAsync(
            connection,
            "SELECT TOP (1) NEG_ID FROM dbo.TBL_NEGOCIO WHERE NEG_ESTADO = 'A' ORDER BY NEG_ID",
            "No se encontro negocio activo necesario para crear terceros.");

        var userId = await QueryRequiredIdAsync(
            connection,
            "SELECT TOP (1) USR_ID FROM dbo.TBL_USUARIOS WHERE USR_ESTADO = 'A' ORDER BY USR_ID",
            "No se encontro usuario activo necesario para crear terceros.");

        var documentTypeId = await QueryRequiredParameterAsync(
            connection,
            "%DOCUMENTO%",
            "No se encontro tipo de documento activo necesario para crear terceros.");

        var departmentId = await QueryRequiredParameterAsync(
            connection,
            "%DEPARTAMENTO%",
            "No se encontro departamento activo necesario para crear terceros.");

        var cityId = await QueryRequiredParameterAsync(
            connection,
            "%CIUDAD%",
            "No se encontro ciudad activa necesaria para crear terceros.");

        var thirdPartyTypeId = await QueryRequiredParameterAsync(
            connection,
            "%TERCERO%",
            "No se encontro tipo de tercero activo necesario para crear terceros.");

        return new ThirdPartySeedData(businessId, userId, documentTypeId, departmentId, cityId, thirdPartyTypeId);
    }

    private static async Task<int> QueryRequiredIdAsync(SqlConnection connection, string sql, string skipMessage)
    {
        var id = await connection.QueryFirstOrDefaultAsync<int?>(sql);
        Skip.If(!id.HasValue || id.Value <= 0, skipMessage);
        return id!.Value;
    }

    private static async Task<int> QueryRequiredParameterAsync(SqlConnection connection, string classDescriptionPattern, string skipMessage)
    {
        var id = await connection.QueryFirstOrDefaultAsync<int?>(
            @"SELECT TOP (1) P.PARAM_ID
              FROM dbo.TBL_PARAMETROS P
              INNER JOIN dbo.TBL_CLASE C ON C.CLA_ID = P.PARAM_CLASE_ID
              WHERE P.PARAM_ESTADO = 'A'
                AND C.CLA_ESTADO = 'A'
                AND UPPER(C.CLA_DESCRIPCION) LIKE @ClassDescriptionPattern
              ORDER BY P.PARAM_ID;",
            new { ClassDescriptionPattern = classDescriptionPattern });

        Skip.If(!id.HasValue || id.Value <= 0, skipMessage);
        return id!.Value;
    }
}
