using UtpPos.Framework.Domain.Entity.Dashboard;
using UtpPos.Framework.InfraStructure.Interface.Dashboard;

namespace UtpPos.WebAPI.IntegrationTests.Fakes;

public sealed class FakeDashboardRepository : IDashboardRepository
{
    public int BasicOverviewCalls { get; private set; }
    public int FinancialOverviewCalls { get; private set; }
    public int? LastBusinessId { get; private set; }
    public DateTime? LastStartDate { get; private set; }
    public DateTime? LastEndDate { get; private set; }
    public int? LastTop { get; private set; }

    public Task<DashboardBasicOverview> GetBasicOverviewAsync(
        int businessId,
        DateTime? startDate,
        DateTime? endDate,
        int top)
    {
        BasicOverviewCalls++;
        LastBusinessId = businessId;
        LastStartDate = startDate;
        LastEndDate = endDate;
        LastTop = top;

        return Task.FromResult(new DashboardBasicOverview
        {
            DailySales = new DashboardDailySales { Total = 100000, SalesCount = 2 }
        });
    }

    public Task<DashboardFinancialOverview> GetFinancialOverviewAsync(
        int businessId,
        DateTime? startDate,
        DateTime? endDate)
    {
        FinancialOverviewCalls++;
        LastBusinessId = businessId;
        LastStartDate = startDate;
        LastEndDate = endDate;
        LastTop = null;

        return Task.FromResult(new DashboardFinancialOverview
        {
            MonthlySales = new DashboardMonthlySales { Total = 300000 }
        });
    }
}
