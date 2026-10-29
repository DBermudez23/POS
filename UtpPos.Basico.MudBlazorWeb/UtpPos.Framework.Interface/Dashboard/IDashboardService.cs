using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Dashboard;

namespace UtpPos.Framework.Interface.Dashboard
{
    public interface IDashboardService
    {
        Task<Response<DashboardBasicOverviewDto>> GetBasicOverviewAsync(DashboardOverviewFilterDto filter);
        Task<Response<DashboardFinancialOverviewDto>> GetFinancialOverviewAsync(DashboardOverviewFilterDto filter);
    }
}
