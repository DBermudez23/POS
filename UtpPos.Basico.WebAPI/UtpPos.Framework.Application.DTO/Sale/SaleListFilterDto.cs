namespace UtpPos.Framework.Application.DTO.Sale
{
    public sealed class SaleListFilterDto
    {
        public int? BusinessId { get; set; }
        public int? ThirdPartyId { get; set; }
        public int? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
