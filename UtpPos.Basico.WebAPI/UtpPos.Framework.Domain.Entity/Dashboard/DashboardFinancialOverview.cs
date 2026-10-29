namespace UtpPos.Framework.Domain.Entity.Dashboard
{
    public sealed class DashboardFinancialOverview
    {
        public DashboardMonthlySales MonthlySales { get; set; } = new();
        public DashboardMonthlyPurchases MonthlyPurchases { get; set; } = new();
        public IEnumerable<DashboardSalesVsPurchasesItem> SalesVsPurchases { get; set; } = Array.Empty<DashboardSalesVsPurchasesItem>();
        public DashboardOperationalBalance OperationalBalance { get; set; } = new();
        public DashboardMonthlyProjection MonthlyProjection { get; set; } = new();
        public IEnumerable<DashboardSalesByPaymentMethodItem> SalesByPaymentMethod { get; set; } = Array.Empty<DashboardSalesByPaymentMethodItem>();
    }

    public sealed class DashboardMonthlySales
    {
        public decimal Total { get; set; }
        public decimal PercentageChange { get; set; }
        public string ComparisonLabel { get; set; } = string.Empty;
    }

    public sealed class DashboardMonthlyPurchases
    {
        public decimal Total { get; set; }
        public decimal PercentageChange { get; set; }
        public string ComparisonLabel { get; set; } = string.Empty;
    }

    public sealed class DashboardSalesVsPurchasesItem
    {
        public string Label { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal PurchaseTotal { get; set; }
        public decimal Balance { get; set; }
    }

    public sealed class DashboardOperationalBalance
    {
        public decimal SalesTotal { get; set; }
        public decimal PurchaseTotal { get; set; }
        public decimal Balance { get; set; }
    }

    public sealed class DashboardMonthlyProjection
    {
        public decimal CurrentMonthSales { get; set; }
        public int ElapsedDays { get; set; }
        public int DaysInMonth { get; set; }
        public decimal DailyAverage { get; set; }
        public decimal ProjectedMonthSales { get; set; }
    }

    public sealed class DashboardSalesByPaymentMethodItem
    {
        public int PaymentMethodId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Percentage { get; set; }
    }
}
