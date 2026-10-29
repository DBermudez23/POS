using FluentAssertions;
using UtpPos.Framework.Domain.Core.ThirdParty;
using UtpPos.Framework.InfraStructure.Interface.ThirdParty;
using NSubstitute;
using DomainThirdParty = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdParty;
using DomainThirdPartyBusiness = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusiness;
using DomainThirdPartyBusinessAssociation = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessAssociation;
using DomainThirdPartyBusinessFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessFilter;
using DomainThirdPartyBusinessResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessResult;
using DomainThirdPartyCreate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreate;
using DomainThirdPartyCreateForBusiness = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreateForBusiness;
using DomainThirdPartyFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyFilter;
using DomainThirdPartyResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyResult;
using DomainThirdPartyUpdate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyUpdate;

namespace UtpPos.Backend.Tests.ThirdParty;

public sealed class ThirdPartyDomainTests
{
    private readonly IThirdPartyRepository _repository = Substitute.For<IThirdPartyRepository>();

    [Fact]
    public async Task CreateAsync_WhenThirdPartyIsValid_ShouldCreateGlobalThirdPartyWithoutBusinessId()
    {
        // Arrange
        var request = CreateThirdParty();
        _repository.CreateAsync(Arg.Any<DomainThirdPartyCreate>()).Returns(new DomainThirdPartyResult { ThirdPartyId = 10 });
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(request);

        // Assert
        result.ThirdPartyId.Should().Be(10);
        await _repository.Received(1).CreateAsync(Arg.Is<DomainThirdPartyCreate>(thirdParty =>
            thirdParty.DocumentNumber == "123" &&
            thirdParty.Name == "Proveedor Uno" &&
            thirdParty.Status == "A"));
    }

    [Theory]
    [InlineData("", "DocumentNumber is required.")]
    [InlineData("   ", "DocumentNumber is required.")]
    public async Task CreateAsync_WhenDocumentNumberIsEmpty_ShouldThrowControlledValidation(string documentNumber, string expectedMessage)
    {
        // Arrange
        var request = CreateThirdParty(documentNumber: documentNumber);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage(expectedMessage);
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainThirdPartyCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsEmpty_ShouldThrowControlledValidation()
    {
        // Arrange
        var request = CreateThirdParty(name: " ");
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Name is required.");
    }

    [Fact]
    public async Task CreateAsync_WhenDocumentTypeIdIsInvalid_ShouldThrowControlledValidation()
    {
        // Arrange
        var request = CreateThirdParty(documentTypeId: 0);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("DocumentTypeId must be greater than zero.");
    }

    [Fact]
    public async Task UpdateAsync_WhenThirdPartyIsValid_ShouldUpdateGlobalThirdPartyWithoutBusinessId()
    {
        // Arrange
        var request = new DomainThirdPartyUpdate { Id = 10, Name = " Nuevo Nombre ", Status = "a" };
        _repository.UpdateAsync(Arg.Any<DomainThirdPartyUpdate>()).Returns(new DomainThirdPartyResult { ThirdPartyId = 10 });
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAsync(request);

        // Assert
        result.ThirdPartyId.Should().Be(10);
        await _repository.Received(1).UpdateAsync(Arg.Is<DomainThirdPartyUpdate>(thirdParty =>
            thirdParty.Id == 10 &&
            thirdParty.Name == "Nuevo Nombre" &&
            thirdParty.Status == "A"));
    }

    [Fact]
    public async Task ListAsync_WhenGlobalFiltersAreProvided_ShouldDelegateWithoutBusinessId()
    {
        // Arrange
        _repository.ListAsync(Arg.Any<DomainThirdPartyFilter>()).Returns(new[] { ExistingThirdParty(10) });
        var filter = new DomainThirdPartyFilter { Status = "a", DocumentNumber = " 123 ", Name = " Proveedor ", Phone = " 555 " };
        var sut = CreateSut();

        // Act
        var result = await sut.ListAsync(filter);

        // Assert
        result.Should().ContainSingle(thirdParty => thirdParty.Id == 10);
        await _repository.Received(1).ListAsync(Arg.Is<DomainThirdPartyFilter>(domainFilter =>
            domainFilter.Status == "A" &&
            domainFilter.DocumentNumber == "123" &&
            domainFilter.Name == "Proveedor" &&
            domainFilter.Phone == "555"));
    }

    [Fact]
    public async Task CreateForBusinessAsync_WhenThirdPartyDoesNotExist_ShouldCreateGlobalAndAssociateBusiness()
    {
        // Arrange
        _repository.ListAsync(Arg.Any<DomainThirdPartyFilter>()).Returns(Array.Empty<DomainThirdParty>());
        _repository.CreateAsync(Arg.Any<DomainThirdPartyCreate>()).Returns(new DomainThirdPartyResult { ThirdPartyId = 33 });
        _repository.AssociateBusinessAsync(Arg.Any<DomainThirdPartyBusinessAssociation>()).Returns(new DomainThirdPartyBusinessResult { ThirdPartyId = 33, NegterId = 44 });
        var sut = CreateSut();

        // Act
        var result = await sut.CreateForBusinessAsync(CreateThirdPartyForBusiness());

        // Assert
        result.ThirdPartyId.Should().Be(33);
        result.NegterId.Should().Be(44);
        await _repository.Received(1).ListAsync(Arg.Is<DomainThirdPartyFilter>(filter =>
            filter.DocumentTypeId == 1 && filter.DocumentNumber == "123"));
        await _repository.Received(1).CreateAsync(Arg.Any<DomainThirdPartyCreate>());
        await _repository.Received(1).AssociateBusinessAsync(Arg.Is<DomainThirdPartyBusinessAssociation>(association =>
            association.BusinessId == 20 && association.ThirdPartyId == 33 && association.UserId == 7));
    }

    [Fact]
    public async Task CreateForBusinessAsync_WhenThirdPartyExists_ShouldNotDuplicateAndShouldAssociateBusiness()
    {
        // Arrange
        _repository.ListAsync(Arg.Any<DomainThirdPartyFilter>()).Returns(new[] { ExistingThirdParty(88) });
        _repository.AssociateBusinessAsync(Arg.Any<DomainThirdPartyBusinessAssociation>()).Returns(new DomainThirdPartyBusinessResult { ThirdPartyId = 88, NegterId = 99 });
        var sut = CreateSut();

        // Act
        var result = await sut.CreateForBusinessAsync(CreateThirdPartyForBusiness());

        // Assert
        result.ThirdPartyId.Should().Be(88);
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainThirdPartyCreate>());
        await _repository.Received(1).AssociateBusinessAsync(Arg.Is<DomainThirdPartyBusinessAssociation>(association =>
            association.BusinessId == 20 && association.ThirdPartyId == 88));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateForBusinessAsync_WhenBusinessIdIsInvalid_ShouldThrowControlledValidation(int businessId)
    {
        // Arrange
        var request = CreateThirdPartyForBusiness(businessId: businessId);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateForBusinessAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("BusinessId must be greater than zero.");
        await _repository.DidNotReceive().ListAsync(Arg.Any<DomainThirdPartyFilter>());
    }

    [Fact]
    public async Task CreateForBusinessAsync_WhenAssociationFails_ShouldPropagateControlledError()
    {
        // Arrange
        _repository.ListAsync(Arg.Any<DomainThirdPartyFilter>()).Returns(new[] { ExistingThirdParty(88) });
        _repository.AssociateBusinessAsync(Arg.Any<DomainThirdPartyBusinessAssociation>()).Returns<Task<DomainThirdPartyBusinessResult>>(_ => throw new InvalidOperationException("association failed"));
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateForBusinessAsync(CreateThirdPartyForBusiness());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("association failed");
    }

    [Fact]
    public async Task ListByBusinessAsync_WhenBusinessIdIsValid_ShouldReturnAssociatedThirdParties()
    {
        // Arrange
        _repository.ListByBusinessAsync(Arg.Any<DomainThirdPartyBusinessFilter>()).Returns(new[] { new DomainThirdPartyBusiness { Id = 10, BusinessId = 20, RelationStatus = "A" } });
        var sut = CreateSut();

        // Act
        var result = await sut.ListByBusinessAsync(new DomainThirdPartyBusinessFilter { BusinessId = 20, Status = "a" });

        // Assert
        result.Should().ContainSingle(thirdParty => thirdParty.Id == 10 && thirdParty.BusinessId == 20);
        await _repository.Received(1).ListByBusinessAsync(Arg.Is<DomainThirdPartyBusinessFilter>(filter => filter.BusinessId == 20 && filter.Status == "A"));
    }

    [Fact]
    public async Task ListByBusinessAsync_WhenBusinessIdIsInvalid_ShouldThrowControlledValidation()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.ListByBusinessAsync(new DomainThirdPartyBusinessFilter { BusinessId = 0 });

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("BusinessId must be greater than zero.");
    }

    [Fact]
    public async Task AssociateBusinessAsync_WhenAssociationIsValid_ShouldCallAssociationRepository()
    {
        // Arrange
        _repository.AssociateBusinessAsync(Arg.Any<DomainThirdPartyBusinessAssociation>()).Returns(new DomainThirdPartyBusinessResult { ThirdPartyId = 10, NegterId = 99 });
        var sut = CreateSut();

        // Act
        var result = await sut.AssociateBusinessAsync(new DomainThirdPartyBusinessAssociation { BusinessId = 20, ThirdPartyId = 10, UserId = 7 });

        // Assert
        result.NegterId.Should().Be(99);
        await _repository.Received(1).AssociateBusinessAsync(Arg.Any<DomainThirdPartyBusinessAssociation>());
    }

    [Fact]
    public async Task AssociateBusinessAsync_WhenThirdPartyIdIsInvalid_ShouldThrowControlledValidation()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = () => sut.AssociateBusinessAsync(new DomainThirdPartyBusinessAssociation { BusinessId = 20, ThirdPartyId = 0, UserId = 7 });

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("ThirdPartyId must be greater than zero.");
        await _repository.DidNotReceive().AssociateBusinessAsync(Arg.Any<DomainThirdPartyBusinessAssociation>());
    }

    [Fact]
    public async Task InactivateBusinessAsync_WhenAssociationIsValid_ShouldInactivateRelationOnly()
    {
        // Arrange
        _repository.InactivateBusinessAsync(Arg.Any<DomainThirdPartyBusinessAssociation>()).Returns(new DomainThirdPartyBusinessResult { ThirdPartyId = 10, NegterId = 99 });
        var sut = CreateSut();

        // Act
        var result = await sut.InactivateBusinessAsync(new DomainThirdPartyBusinessAssociation { BusinessId = 20, ThirdPartyId = 10, UserId = 7 });

        // Assert
        result.NegterId.Should().Be(99);
        await _repository.Received(1).InactivateBusinessAsync(Arg.Any<DomainThirdPartyBusinessAssociation>());
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<int>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ValidateBusinessAsync_ShouldReturnRepositoryResult(bool existsActiveRelation)
    {
        // Arrange
        _repository.ValidateBusinessAsync(20, 10).Returns(existsActiveRelation);
        var sut = CreateSut();

        // Act
        var result = await sut.ValidateBusinessAsync(20, 10);

        // Assert
        result.Should().Be(existsActiveRelation);
    }

    private ThirdPartyDomain CreateSut() => new(_repository);

    private static DomainThirdPartyCreate CreateThirdParty(
        int documentTypeId = 1,
        string documentNumber = "123",
        string name = "Proveedor Uno") => new()
    {
        DocumentTypeId = documentTypeId,
        DocumentNumber = documentNumber,
        Name = name,
        DepartmentId = 2,
        CityId = 3,
        Address = " Calle 1 ",
        Phone = "555",
        CreatedByUserId = 7,
        Status = "a",
        ThirdPartyTypeId = 4
    };

    private static DomainThirdPartyCreateForBusiness CreateThirdPartyForBusiness(int businessId = 20) => new()
    {
        BusinessId = businessId,
        DocumentTypeId = 1,
        DocumentNumber = " 123 ",
        Name = "Proveedor Uno",
        DepartmentId = 2,
        CityId = 3,
        Phone = "555",
        CreatedByUserId = 7,
        Status = "A",
        ThirdPartyTypeId = 4
    };

    private static DomainThirdParty ExistingThirdParty(int id) => new()
    {
        Id = id,
        DocumentTypeId = 1,
        DocumentNumber = "123",
        Name = "Proveedor Uno",
        Status = "A"
    };
}
