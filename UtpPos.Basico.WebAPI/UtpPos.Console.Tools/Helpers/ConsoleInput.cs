using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Helpers
{
    public static class ConsoleInput
    {
        public static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                ConsoleIO.Write(prompt);
                if (int.TryParse(ConsoleIO.ReadLine()?.Trim(), out var value) && value > 0)
                    return value;

                ConsoleIO.WriteLine("Ingrese un número entero mayor que cero.");
            }
        }

        public static int? ReadOptionalPositiveInt(string prompt)
        {
            while (true)
            {
                ConsoleIO.Write(prompt);
                var input = ConsoleIO.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input))
                    return null;

                if (int.TryParse(input, out var value) && value > 0)
                    return value;

                ConsoleIO.WriteLine("Ingrese un número entero mayor que cero o deje el valor vacío.");
            }
        }

        public static string ReadRequired(string prompt, int maximumLength)
        {
            while (true)
            {
                ConsoleIO.Write(prompt);
                var value = ConsoleIO.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    ConsoleIO.WriteLine("El valor es obligatorio.");
                    continue;
                }

                if (value.Length <= maximumLength)
                    return value;

                ConsoleIO.WriteLine($"El valor no puede superar {maximumLength} caracteres.");
            }
        }

        public static string? ReadOptional(string prompt, int maximumLength)
        {
            while (true)
            {
                ConsoleIO.Write(prompt);
                var value = EmptyToNull(ConsoleIO.ReadLine());
                if (value?.Length <= maximumLength || value is null)
                    return value;

                ConsoleIO.WriteLine($"El valor no puede superar {maximumLength} caracteres.");
            }
        }

        public static string ReadFlag(string prompt, string defaultValue)
        {
            return ReadOption(prompt, defaultValue, "S", "N", "El valor debe ser S o N.");
        }

        public static string ReadStatus(string prompt, string defaultValue = "A")
        {
            return ReadOption(prompt, defaultValue, "A", "I", "El estado debe ser A o I.");
        }

        public static int ReadSelection(string prompt, int maximum)
        {
            while (true)
            {
                ConsoleIO.Write(prompt);
                if (int.TryParse(ConsoleIO.ReadLine()?.Trim(), out var selection) &&
                    selection >= 1 && selection <= maximum)
                {
                    return selection;
                }

                ConsoleIO.WriteLine($"Seleccione un número entre 1 y {maximum}.");
            }
        }

        public static bool Confirm(string prompt)
        {
            ConsoleIO.Write(prompt);
            var value = ConsoleIO.ReadLine()?.Trim();
            return string.Equals(value, "S", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "SI", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "SÍ", StringComparison.OrdinalIgnoreCase);
        }

        public static string Truncate(string? value, int length)
        {
            var normalized = value ?? string.Empty;
            return normalized.Length <= length ? normalized : normalized[..(length - 3)] + "...";
        }

        public static string FormatNullable(int? value) => value?.ToString() ?? "NULL";

        public static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string ReadOption(
            string prompt,
            string defaultValue,
            string firstValue,
            string secondValue,
            string errorMessage)
        {
            while (true)
            {
                ConsoleIO.Write(prompt);
                var value = ConsoleIO.ReadLine()?.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(value))
                    return defaultValue;

                if (value == firstValue || value == secondValue)
                    return value;

                ConsoleIO.WriteLine(errorMessage);
            }
        }
    }
}
