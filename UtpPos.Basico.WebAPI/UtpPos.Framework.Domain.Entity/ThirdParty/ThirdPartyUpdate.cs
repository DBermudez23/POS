namespace UtpPos.Framework.Domain.Entity.ThirdParty
{
    public sealed class ThirdPartyUpdate
    {
        public int Id { get; set; }
        public int? DocumentTypeId { get; set; }
        public string? DocumentNumber { get; set; }
        public string? Name { get; set; }
        public int? DepartmentId { get; set; }
        public int? CityId { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }
        public int? ThirdPartyTypeId { get; set; }
        public bool ClearAddress { get; set; }
    }
}
