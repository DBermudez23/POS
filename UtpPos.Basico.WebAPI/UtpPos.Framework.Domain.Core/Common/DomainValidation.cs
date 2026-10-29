namespace UtpPos.Framework.Domain.Core.Common
{
    public static class DomainValidation
    {
        public static void EnsurePositiveId(int id, string fieldName = "Id")
        {
            if (id <= 0)
                throw new ArgumentException($"{fieldName} must be greater than zero.");
        }

        public static void EnsureRequired(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{fieldName} is required.");
        }

        public static string? NormalizeCode(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
        }

        public static string? NormalizeStatus(string? status)
        {
            return NormalizeCode(status);
        }

        public static void EnsureActiveInactiveStatus(string? status, bool required = false)
        {
            EnsureAllowedCode(status, "Status", required, "A", "I");
        }

        public static void EnsureAllowedCode(string? value, string fieldName, bool required, params string[] allowedValues)
        {
            var normalizedValue = NormalizeCode(value);

            if (string.IsNullOrWhiteSpace(normalizedValue))
            {
                if (required)
                    throw new ArgumentException($"{fieldName} is required.");

                return;
            }

            var normalizedAllowedValues = allowedValues.Select(NormalizeCode);
            if (!normalizedAllowedValues.Contains(normalizedValue))
                throw new ArgumentException($"{fieldName} must be {string.Join(" or ", allowedValues)}.");
        }

        public static void EnsurePositiveAmount(decimal value, string fieldName)
        {
            if (value <= 0)
                throw new ArgumentException($"{fieldName} must be greater than zero.");
        }

        public static void EnsureNonNegativeAmount(decimal value, string fieldName)
        {
            if (value < 0)
                throw new ArgumentException($"{fieldName} cannot be negative.");
        }

        public static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
