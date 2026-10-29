namespace UtpPos.Framework.Shared.ThirdParty
{
    public sealed class ThirdPartyCreateDto
    {
        public int DocumentTypeId { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public int CityId { get; set; }
        public string? Address { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = "A";
        public int ThirdPartyTypeId { get; set; }
    }
}
