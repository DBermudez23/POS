using Microsoft.Data.SqlClient;

namespace UtpPos.Database.IntegrationTests.Infrastructure;

public static class DatabaseTestGuard
{
    private const string ForbiddenDatabaseName = "VENTAS-POS";

    public static string Validate(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("UTPPOS_TEST_SQL_CONNECTION_STRING no esta configurada. Los DB integration tests requieren una BD de pruebas explicita.");

        EnsureRawConnectionStringDoesNotTargetForbiddenDatabase(connectionString);

        SqlConnectionStringBuilder builder;
        try
        {
            builder = new SqlConnectionStringBuilder(connectionString);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("UTPPOS_TEST_SQL_CONNECTION_STRING no es una cadena de conexion valida.", exception);
        }

        var databaseName = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("La cadena de conexion de pruebas debe incluir Database o Initial Catalog.");

        if (IsForbiddenDatabaseName(databaseName))
            throw new InvalidOperationException($"La base de datos configurada no parece ser de pruebas. Nombre detectado: {databaseName}. Abortando DB integration tests.");

        if (!LooksLikeTestDatabase(databaseName))
            throw new InvalidOperationException($"La base de datos configurada no parece ser de pruebas. Nombre detectado: {databaseName}. Abortando DB integration tests.");

        return databaseName;
    }

    public static bool LooksLikeTestDatabase(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
            return false;

        var normalized = databaseName.Trim().ToUpperInvariant();
        return normalized.Contains("TEST", StringComparison.Ordinal);
    }

    public static bool IsForbiddenDatabaseName(string databaseName)
    {
        return string.Equals(databaseName?.Trim(), ForbiddenDatabaseName, StringComparison.OrdinalIgnoreCase);
    }

    private static void EnsureRawConnectionStringDoesNotTargetForbiddenDatabase(string connectionString)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            var separatorIndex = part.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = part[..separatorIndex].Trim();
            var value = part[(separatorIndex + 1)..].Trim();

            if ((string.Equals(key, "Database", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(key, "Initial Catalog", StringComparison.OrdinalIgnoreCase)) &&
                IsForbiddenDatabaseName(value))
            {
                throw new InvalidOperationException($"La base de datos configurada no parece ser de pruebas. Nombre detectado: {value}. Abortando DB integration tests.");
            }
        }
    }
}
