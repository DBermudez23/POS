using Xunit;

namespace UtpPos.Database.IntegrationTests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DatabaseIntegrationCollection : ICollectionFixture<DatabaseIntegrationFixture>
{
    public const string Name = "Database integration";
}
