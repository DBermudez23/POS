namespace UtpPos.Framework.Shared.Dashboard
{
    public sealed class DashboardOverviewFilterDto
    {
        public int? BusinessId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Top { get; set; }
    }
}
