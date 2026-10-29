namespace UtpPos.Framework.Transversal.Common.Auth
{
    public interface IRefreshTokenHashService
    {
        string Hash(string refreshToken);
    }
}
