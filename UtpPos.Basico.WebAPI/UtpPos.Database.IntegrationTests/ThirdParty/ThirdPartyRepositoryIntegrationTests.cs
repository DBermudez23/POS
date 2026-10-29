using Dapper;
using FluentAssertions;
using UtpPos.Database.IntegrationTests.Infrastructure;
using UtpPos.Framework.Domain.Entity.ThirdParty;
using UtpPos.Framework.InfraStructure.Repository.ThirdParty;
using Xunit;
using Xunit.Abstractions;

namespace UtpPos.Database.IntegrationTests.ThirdParty;

[Collection(DatabaseIntegrationCollection.Name)]
public sealed class ThirdPartyRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseIntegrationFixture _fixture;
    private readonly ITestOutputHelper _output;
    private readonly TestDataCleaner _cleaner;

    public ThirdPartyRepositoryIntegrationTests(DatabaseIntegrationFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
        _cleaner = new TestDataCleaner(fixture);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _cleaner.CleanThirdPartyDataAsync();

    [SkippableFact]
    public async Task ThirdPartyColumns_ShouldUseExpectedDocumentColumn()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();
        await ThirdPartySchemaGuard.SkipIfRequiredSchemaIsMissingAsync(connection);

        var hasExpectedColumn = await ColumnExistsAsync(connection, "TBL_TERCEROS", "TER_NUMERO_DOCUMENTO");
        var hasLegacyDocumentColumn = await ColumnExistsAsync(connection, "TBL_TERCEROS", "TER_NUM_DOCUMENTO");
        var hasLegacyBusinessColumn = await ColumnExistsAsync(connection, "TBL_TERCEROS", "TER_NEG_ID");

        if (hasLegacyBusinessColumn)
            _output.WriteLine("Riesgo: TBL_TERCEROS aun contiene TER_NEG_ID fisicamente. Los tests no lo usan.");

        hasExpectedColumn.Should().BeTrue();
        hasLegacyDocumentColumn.Should().BeFalse();
    }

    [SkippableFact]
    public async Task CreateAsync_WhenValidThirdParty_ShouldCreateGlobalThirdParty()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();
        await ThirdPartySchemaGuard.SkipIfRequiredSchemaIsMissingAsync(connection);
        var seed = await ThirdPartySeedResolver.ResolveRequiredAsync(connection);
        var repository = CreateRepository();
        var thirdParty = CreateThirdParty(seed, UniqueSuffix());

        var result = await repository.CreateAsync(thirdParty);

        result.ThirdPartyId.Should().BeGreaterThan(0);
        var stored = await connection.QuerySingleOrDefaultAsync<StoredThirdParty>(
            @"SELECT TER_ID, TER_NUMERO_DOCUMENTO, TER_NOMBRE, TER_ESTADO
              FROM dbo.TBL_TERCEROS
              WHERE TER_ID = @ThirdPartyId;",
            new { result.ThirdPartyId });

        stored.Should().NotBeNull();
        stored!.TER_NUMERO_DOCUMENTO.Should().Be(thirdParty.DocumentNumber);
        stored.TER_ESTADO.Should().Be("A");

        var hasLegacyBusinessColumn = await ColumnExistsAsync(connection, "TBL_TERCEROS", "TER_NEG_ID");
        if (hasLegacyBusinessColumn)
            _output.WriteLine("Riesgo: TBL_TERCEROS aun contiene TER_NEG_ID fisicamente. CreateAsync no envia TER_NEG_ID.");
    }

    [SkippableFact]
    public async Task AssociateBusinessAsync_WhenThirdPartyAndBusinessExist_ShouldCreateActiveRelation()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();
        await ThirdPartySchemaGuard.SkipIfRequiredSchemaIsMissingAsync(connection);
        var seed = await ThirdPartySeedResolver.ResolveRequiredAsync(connection);
        var repository = CreateRepository();
        var thirdPartyId = await CreateThirdPartyAsync(repository, seed, UniqueSuffix());

        var result = await repository.AssociateBusinessAsync(new ThirdPartyBusinessAssociation
        {
            BusinessId = seed.BusinessId,
            ThirdPartyId = thirdPartyId,
            UserId = seed.UserId
        });

        result.ThirdPartyId.Should().Be(thirdPartyId);
        var relation = await QueryRelationAsync(connection, seed.BusinessId, thirdPartyId);
        relation.Should().NotBeNull();
        relation!.NEGTER_NEG_ID.Should().Be(seed.BusinessId);
        relation.NEGTER_TER_ID.Should().Be(thirdPartyId);
        relation.NEGTER_ESTADO.Should().Be("A");
    }

    [SkippableFact]
    public async Task ListByBusinessAsync_WhenRelationExists_ShouldReturnThirdParty()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();
        await ThirdPartySchemaGuard.SkipIfRequiredSchemaIsMissingAsync(connection);
        var seed = await ThirdPartySeedResolver.ResolveRequiredAsync(connection);
        var repository = CreateRepository();
        var suffix = UniqueSuffix();
        var thirdParty = CreateThirdParty(seed, suffix);
        var created = await repository.CreateAsync(thirdParty);
        await repository.AssociateBusinessAsync(new ThirdPartyBusinessAssociation
        {
            BusinessId = seed.BusinessId,
            ThirdPartyId = created.ThirdPartyId,
            UserId = seed.UserId
        });

        var results = await repository.ListByBusinessAsync(new ThirdPartyBusinessFilter
        {
            BusinessId = seed.BusinessId,
            DocumentNumber = thirdParty.DocumentNumber,
            Name = thirdParty.Name,
            Status = "A"
        });

        results.Should().ContainSingle(thirdPartyResult =>
            thirdPartyResult.Id == created.ThirdPartyId &&
            thirdPartyResult.DocumentNumber == thirdParty.DocumentNumber &&
            thirdPartyResult.RelationStatus == "A");
    }

    [SkippableFact]
    public async Task ValidateBusinessAsync_WhenRelationIsActive_ShouldReturnTrue()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();
        await ThirdPartySchemaGuard.SkipIfRequiredSchemaIsMissingAsync(connection);
        var seed = await ThirdPartySeedResolver.ResolveRequiredAsync(connection);
        var repository = CreateRepository();
        var thirdPartyId = await CreateThirdPartyAsync(repository, seed, UniqueSuffix());
        await repository.AssociateBusinessAsync(new ThirdPartyBusinessAssociation
        {
            BusinessId = seed.BusinessId,
            ThirdPartyId = thirdPartyId,
            UserId = seed.UserId
        });

        var exists = await repository.ValidateBusinessAsync(seed.BusinessId, thirdPartyId);

        exists.Should().BeTrue();
    }

    [SkippableFact]
    public async Task ValidateBusinessAsync_WhenRelationDoesNotExist_ShouldReturnFalse()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();
        await ThirdPartySchemaGuard.SkipIfRequiredSchemaIsMissingAsync(connection);
        var seed = await ThirdPartySeedResolver.ResolveRequiredAsync(connection);
        var repository = CreateRepository();
        var thirdPartyId = await CreateThirdPartyAsync(repository, seed, UniqueSuffix());

        var exists = await repository.ValidateBusinessAsync(seed.BusinessId, thirdPartyId);

        exists.Should().BeFalse();
    }

    [SkippableFact]
    public async Task InactivateBusinessAsync_WhenRelationExists_ShouldInactivateOnlyRelation()
    {
        _fixture.SkipIfNotConfigured();

        await using var connection = _fixture.CreateConnection();
        await connection.OpenAsync();
        await ThirdPartySchemaGuard.SkipIfRequiredSchemaIsMissingAsync(connection);
        var seed = await ThirdPartySeedResolver.ResolveRequiredAsync(connection);
        var repository = CreateRepository();
        var thirdPartyId = await CreateThirdPartyAsync(repository, seed, UniqueSuffix());
        await repository.AssociateBusinessAsync(new ThirdPartyBusinessAssociation
        {
            BusinessId = seed.BusinessId,
            ThirdPartyId = thirdPartyId,
            UserId = seed.UserId
        });

        await repository.InactivateBusinessAsync(new ThirdPartyBusinessAssociation
        {
            BusinessId = seed.BusinessId,
            ThirdPartyId = thirdPartyId,
            UserId = seed.UserId
        });

        var relation = await QueryRelationAsync(connection, seed.BusinessId, thirdPartyId);
        var thirdPartyStatus = await connection.QuerySingleAsync<string>(
            "SELECT TER_ESTADO FROM dbo.TBL_TERCEROS WHERE TER_ID = @ThirdPartyId;",
            new { ThirdPartyId = thirdPartyId });
        var exists = await repository.ValidateBusinessAsync(seed.BusinessId, thirdPartyId);

        relation.Should().NotBeNull();
        relation!.NEGTER_ESTADO.Should().Be("I");
        thirdPartyStatus.Should().Be("A");
        exists.Should().BeFalse();
    }

    private ThirdPartyRepository CreateRepository()
    {
        return new ThirdPartyRepository(_fixture.CreateConnectionFactory());
    }

    private async Task<int> CreateThirdPartyAsync(ThirdPartyRepository repository, ThirdPartySeedData seed, string suffix)
    {
        var result = await repository.CreateAsync(CreateThirdParty(seed, suffix));
        result.ThirdPartyId.Should().BeGreaterThan(0);
        return result.ThirdPartyId;
    }

    private ThirdPartyCreate CreateThirdParty(ThirdPartySeedData seed, string suffix)
    {
        return new ThirdPartyCreate
        {
            DocumentTypeId = seed.DocumentTypeId,
            DocumentNumber = $"999{suffix}"[..Math.Min(20, $"999{suffix}".Length)],
            Name = $"TEST_TER_{suffix}"[..Math.Min(60, $"TEST_TER_{suffix}".Length)],
            DepartmentId = seed.DepartmentId,
            CityId = seed.CityId,
            Address = $"TEST_ADDR_{suffix}"[..Math.Min(100, $"TEST_ADDR_{suffix}".Length)],
            Phone = "3000000000",
            CreatedByUserId = seed.UserId,
            Status = "A",
            ThirdPartyTypeId = seed.ThirdPartyTypeId
        };
    }

    private string UniqueSuffix()
    {
        return $"{_fixture.RunId}_{Guid.NewGuid():N}"[..32].ToUpperInvariant();
    }

    private static async Task<bool> ColumnExistsAsync(Microsoft.Data.SqlClient.SqlConnection connection, string tableName, string columnName)
    {
        var count = await connection.QuerySingleAsync<int>(
            @"SELECT COUNT(1)
              FROM sys.columns C
              INNER JOIN sys.tables T ON T.object_id = C.object_id
              WHERE T.name = @TableName
                AND C.name = @ColumnName;",
            new { TableName = tableName, ColumnName = columnName });

        return count > 0;
    }

    private static async Task<StoredRelation?> QueryRelationAsync(Microsoft.Data.SqlClient.SqlConnection connection, int businessId, int thirdPartyId)
    {
        return await connection.QuerySingleOrDefaultAsync<StoredRelation>(
            @"SELECT NEGTER_ID, NEGTER_NEG_ID, NEGTER_TER_ID, NEGTER_ESTADO
              FROM dbo.TBL_NEGOCIO_TERCEROS
              WHERE NEGTER_NEG_ID = @BusinessId
                AND NEGTER_TER_ID = @ThirdPartyId;",
            new { BusinessId = businessId, ThirdPartyId = thirdPartyId });
    }

    private sealed class StoredThirdParty
    {
        public int TER_ID { get; set; }
        public string TER_NUMERO_DOCUMENTO { get; set; } = string.Empty;
        public string TER_NOMBRE { get; set; } = string.Empty;
        public string TER_ESTADO { get; set; } = string.Empty;
    }

    private sealed class StoredRelation
    {
        public int NEGTER_ID { get; set; }
        public int NEGTER_NEG_ID { get; set; }
        public int NEGTER_TER_ID { get; set; }
        public string NEGTER_ESTADO { get; set; } = string.Empty;
    }
}
