using UtpPos.Framework.Domain.Entity.Dashboard;

namespace UtpPos.Framework.InfraStructure.Interface.Dashboard
{
    public interface IDashboardRepository
    {
        Task<DashboardBasicOverview> GetBasicOverviewAsync(
            int businessId,
            DateTime? startDate,
            DateTime? endDate,
            int top);

        Task<DashboardFinancialOverview> GetFinancialOverviewAsync(
            int businessId,
            DateTime? startDate,
            DateTime? endDate);
    }
}
