using UtpPos.Framework.Application.DTO.Dashboard;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Interface.Dashboard
{
    public interface IDashboardApplication
    {
        Task<DashboardBusinessResolutionDto> ResolveDashboardBusinessIdAsync(
            int userId,
            int? userBusinessId,
            int? requestedBusinessId);

        Task<Response<DashboardBasicOverviewDto>> GetBasicOverviewAsync(
            int businessId,
            DateTime? startDate,
            DateTime? endDate,
            int? top);

        Task<Response<DashboardFinancialOverviewDto>> GetFinancialOverviewAsync(
            int businessId,
            DateTime? startDate,
            DateTime? endDate);
    }
}
