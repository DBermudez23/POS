namespace UtpPos.Framework.Application.DTO.ThirdParty
{
    public sealed class ThirdPartyBusinessListFilterDto
    {
        public int? DocumentTypeId { get; set; }
        public string? Status { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }
}
