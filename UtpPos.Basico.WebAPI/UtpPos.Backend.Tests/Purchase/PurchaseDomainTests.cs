using FluentAssertions;
using UtpPos.Framework.Domain.Core.Purchase;
using UtpPos.Framework.InfraStructure.Interface.Product;
using UtpPos.Framework.InfraStructure.Interface.Purchase;
using UtpPos.Framework.InfraStructure.Interface.ThirdParty;
using NSubstitute;
using DomainProduct = UtpPos.Framework.Domain.Entity.Product.Product;
using DomainPurchaseCreate = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreate;
using DomainPurchaseCreateDetail = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreateDetail;
using DomainPurchaseCreatePayment = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreatePayment;
using DomainPurchaseWriteResult = UtpPos.Framework.Domain.Entity.Purchase.PurchaseWriteResult;

namespace UtpPos.Backend.Tests.Purchase;

public sealed class PurchaseDomainTests
{
    private readonly IPurchaseRepository _repository = Substitute.For<IPurchaseRepository>();
    private readonly IThirdPartyRepository _thirdPartyRepository = Substitute.For<IThirdPartyRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();

    [Fact]
    public async Task CreateAsync_WhenPurchaseIsValid_ShouldRegisterPurchase()
    {
        // Arrange
        var purchase = CreatePurchase();
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, 20));
        _repository.CreateAsync(Arg.Any<DomainPurchaseCreate>()).Returns(new DomainPurchaseWriteResult { PurchaseId = 100, Total = 119 });
        var sut = CreateSut();

        // Act
        var result = await sut.CreateAsync(purchase);

        // Assert
        result.PurchaseId.Should().Be(100);
        await _thirdPartyRepository.Received(1).ValidateBusinessAsync(20, 30);
        await _repository.Received(1).CreateAsync(purchase);
    }

    [Fact]
    public async Task CreateAsync_WhenBusinessIdIsInvalid_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        purchase.BusinessId = 0;
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("BusinessId must be greater than zero.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainPurchaseCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenSupplierIdIsInvalid_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        purchase.ThirdPartyId = 0;
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("ThirdPartyId must be greater than zero.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainPurchaseCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenSupplierIsNotAssociatedToBusiness_ShouldThrowExpectedErrorAndNotCreatePurchase()
    {
        // Arrange
        var purchase = CreatePurchase();
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(false);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("El proveedor no está asociado al negocio de la compra.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainPurchaseCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenPurchaseHasNoDetails_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        purchase.Details = Array.Empty<DomainPurchaseCreateDetail>();
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Purchase must include at least one detail.");
    }

    [Fact]
    public async Task CreateAsync_WhenQuantityIsZeroOrNegative_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        purchase.Details = new[] { new DomainPurchaseCreateDetail { ProductId = 40, Quantity = 0, UnitValue = 100, VatPercentage = 19 } };
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Quantity must be greater than zero.");
    }

    [Fact]
    public async Task CreateAsync_WhenUnitValueIsZero_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        purchase.Details = new[] { new DomainPurchaseCreateDetail { ProductId = 40, Quantity = 1, UnitValue = 0, VatPercentage = 19 } };
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("UnitValue must be greater than zero.");
        await _productRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task CreateAsync_WhenUnitValueIsNegative_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        purchase.Details = new[] { new DomainPurchaseCreateDetail { ProductId = 40, Quantity = 1, UnitValue = -1, VatPercentage = 19 } };
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("UnitValue must be greater than zero.");
        await _productRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task CreateAsync_WhenProductDoesNotBelongToBusiness_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, businessId: 999));
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Producto no encontrado para el negocio de la compra.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainPurchaseCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenProductDoesNotExist_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns((DomainProduct?)null);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Producto no encontrado para el negocio de la compra.");
        await _repository.DidNotReceive().CreateAsync(Arg.Any<DomainPurchaseCreate>());
    }

    [Fact]
    public async Task CreateAsync_WhenProductIdIsInvalid_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        purchase.Details = new[] { new DomainPurchaseCreateDetail { ProductId = 0, Quantity = 1, UnitValue = 100, VatPercentage = 19 } };
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("ProductId must be greater than zero.");
    }

    [Fact]
    public async Task CreateAsync_WhenPurchaseHasNoPayments_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        purchase.Payments = Array.Empty<DomainPurchaseCreatePayment>();
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Purchase must include at least one payment.");
    }

    [Fact]
    public async Task CreateAsync_WhenPaymentChangeIsGreaterThanReceivedValue_ShouldThrowControlledValidation()
    {
        // Arrange
        var purchase = CreatePurchase();
        purchase.Payments = new[] { new DomainPurchaseCreatePayment { PaymentMethodId = 50, ReceivedValue = 100, ChangeValue = 101 } };
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, 20));
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("ChangeValue cannot be greater than ReceivedValue.");
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryThrows_ShouldPropagateCurrentDomainError()
    {
        // Arrange
        var purchase = CreatePurchase();
        _thirdPartyRepository.ValidateBusinessAsync(purchase.BusinessId, purchase.ThirdPartyId).Returns(true);
        _productRepository.GetByIdAsync(40).Returns(Product(40, 20));
        _repository.CreateAsync(Arg.Any<DomainPurchaseCreate>()).Returns<Task<DomainPurchaseWriteResult>>(_ => throw new InvalidOperationException("repository failed"));
        var sut = CreateSut();

        // Act
        var act = () => sut.CreateAsync(purchase);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("repository failed");
    }

    private PurchaseDomain CreateSut() => new(_repository, _thirdPartyRepository, _productRepository);

    private static DomainProduct Product(int id, int businessId) => new()
    {
        Id = id,
        BusinessId = businessId,
        Name = "Producto",
        Status = "A"
    };

    private static DomainPurchaseCreate CreatePurchase() => new()
    {
        BusinessId = 20,
        ThirdPartyId = 30,
        UserId = 7,
        KardexMovementTypeId = 60,
        Details = new[]
        {
            new DomainPurchaseCreateDetail
            {
                ProductId = 40,
                Quantity = 2,
                UnitValue = 100,
                VatPercentage = 19
            }
        },
        Payments = new[]
        {
            new DomainPurchaseCreatePayment
            {
                PaymentMethodId = 50,
                ReceivedValue = 238,
                ChangeValue = 0
            }
        }
    };
}
