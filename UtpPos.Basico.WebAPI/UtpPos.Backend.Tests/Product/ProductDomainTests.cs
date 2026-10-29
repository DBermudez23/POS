using FluentAssertions;
using UtpPos.Framework.Domain.Core.Product;
using UtpPos.Framework.InfraStructure.Interface.Product;
using NSubstitute;
using DomainProduct = UtpPos.Framework.Domain.Entity.Product.Product;
using DomainProductFilter = UtpPos.Framework.Domain.Entity.Product.ProductFilter;
using DomainProductUpdate = UtpPos.Framework.Domain.Entity.Product.ProductUpdate;

namespace UtpPos.Backend.Tests.Product;

public sealed class ProductDomainTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();

    [Fact]
    public async Task CreateAsync_WhenProductIsValid_ShouldCreateProductWithoutRequiringSaleValue()
    {
        // Arrange
        _repository.CreateAsync(Arg.Any<DomainProduct>()).Returns(10);
        var product = CreateProduct();
        product.SaleValue = null;
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(product);

        // Assert
        result.Should().Be(10);
        await _repository.Received(1).CreateAsync(Arg.Is<DomainProduct>(value =>
            value.BusinessId == 20 &&
            value.SaleValue == null &&
            value.Status == "A"));
    }

    [Fact]
    public async Task CreateAsync_WhenBusinessIdIsMissing_ShouldThrowControlledValidation()
    {
        // Arrange
        var product = CreateProduct();
        product.BusinessId = null;
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(product);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("BusinessId is required.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainProduct>());
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsEmpty_ShouldThrowControlledValidation()
    {
        // Arrange
        var product = CreateProduct();
        product.Name = " ";
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(product);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Name is required.");
    }

    [Fact]
    public async Task CreateAsync_WhenProfitMarginIsNegative_ShouldThrowControlledValidation()
    {
        // Arrange
        var product = CreateProduct();
        product.ProfitMarginPercentage = -1;
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(product);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("ProfitMarginPercentage cannot be negative.");
    }

    [Fact]
    public async Task CreateAsync_WhenVatIsNegative_ShouldThrowControlledValidation()
    {
        // Arrange
        var product = CreateProduct();
        product.VatPercentage = -1;
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(product);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("VatPercentage cannot be negative.");
    }

    [Fact]
    public async Task CreateAsync_WhenExternalIdIsMissing_ShouldAllowProductCreation()
    {
        // Arrange
        var product = CreateProduct();
        product.ExternalId = null;
        _repository.CreateAsync(Arg.Any<DomainProduct>()).Returns(11);
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(product);

        // Assert
        result.Should().Be(11);
        await _repository.Received(1).CreateAsync(Arg.Is<DomainProduct>(value => value.ExternalId == null));
    }

    [Fact]
    public async Task CreateAsync_WhenSaleValueIsProvidedOnDomainEntity_ShouldIgnoreManualSaleValue()
    {
        // Arrange
        var product = CreateProduct();
        product.SaleValue = 999;
        _repository.CreateAsync(Arg.Any<DomainProduct>()).Returns(12);
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(product);

        // Assert
        result.Should().Be(12);
        await _repository.Received(1).CreateAsync(Arg.Is<DomainProduct>(value => value.SaleValue == null));
    }

    [Fact]
    public void ProductUpdate_ShouldNotExposeSaleValueAsEditableInput()
    {
        // Arrange
        var updateType = typeof(DomainProductUpdate);

        // Act
        var saleValueProperty = updateType.GetProperty("SaleValue");

        // Assert
        saleValueProperty.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenProductIsValid_ShouldUpdateWithoutSaleValue()
    {
        // Arrange
        var update = new DomainProductUpdate
        {
            Id = 10,
            BusinessId = 20,
            Name = "Nuevo Producto",
            VatPercentage = 19,
            ProfitMarginPercentage = 30,
            Status = "a"
        };
        _repository.UpdateAsync(Arg.Any<DomainProductUpdate>()).Returns(10);
        var sut = CreateSut();

        // Act
        var result = await sut.UpdateAsync(update);

        // Assert
        result.Should().Be(10);
        await _repository.Received(1).UpdateAsync(Arg.Is<DomainProductUpdate>(value =>
            value.Id == 10 && value.BusinessId == 20 && value.Status == "A"));
    }

    [Fact]
    public async Task UpdateAsync_WhenBusinessIsCleared_ShouldThrowControlledValidation()
    {
        // Arrange
        var update = new DomainProductUpdate { Id = 10, ClearBusiness = true };
        var sut = CreateSut();

        // Act
        var act = () => sut.UpdateAsync(update);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("BusinessId is required.");
    }

    [Fact]
    public async Task ListAsync_WhenFiltersAreValid_ShouldNormalizeStatusAndDelegate()
    {
        // Arrange
        _repository.ListAsync(Arg.Any<DomainProductFilter>()).Returns(new[] { new DomainProduct { Id = 10, Name = "Producto" } });
        var filter = new DomainProductFilter { BusinessId = 20, CategoryId = 2, Status = "a", Name = "  Producto  ", ExternalId = " ext-1 " };
        var sut = CreateSut();

        // Act
        var result = await sut.ListAsync(filter);

        // Assert
        result.Should().ContainSingle(product => product.Id == 10);
        await _repository.Received(1).ListAsync(Arg.Is<DomainProductFilter>(value =>
            value.BusinessId == 20 &&
            value.CategoryId == 2 &&
            value.Status == "A" &&
            value.Name == "  Producto  " &&
            value.ExternalId == " ext-1 "));
    }

    private ProductDomain CreateSut() => new(_repository);

    private static DomainProduct CreateProduct() => new()
    {
        Name = "Producto Uno",
        Description = "Descripción",
        CategoryId = 3,
        BusinessId = 20,
        CreatedByUserId = 7,
        VatPercentage = 19,
        ProfitMarginPercentage = 30,
        ExternalId = "EXT-1",
        Status = "a"
    };
}
