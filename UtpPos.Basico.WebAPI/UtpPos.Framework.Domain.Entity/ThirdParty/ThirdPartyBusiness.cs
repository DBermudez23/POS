namespace UtpPos.Framework.Domain.Entity.ThirdParty
{
    public sealed class ThirdPartyBusiness
    {
        public int Id { get; set; }
        public int? NegterId { get; set; }
        public int BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string RelationStatus { get; set; } = string.Empty;
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string Phone { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ThirdPartyTypeId { get; set; }
        public string ThirdPartyTypeName { get; set; } = string.Empty;
    }
}
