namespace UtpPos.Framework.Transversal.Common.Auth
{
    public class AccessTokenResult
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public int ExpiresIn { get; set; }
    }
}
