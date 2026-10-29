namespace UtpPos.Framework.Domain.Entity.ThirdParty
{
    public sealed class ThirdPartyBusinessFilter
    {
        public int BusinessId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string? Status { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }
}
