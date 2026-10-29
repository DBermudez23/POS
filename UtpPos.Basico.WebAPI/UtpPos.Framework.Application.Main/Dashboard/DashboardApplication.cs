using UtpPos.Framework.Application.DTO.Dashboard;
using UtpPos.Framework.Application.Interface.Dashboard;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Domain.Entity.Dashboard;
using UtpPos.Framework.InfraStructure.Interface.Dashboard;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Main.Dashboard
{
    public sealed class DashboardApplication : IDashboardApplication
    {
        private const int DefaultTop = 5;
        private const int MaxTop = 20;
        private const string DashboardGlobalPermission = "dashboard-global.ver";
        private const string BusinessSelectionRequiredMessage = "Debe seleccionar un negocio para visualizar las métricas del dashboard.";

        private readonly IDashboardRepository _repository;
        private readonly IRbacApplication _rbacApplication;

        public DashboardApplication(IDashboardRepository repository, IRbacApplication rbacApplication)
        {
            _repository = repository;
            _rbacApplication = rbacApplication;
        }

        public async Task<DashboardBusinessResolutionDto> ResolveDashboardBusinessIdAsync(
            int userId,
            int? userBusinessId,
            int? requestedBusinessId)
        {
            if (userBusinessId.HasValue && userBusinessId.Value > 0)
                return Resolved(userBusinessId.Value);

            if (!requestedBusinessId.HasValue)
                return Failed(BusinessSelectionRequiredMessage);

            if (requestedBusinessId.Value <= 0)
                return Failed("El negocio seleccionado no es válido.");

            if (userId <= 0)
                return Failed("El token no contiene un usuario válido.", isUnauthorized: true);

            var permissionResponse = await _rbacApplication.UserHasPermissionAsync(userId, DashboardGlobalPermission);
            if (!permissionResponse.IsSuccess || !permissionResponse.Data)
                return Failed("No tiene permisos para consultar dashboard de otros negocios.", isForbidden: true);

            return Resolved(requestedBusinessId.Value);
        }

        public async Task<Response<DashboardBasicOverviewDto>> GetBasicOverviewAsync(
            int businessId,
            DateTime? startDate,
            DateTime? endDate,
            int? top)
        {
            try
            {
                var validationMessage = ValidateRequest(businessId, startDate, endDate);
                if (validationMessage is not null)
                    return Fail<DashboardBasicOverviewDto>(validationMessage);

                var normalizedTop = NormalizeTop(top);
                var overview = await _repository.GetBasicOverviewAsync(businessId, startDate, endDate, normalizedTop);

                return Success(MapBasicOverview(overview), "Resumen básico del dashboard consultado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<DashboardBasicOverviewDto>(ex.Message);
            }
        }

        public async Task<Response<DashboardFinancialOverviewDto>> GetFinancialOverviewAsync(
            int businessId,
            DateTime? startDate,
            DateTime? endDate)
        {
            try
            {
                var validationMessage = ValidateRequest(businessId, startDate, endDate);
                if (validationMessage is not null)
                    return Fail<DashboardFinancialOverviewDto>(validationMessage);

                var overview = await _repository.GetFinancialOverviewAsync(businessId, startDate, endDate);

                return Success(MapFinancialOverview(overview), "Resumen financiero del dashboard consultado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<DashboardFinancialOverviewDto>(ex.Message);
            }
        }

        private static string? ValidateRequest(int businessId, DateTime? startDate, DateTime? endDate)
        {
            if (businessId <= 0)
                return "El negocio del token no es válido.";

            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
                return "La fecha inicial no puede ser mayor que la fecha final.";

            return null;
        }

        private static int NormalizeTop(int? top)
        {
            if (!top.HasValue || top.Value <= 0)
                return DefaultTop;

            return Math.Min(top.Value, MaxTop);
        }

        private static DashboardBusinessResolutionDto Resolved(int businessId)
        {
            return new DashboardBusinessResolutionDto
            {
                IsSuccess = true,
                BusinessId = businessId
            };
        }

        private static DashboardBusinessResolutionDto Failed(
            string message,
            bool isUnauthorized = false,
            bool isForbidden = false)
        {
            return new DashboardBusinessResolutionDto
            {
                IsSuccess = false,
                IsUnauthorized = isUnauthorized,
                IsForbidden = isForbidden,
                Message = message
            };
        }

        private static DashboardBasicOverviewDto MapBasicOverview(DashboardBasicOverview overview)
        {
            return new DashboardBasicOverviewDto
            {
                DailySales = new DashboardDailySalesDto
                {
                    Total = overview.DailySales.Total,
                    SalesCount = overview.DailySales.SalesCount,
                    PercentageChange = overview.DailySales.PercentageChange,
                    ComparisonLabel = overview.DailySales.ComparisonLabel
                },
                AverageTicket = new DashboardAverageTicketDto
                {
                    Value = overview.AverageTicket.Value,
                    SalesCount = overview.AverageTicket.SalesCount
                },
                WeeklySales = overview.WeeklySales.Select(item => new DashboardWeeklySalesItemDto
                {
                    Label = item.Label,
                    Date = item.Date,
                    Total = item.Total,
                    SalesCount = item.SalesCount,
                    AverageTicket = item.AverageTicket
                }).ToList(),
                SalesByCategory = overview.SalesByCategory.Select(item => new DashboardSalesByCategoryItemDto
                {
                    CategoryId = item.CategoryId,
                    CategoryName = item.CategoryName,
                    Total = item.Total,
                    Percentage = item.Percentage
                }).ToList(),
                RecentSales = overview.RecentSales.Select(item => new DashboardRecentSaleItemDto
                {
                    SaleId = item.SaleId,
                    InvoiceNumber = item.InvoiceNumber,
                    CustomerName = item.CustomerName,
                    Time = item.Time,
                    PaymentMethod = item.PaymentMethod,
                    Status = item.Status,
                    Total = item.Total
                }).ToList(),
                InventoryAlerts = overview.InventoryAlerts.Select(item => new DashboardInventoryAlertItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Sku = item.Sku,
                    CurrentStock = item.CurrentStock,
                    MinimumStock = item.MinimumStock,
                    Severity = item.Severity
                }).ToList(),
                StockAlertsCount = new DashboardStockAlertsCountDto
                {
                    TotalAlerts = overview.StockAlertsCount.TotalAlerts,
                    CriticalAlerts = overview.StockAlertsCount.CriticalAlerts
                },
                RecentKardexMovements = overview.RecentKardexMovements.Select(item => new DashboardRecentKardexMovementItemDto
                {
                    KardexId = item.KardexId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    MovementType = item.MovementType,
                    Quantity = item.Quantity,
                    Date = item.Date
                }).ToList()
            };
        }

        private static DashboardFinancialOverviewDto MapFinancialOverview(DashboardFinancialOverview overview)
        {
            return new DashboardFinancialOverviewDto
            {
                MonthlySales = new DashboardMonthlySalesDto
                {
                    Total = overview.MonthlySales.Total,
                    PercentageChange = overview.MonthlySales.PercentageChange,
                    ComparisonLabel = overview.MonthlySales.ComparisonLabel
                },
                MonthlyPurchases = new DashboardMonthlyPurchasesDto
                {
                    Total = overview.MonthlyPurchases.Total,
                    PercentageChange = overview.MonthlyPurchases.PercentageChange,
                    ComparisonLabel = overview.MonthlyPurchases.ComparisonLabel
                },
                SalesVsPurchases = overview.SalesVsPurchases.Select(item => new DashboardSalesVsPurchasesItemDto
                {
                    Label = item.Label,
                    PeriodStart = item.PeriodStart,
                    PeriodEnd = item.PeriodEnd,
                    SalesTotal = item.SalesTotal,
                    PurchaseTotal = item.PurchaseTotal,
                    Balance = item.Balance
                }).ToList(),
                OperationalBalance = new DashboardOperationalBalanceDto
                {
                    SalesTotal = overview.OperationalBalance.SalesTotal,
                    PurchaseTotal = overview.OperationalBalance.PurchaseTotal,
                    Balance = overview.OperationalBalance.Balance
                },
                MonthlyProjection = new DashboardMonthlyProjectionDto
                {
                    CurrentMonthSales = overview.MonthlyProjection.CurrentMonthSales,
                    ElapsedDays = overview.MonthlyProjection.ElapsedDays,
                    DaysInMonth = overview.MonthlyProjection.DaysInMonth,
                    DailyAverage = overview.MonthlyProjection.DailyAverage,
                    ProjectedMonthSales = overview.MonthlyProjection.ProjectedMonthSales
                },
                SalesByPaymentMethod = overview.SalesByPaymentMethod.Select(item => new DashboardSalesByPaymentMethodItemDto
                {
                    PaymentMethodId = item.PaymentMethodId,
                    PaymentMethod = item.PaymentMethod,
                    Total = item.Total,
                    Percentage = item.Percentage
                }).ToList()
            };
        }

        private static Response<T> Success<T>(T data, string message)
        {
            return new Response<T> { IsSuccess = true, Data = data, Message = message };
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T> { IsSuccess = false, Data = default, Message = message };
        }
    }
}
