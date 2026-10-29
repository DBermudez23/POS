namespace UtpPos.Framework.Application.DTO.Dashboard
{
    public sealed class DashboardBusinessResolutionDto
    {
        public bool IsSuccess { get; set; }
        public bool IsUnauthorized { get; set; }
        public bool IsForbidden { get; set; }
        public int BusinessId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
