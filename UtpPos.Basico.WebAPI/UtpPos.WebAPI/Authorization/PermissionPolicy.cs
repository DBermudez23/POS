namespace UtpPos.Framework.Services.WebApi.Authorization
{
    internal static class PermissionPolicy
    {
        public const string Prefix = "Permission:";

        public static string BuildPolicyName(string permission)
        {
            return Prefix + NormalizePermission(permission);
        }

        public static string BuildPolicyName(string module, string action)
        {
            return BuildPolicyName($"{NormalizeCode(module, nameof(module))}.{NormalizeCode(action, nameof(action))}");
        }

        public static bool TryGetPermission(string policyName, out string permission)
        {
            permission = string.Empty;
            if (string.IsNullOrWhiteSpace(policyName) ||
                !policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                permission = NormalizePermission(policyName[Prefix.Length..]);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static string NormalizePermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("El permiso es obligatorio.", nameof(permission));

            var parts = permission.Trim().ToLowerInvariant().Split('.', StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException("El permiso debe tener el formato modulo.accion.", nameof(permission));

            return $"{NormalizeCode(parts[0], nameof(permission))}.{NormalizeCode(parts[1], nameof(permission))}";
        }

        private static string NormalizeCode(string value, string parameterName)
        {
            var normalized = value?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized) ||
                !normalized.All(character =>
                    character is >= 'a' and <= 'z' ||
                    character is >= '0' and <= '9' ||
                    character is '_' or '-'))
            {
                throw new ArgumentException(
                    "El módulo y la acción solo admiten letras, números, guion bajo o guion medio.",
                    parameterName);
            }

            return normalized;
        }
    }
}
