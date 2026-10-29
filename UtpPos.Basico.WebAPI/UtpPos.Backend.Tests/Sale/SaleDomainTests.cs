using FluentAssertions;
using UtpPos.Framework.Domain.Core.Sale;
using UtpPos.Framework.InfraStructure.Interface.Product;
using UtpPos.Framework.InfraStructure.Interface.Sale;
using UtpPos.Framework.InfraStructure.Interface.ThirdParty;
using NSubstitute;
using DomainProduct = UtpPos.Framework.Domain.Entity.Product.Product;
using DomainSaleCreate = UtpPos.Framework.Domain.Entity.Sale.SaleCreate;
using DomainSaleCreateDetail = UtpPos.Framework.Domain.Entity.Sale.SaleCreateDetail;
using DomainSaleCreatePayment = UtpPos.Framework.Domain.Entity.Sale.SaleCreatePayment;
using DomainSaleWriteResult = UtpPos.Framework.Domain.Entity.Sale.SaleWriteResult;

namespace UtpPos.Backend.Tests.Sale;

public sealed class SaleDomainTests
{
    private readonly ISaleRepository _repository = Substitute.For<ISaleRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IThirdPartyRepository _thirdPartyRepository = Substitute.For<IThirdPartyRepository>();

    [Fact]
    public async Task CreateAsync_WhenSaleIsValid_ShouldRegisterSale()
    {
        // Arrange
        var sale = CreateSale();
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, businessId: 20, saleValue: 150, vatPercentage: 19));
        _repository.CreateAsync(Arg.Any<DomainSaleCreate>()).Returns(new DomainSaleWriteResult { SaleId = 100, Total = 357 });
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(sale);

        // Assert
        result.SaleId.Should().Be(100);
        await _thirdPartyRepository.Received(1).ValidateBusinessAsync(20, 30);
        await _repository.Received(1).CreateAsync(Arg.Any<DomainSaleCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenBusinessIdIsInvalid_ShouldThrowControlledValidation()
    {
        // Arrange
        var sale = CreateSale();
        sale.BusinessId = 0;
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("BusinessId must be greater than zero.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainSaleCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenCustomerIdIsInvalid_ShouldThrowControlledValidation()
    {
        // Arrange
        var sale = CreateSale();
        sale.ThirdPartyId = 0;
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("ThirdPartyId must be greater than zero.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainSaleCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenCustomerIsNotAssociatedToBusiness_ShouldThrowExpectedErrorAndNotCreateSale()
    {
        // Arrange
        var sale = CreateSale();
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(false);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("El cliente no está asociado al negocio de la venta.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainSaleCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenSaleHasNoDetails_ShouldThrowControlledValidation()
    {
        // Arrange
        var sale = CreateSale();
        sale.Details = Array.Empty<DomainSaleCreateDetail>();
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(true);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Sale must include at least one detail.");
    }

    [Fact]
    public async Task CreateAsync_WhenQuantityIsZeroOrNegative_ShouldThrowControlledValidation()
    {
        // Arrange
        var sale = CreateSale();
        sale.Details = new[] { new DomainSaleCreateDetail { ProductId = 40, Quantity = 0, UnitValue = 999, VatPercentage = 0 } };
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(true);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public async Task CreateAsync_WhenProductHasNoCalculatedSaleValue_ShouldThrowExpectedError()
    {
        // Arrange
        var sale = CreateSale();
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, businessId: 20, saleValue: 0, vatPercentage: 19));
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("El producto no tiene valor de venta calculado. Registre una compra o revise el margen del producto.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainSaleCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenProductDoesNotBelongToBusiness_ShouldThrowControlledValidation()
    {
        // Arrange
        var sale = CreateSale();
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, businessId: 999, saleValue: 150, vatPercentage: 19));
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Producto no encontrado para el negocio de la venta.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainSaleCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenUnitValueIsManipulated_ShouldOverwriteWithProductSaleValue()
    {
        // Arrange
        var sale = CreateSale();
        sale.Details = new[] { new DomainSaleCreateDetail { ProductId = 40, Quantity = 2, UnitValue = 1, VatPercentage = 0 } };
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, businessId: 20, saleValue: 150, vatPercentage: 19));
        _repository.CreateAsync(Arg.Any<DomainSaleCreate>()).Returns(new DomainSaleWriteResult { SaleId = 100 });
        var sut = CreateSut();

        // Act
        await sut.CreateAsync(sale);

        // Assert
        await _repository.Received(1).CreateAsync(Arg.Is<DomainSaleCreate>(value =>
            value.Details.Single().UnitValue == 150 &&
            value.Details.Single().VatPercentage == 19));
    }

    [Fact]
    public async Task CreateAsync_WhenSaleHasNoPayments_ShouldThrowControlledValidation()
    {
        // Arrange
        var sale = CreateSale();
        sale.Payments = Array.Empty<DomainSaleCreatePayment>();
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(true);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Sale must include at least one payment.");
    }

    [Fact]
    public async Task CreateAsync_WhenPaymentChangeIsGreaterThanReceivedValue_ShouldThrowControlledValidation()
    {
        // Arrange
        var sale = CreateSale();
        sale.Payments = new[] { new DomainSaleCreatePayment { PaymentMethodId = 50, ReceivedValue = 100, ChangeValue = 101 } };
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, businessId: 20, saleValue: 150, vatPercentage: 19));
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("ChangeValue cannot be greater than ReceivedValue.");
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryThrows_ShouldPropagateCurrentDomainError()
    {
        // Arrange
        var sale = CreateSale();
        _thirdPartyRepository.ValidateBusinessAsync(sale.BusinessId, sale.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, businessId: 20, saleValue: 150, vatPercentage: 19));
        _repository.CreateAsync(Arg.Any<DomainSaleCreate>()).Returns<Task<DomainSaleWriteResult>>(_ => throw new InvalidOperationException("repository failed"));
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(sale);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("repository failed");
    }

    private SaleDomain CreateSut() => new(_repository, _productRepository, _thirdPartyRepository);

    private static DomainSaleCreate CreateSale() => new()
    {
        BusinessId = 20,
        ThirdPartyId = 30,
        UserId = 7,
        KardexMovementTypeId = 60,
        Details = new[]
        {
            new DomainSaleCreateDetail
            {
                ProductId = 40,
                Quantity = 2,
                UnitValue = 1,
                VatPercentage = 0
            }
        },
        Payments = new[]
        {
            new DomainSaleCreatePayment
            {
                PaymentMethodId = 50,
                ReceivedValue = 357,
                ChangeValue = 0
            }
        }
    };

    private static DomainProduct Product(int id, int businessId, decimal saleValue, decimal vatPercentage) => new()
    {
        Id = id,
        BusinessId = businessId,
        Name = "Producto",
        SaleValue = saleValue,
        VatPercentage = vatPercentage,
        Status = "A"
    };
}
