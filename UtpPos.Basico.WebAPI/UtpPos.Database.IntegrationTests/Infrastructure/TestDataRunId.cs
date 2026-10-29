namespace UtpPos.Database.IntegrationTests.Infrastructure;

public static class TestDataRunId
{
    public static string Create()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"TEST_{timestamp}_{suffix}";
    }
}
