namespace UtpPos.Framework.Transversal.Common.Auth
{
    public interface IRefreshTokenService
    {
        RefreshTokenResult GenerateToken();
    }
}
