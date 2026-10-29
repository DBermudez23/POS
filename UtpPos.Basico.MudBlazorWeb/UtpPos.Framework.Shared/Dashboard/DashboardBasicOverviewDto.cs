namespace UtpPos.Framework.Shared.Dashboard
{
    public sealed class DashboardBasicOverviewDto
    {
        public DashboardDailySalesDto DailySales { get; set; } = new();
        public DashboardAverageTicketDto AverageTicket { get; set; } = new();
        public IReadOnlyList<DashboardWeeklySalesItemDto> WeeklySales { get; set; } = Array.Empty<DashboardWeeklySalesItemDto>();
        public IReadOnlyList<DashboardSalesByCategoryItemDto> SalesByCategory { get; set; } = Array.Empty<DashboardSalesByCategoryItemDto>();
        public IReadOnlyList<DashboardRecentSaleItemDto> RecentSales { get; set; } = Array.Empty<DashboardRecentSaleItemDto>();
        public IReadOnlyList<DashboardInventoryAlertItemDto> InventoryAlerts { get; set; } = Array.Empty<DashboardInventoryAlertItemDto>();
        public DashboardStockAlertsCountDto StockAlertsCount { get; set; } = new();
        public IReadOnlyList<DashboardRecentKardexMovementItemDto> RecentKardexMovements { get; set; } = Array.Empty<DashboardRecentKardexMovementItemDto>();
    }

    public sealed class DashboardDailySalesDto
    {
        public decimal Total { get; set; }
        public int SalesCount { get; set; }
        public decimal PercentageChange { get; set; }
        public string ComparisonLabel { get; set; } = string.Empty;
    }

    public sealed class DashboardAverageTicketDto
    {
        public decimal Value { get; set; }
        public int SalesCount { get; set; }
    }

    public sealed class DashboardWeeklySalesItemDto
    {
        public string Label { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public int SalesCount { get; set; }
        public decimal AverageTicket { get; set; }
    }

    public sealed class DashboardSalesByCategoryItemDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Percentage { get; set; }
    }

    public sealed class DashboardRecentSaleItemDto
    {
        public int SaleId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public sealed class DashboardInventoryAlertItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public string Severity { get; set; } = string.Empty;
    }

    public sealed class DashboardStockAlertsCountDto
    {
        public int TotalAlerts { get; set; }
        public int CriticalAlerts { get; set; }
    }

    public sealed class DashboardRecentKardexMovementItemDto
    {
        public int KardexId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public DateTime Date { get; set; }
    }
}
