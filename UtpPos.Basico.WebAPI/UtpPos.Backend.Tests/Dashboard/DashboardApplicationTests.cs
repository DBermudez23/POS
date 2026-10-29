using FluentAssertions;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Application.Main.Dashboard;
using UtpPos.Framework.Domain.Entity.Dashboard;
using UtpPos.Framework.InfraStructure.Interface.Dashboard;
using UtpPos.Framework.Transversal.Common;
using NSubstitute;

namespace UtpPos.Backend.Tests.Dashboard;

public sealed class DashboardApplicationTests
{
    private readonly IDashboardRepository _repository = Substitute.For<IDashboardRepository>();
    private readonly IRbacApplication _rbacApplication = Substitute.For<IRbacApplication>();

    [Fact]
    public async Task ResolveDashboardBusinessIdAsync_WhenUserHasBusinessClaim_ShouldUseClaimAndIgnoreRequestedBusinessId()
    {
        var sut = CreateSut();

        var result = await sut.ResolveDashboardBusinessIdAsync(5, userBusinessId: 7, requestedBusinessId: 999);

        result.IsSuccess.Should().BeTrue();
        result.BusinessId.Should().Be(7);
        await _rbacApplication.DidNotReceive().UserHasPermissionAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveDashboardBusinessIdAsync_WhenGlobalUserDoesNotSendBusinessId_ShouldRequireSelection()
    {
        var sut = CreateSut();

        var result = await sut.ResolveDashboardBusinessIdAsync(5, userBusinessId: null, requestedBusinessId: null);

        result.IsSuccess.Should().BeFalse();
        result.IsForbidden.Should().BeFalse();
        result.Message.Should().Be("Debe seleccionar un negocio para visualizar las métricas del dashboard.");
        await _rbacApplication.DidNotReceive().UserHasPermissionAsync(Arg.Any<int>(), Arg.Any<string>());
    }

    [Fact]
    public async Task ResolveDashboardBusinessIdAsync_WhenGlobalUserSendsBusinessIdWithoutGlobalPermission_ShouldForbid()
    {
        _rbacApplication.UserHasPermissionAsync(5, "dashboard-global.ver").Returns(new Response<bool>
        {
            IsSuccess = true,
            Data = false,
            Message = "Forbidden"
        });
        var sut = CreateSut();

        var result = await sut.ResolveDashboardBusinessIdAsync(5, userBusinessId: null, requestedBusinessId: 7);

        result.IsSuccess.Should().BeFalse();
        result.IsForbidden.Should().BeTrue();
        await _rbacApplication.Received(1).UserHasPermissionAsync(5, "dashboard-global.ver");
    }

    [Fact]
    public async Task ResolveDashboardBusinessIdAsync_WhenGlobalUserHasGlobalPermission_ShouldUseRequestedBusinessId()
    {
        _rbacApplication.UserHasPermissionAsync(5, "dashboard-global.ver").Returns(new Response<bool>
        {
            IsSuccess = true,
            Data = true,
            Message = "OK"
        });
        var sut = CreateSut();

        var result = await sut.ResolveDashboardBusinessIdAsync(5, userBusinessId: null, requestedBusinessId: 7);

        result.IsSuccess.Should().BeTrue();
        result.BusinessId.Should().Be(7);
        await _rbacApplication.Received(1).UserHasPermissionAsync(5, "dashboard-global.ver");
    }

    [Fact]
    public async Task GetBasicOverviewAsync_WhenBusinessIdIsInvalid_ShouldFailWithoutCallingRepository()
    {
        var sut = CreateSut();

        var result = await sut.GetBasicOverviewAsync(0, null, null, null);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("El negocio del token no es válido.");
        await _repository.DidNotReceive().GetBasicOverviewAsync(
            Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>());
    }

    [Fact]
    public async Task GetBasicOverviewAsync_WhenStartDateIsGreaterThanEndDate_ShouldFailWithoutCallingRepository()
    {
        var sut = CreateSut();

        var result = await sut.GetBasicOverviewAsync(7, new DateTime(2026, 2, 1), new DateTime(2026, 1, 1), null);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("La fecha inicial no puede ser mayor que la fecha final.");
        await _repository.DidNotReceive().GetBasicOverviewAsync(
            Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>());
    }

    [Fact]
    public async Task GetBasicOverviewAsync_WhenTopIsMissing_ShouldUseDefaultTop()
    {
        _repository.GetBasicOverviewAsync(Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>())
            .Returns(new DashboardBasicOverview());
        var sut = CreateSut();

        var result = await sut.GetBasicOverviewAsync(7, null, null, null);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetBasicOverviewAsync(7, null, null, 5);
    }

    [Fact]
    public async Task GetBasicOverviewAsync_WhenTopExceedsLimit_ShouldCapTopAtTwenty()
    {
        _repository.GetBasicOverviewAsync(Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>())
            .Returns(new DashboardBasicOverview());
        var sut = CreateSut();

        var result = await sut.GetBasicOverviewAsync(7, null, null, 50);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).GetBasicOverviewAsync(7, null, null, 20);
    }

    [Fact]
    public async Task GetBasicOverviewAsync_WhenRepositoryReturnsData_ShouldMapResponse()
    {
        _repository.GetBasicOverviewAsync(Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(), Arg.Any<int>())
            .Returns(new DashboardBasicOverview
            {
                DailySales = new DashboardDailySales { Total = 120000, SalesCount = 3, PercentageChange = 10, ComparisonLabel = "vs ayer" },
                WeeklySales = new[] { new DashboardWeeklySalesItem { Label = "Hoy", Date = new DateTime(2026, 1, 1), Total = 120000, SalesCount = 3, AverageTicket = 40000 } }
            });
        var sut = CreateSut();

        var result = await sut.GetBasicOverviewAsync(7, null, null, 5);

        result.IsSuccess.Should().BeTrue();
        result.Data!.DailySales.Total.Should().Be(120000);
        result.Data.WeeklySales.Should().ContainSingle(item => item.Label == "Hoy" && item.AverageTicket == 40000);
    }

    [Fact]
    public async Task GetFinancialOverviewAsync_WhenStartDateIsGreaterThanEndDate_ShouldFailWithoutCallingRepository()
    {
        var sut = CreateSut();

        var result = await sut.GetFinancialOverviewAsync(7, new DateTime(2026, 2, 1), new DateTime(2026, 1, 1));

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("La fecha inicial no puede ser mayor que la fecha final.");
        await _repository.DidNotReceive().GetFinancialOverviewAsync(
            Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>());
    }

    [Fact]
    public async Task GetFinancialOverviewAsync_WhenRepositoryReturnsData_ShouldMapResponse()
    {
        _repository.GetFinancialOverviewAsync(Arg.Any<int>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>())
            .Returns(new DashboardFinancialOverview
            {
                MonthlySales = new DashboardMonthlySales { Total = 300000, PercentageChange = 5, ComparisonLabel = "vs mes anterior" },
                OperationalBalance = new DashboardOperationalBalance { SalesTotal = 300000, PurchaseTotal = 100000, Balance = 200000 }
            });
        var sut = CreateSut();

        var result = await sut.GetFinancialOverviewAsync(7, null, null);

        result.IsSuccess.Should().BeTrue();
        result.Data!.MonthlySales.Total.Should().Be(300000);
        result.Data.OperationalBalance.Balance.Should().Be(200000);
    }

    private DashboardApplication CreateSut() => new(_repository, _rbacApplication);
}
