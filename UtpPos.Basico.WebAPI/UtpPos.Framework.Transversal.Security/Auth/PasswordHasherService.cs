using System.Text;
using System.Text.RegularExpressions;

namespace UtpPos.Framework.Transversal.Common.Auth
{
    public class PasswordHasherService : IPasswordHasherService
    {
        private const int WorkFactor = 12;
        private const int MaximumPasswordBytes = 72;
        private static readonly Regex BcryptHashPattern = new(
            @"^\$2[abxy]\$\d{2}\$[./A-Za-z0-9]{53}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña es obligatoria.", nameof(password));

            if (Encoding.UTF8.GetByteCount(password) > MaximumPasswordBytes)
                throw new ArgumentException("La contraseña no puede superar 72 bytes UTF-8.", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool IsValidHash(string passwordHash)
        {
            return !string.IsNullOrWhiteSpace(passwordHash) &&
                   BcryptHashPattern.IsMatch(passwordHash);
        }

        public bool Verify(string password, string passwordHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
