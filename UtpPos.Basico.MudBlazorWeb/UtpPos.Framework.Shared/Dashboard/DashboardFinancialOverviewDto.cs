namespace UtpPos.Framework.Shared.Dashboard
{
    public sealed class DashboardFinancialOverviewDto
    {
        public DashboardMonthlySalesDto MonthlySales { get; set; } = new();
        public DashboardMonthlyPurchasesDto MonthlyPurchases { get; set; } = new();
        public IReadOnlyList<DashboardSalesVsPurchasesItemDto> SalesVsPurchases { get; set; } = Array.Empty<DashboardSalesVsPurchasesItemDto>();
        public DashboardOperationalBalanceDto OperationalBalance { get; set; } = new();
        public DashboardMonthlyProjectionDto MonthlyProjection { get; set; } = new();
        public IReadOnlyList<DashboardSalesByPaymentMethodItemDto> SalesByPaymentMethod { get; set; } = Array.Empty<DashboardSalesByPaymentMethodItemDto>();
    }

    public sealed class DashboardMonthlySalesDto
    {
        public decimal Total { get; set; }
        public decimal PercentageChange { get; set; }
        public string ComparisonLabel { get; set; } = string.Empty;
    }

    public sealed class DashboardMonthlyPurchasesDto
    {
        public decimal Total { get; set; }
        public decimal PercentageChange { get; set; }
        public string ComparisonLabel { get; set; } = string.Empty;
    }

    public sealed class DashboardSalesVsPurchasesItemDto
    {
        public string Label { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal PurchaseTotal { get; set; }
        public decimal Balance { get; set; }
    }

    public sealed class DashboardOperationalBalanceDto
    {
        public decimal SalesTotal { get; set; }
        public decimal PurchaseTotal { get; set; }
        public decimal Balance { get; set; }
    }

    public sealed class DashboardMonthlyProjectionDto
    {
        public decimal CurrentMonthSales { get; set; }
        public int ElapsedDays { get; set; }
        public int DaysInMonth { get; set; }
        public decimal DailyAverage { get; set; }
        public decimal ProjectedMonthSales { get; set; }
    }

    public sealed class DashboardSalesByPaymentMethodItemDto
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Percentage { get; set; }
    }
}
