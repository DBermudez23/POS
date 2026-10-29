namespace UtpPos.Framework.Domain.Entity.Dashboard
{
    public sealed class DashboardBasicOverview
    {
        public DashboardDailySales DailySales { get; set; } = new();
        public DashboardAverageTicket AverageTicket { get; set; } = new();
        public IEnumerable<DashboardWeeklySalesItem> WeeklySales { get; set; } = Array.Empty<DashboardWeeklySalesItem>();
        public IEnumerable<DashboardSalesByCategoryItem> SalesByCategory { get; set; } = Array.Empty<DashboardSalesByCategoryItem>();
        public IEnumerable<DashboardRecentSaleItem> RecentSales { get; set; } = Array.Empty<DashboardRecentSaleItem>();
        public IEnumerable<DashboardInventoryAlertItem> InventoryAlerts { get; set; } = Array.Empty<DashboardInventoryAlertItem>();
        public DashboardStockAlertsCount StockAlertsCount { get; set; } = new();
        public IEnumerable<DashboardRecentKardexMovementItem> RecentKardexMovements { get; set; } = Array.Empty<DashboardRecentKardexMovementItem>();
    }

    public sealed class DashboardDailySales
    {
        public decimal Total { get; set; }
        public int SalesCount { get; set; }
        public decimal PercentageChange { get; set; }
        public string ComparisonLabel { get; set; } = string.Empty;
    }

    public sealed class DashboardAverageTicket
    {
        public decimal Value { get; set; }
        public int SalesCount { get; set; }
    }

    public sealed class DashboardWeeklySalesItem
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public int SalesCount { get; set; }
        public decimal AverageTicket { get; set; }
    }

    public sealed class DashboardSalesByCategoryItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Percentage { get; set; }
    }

    public sealed class DashboardRecentSaleItem
    {
        public int SaleId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public sealed class DashboardInventoryAlertItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    public sealed class DashboardStockAlertsCount
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
    }

    public sealed class DashboardRecentKardexMovementItem
    {
        public int KardexId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime Date { get; set; }
    }
}
