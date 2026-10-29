using System.Data;
using Dapper;
using UtpPos.Framework.Domain.Entity.Dashboard;
using UtpPos.Framework.InfraStructure.Interface.Dashboard;
using UtpPos.Framework.Transversal.Interface;

namespace UtpPos.Framework.InfraStructure.Repository.Dashboard
{
    public sealed class DashboardRepository : IDashboardRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public DashboardRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<DashboardBasicOverview> GetBasicOverviewAsync(
            int businessId,
            DateTime? startDate,
            DateTime? endDate,
            int top)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NEGOCIO_ID", businessId, DbType.Int32);
            parameters.Add("@FECHA_INICIO", startDate?.Date, DbType.Date);
            parameters.Add("@FECHA_FIN", endDate?.Date, DbType.Date);
            parameters.Add("@TOP", top, DbType.Int32);

            using var grids = await connection.QueryMultipleAsync(
                "dbo.UspDashboardResumenBasico",
                parameters,
                commandType: CommandType.StoredProcedure);

            return new DashboardBasicOverview
            {
                DailySales = await grids.ReadFirstOrDefaultAsync<DashboardDailySales>() ?? new DashboardDailySales(),
                AverageTicket = await grids.ReadFirstOrDefaultAsync<DashboardAverageTicket>() ?? new DashboardAverageTicket(),
                WeeklySales = (await grids.ReadAsync<DashboardWeeklySalesItem>()).ToList(),
                SalesByCategory = (await grids.ReadAsync<DashboardSalesByCategoryItem>()).ToList(),
                RecentSales = (await grids.ReadAsync<DashboardRecentSaleItem>()).ToList(),
                InventoryAlerts = (await grids.ReadAsync<DashboardInventoryAlertItem>()).ToList(),
                StockAlertsCount = await grids.ReadFirstOrDefaultAsync<DashboardStockAlertsCount>() ?? new DashboardStockAlertsCount(),
                RecentKardexMovements = (await grids.ReadAsync<DashboardRecentKardexMovementItem>()).ToList()
            };
        }

        public async Task<DashboardFinancialOverview> GetFinancialOverviewAsync(
            int businessId,
            DateTime? startDate,
            DateTime? endDate)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NEGOCIO_ID", businessId, DbType.Int32);
            parameters.Add("@FECHA_INICIO", startDate?.Date, DbType.Date);
            parameters.Add("@FECHA_FIN", endDate?.Date, DbType.Date);

            using var grids = await connection.QueryMultipleAsync(
                "dbo.UspDashboardResumenFinanciero",
                parameters,
                commandType: CommandType.StoredProcedure);

            return new DashboardFinancialOverview
            {
                MonthlySales = await grids.ReadFirstOrDefaultAsync<DashboardMonthlySales>() ?? new DashboardMonthlySales(),
                MonthlyPurchases = await grids.ReadFirstOrDefaultAsync<DashboardMonthlyPurchases>() ?? new DashboardMonthlyPurchases(),
                SalesVsPurchases = (await grids.ReadAsync<DashboardSalesVsPurchasesItem>()).ToList(),
                OperationalBalance = await grids.ReadFirstOrDefaultAsync<DashboardOperationalBalance>() ?? new DashboardOperationalBalance(),
                MonthlyProjection = await grids.ReadFirstOrDefaultAsync<DashboardMonthlyProjection>() ?? new DashboardMonthlyProjection(),
                SalesByPaymentMethod = (await grids.ReadAsync<DashboardSalesByPaymentMethodItem>()).ToList()
            };
        }
    }
}
