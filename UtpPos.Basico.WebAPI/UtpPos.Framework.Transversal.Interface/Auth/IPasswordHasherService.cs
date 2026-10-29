namespace UtpPos.Framework.Transversal.Common.Auth
{
    public interface IPasswordHasherService
    {
        string Hash(string password);
        bool IsValidHash(string passwordHash);
        bool Verify(string password, string passwordHash);
    }
}
