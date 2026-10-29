namespace UtpPos.Framework.Transversal.Common.Auth
{
    public class RefreshTokenResult
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
