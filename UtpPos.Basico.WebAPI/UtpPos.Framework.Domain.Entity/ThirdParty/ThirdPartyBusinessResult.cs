namespace UtpPos.Framework.Domain.Entity.ThirdParty
{
    public sealed class ThirdPartyBusinessResult
    {
        public int ThirdPartyId { get; set; }
        public int? NegterId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
