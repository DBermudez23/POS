using System.Net.Mail;
using System.Text;
using UtpPos.Console.Tools.Helpers;
using UtpPos.Framework.Application.DTO.User;
using UtpPos.Framework.Application.Interface.User;
using UtpPos.Framework.Transversal.Common.Auth;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Runners
{
    public class UserRunner
    {
        private const int MaximumBcryptPasswordBytes = 72;
        private readonly IUserApplication _application;
        private readonly IPasswordHasherService _passwordHasherService;

        public UserRunner(IUserApplication application, IPasswordHasherService passwordHasherService)
        {
            _application = application;
            _passwordHasherService = passwordHasherService;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                ConsoleIO.WriteLine("==== Gestión de Usuarios ====");
                ConsoleIO.WriteLine();
                ConsoleIO.WriteLine("[1] Listar usuarios");
                ConsoleIO.WriteLine("[2] Crear usuario");
                ConsoleIO.WriteLine("[0] Volver");
                ConsoleIO.Write("Seleccione una opción: ");
                var option = ConsoleIO.ReadLine()?.Trim();
                ConsoleIO.WriteLine();

                switch (option)
                {
                    case "1":
                        await ListAsync();
                        break;
                    case "2":
                        await CreateAsync();
                        break;
                    case "0":
                    case null:
                        return;
                    default:
                        ConsoleIO.WriteLine("Opción no válida.");
                        break;
                }

                ConsoleIO.WriteLine();
            }
        }

        private async Task ListAsync()
        {
            var response = await _application.ListAsync(new UserListFilterDto());
            if (!response.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            var users = response.Data?.ToList() ?? new List<UserDto>();
            if (users.Count == 0)
            {
                ConsoleIO.WriteLine("No existen usuarios registrados.");
                return;
            }

            ConsoleIO.WriteLine(
                $"{"USR_ID",-8} {"NEG_ID",-8} {"TIPO_DOC",-22} {"DOCUMENTO",-20} " +
                $"{"NOMBRE",-28} {"CORREO",-32} {"ESTADO",-8}");
            foreach (var user in users)
            {
                ConsoleIO.WriteLine(
                    $"{user.Id,-8} {FormatBusinessId(user.BusinessId),-8} " +
                    $"{Truncate(user.DocumentType, 22),-22} {Truncate(user.DocumentNumber, 20),-20} " +
                    $"{Truncate(user.Name, 28),-28} {Truncate(user.Email, 32),-32} {user.Status,-8}");
            }
        }

        private async Task CreateAsync()
        {
            var businessId = ReadOptionalPositiveInt("Business ID / NEG_ID (opcional): ");
            var documentTypeId = ReadPositiveInt("Document Type ID / USR_TIPO_DOC_ID: ");
            var documentNumber = ReadRequired("Document Number / USR_NUMERO_DOCUMENTO: ", 20);
            var name = ReadRequired("Name / USR_NOMBRE: ", 100);
            var email = ReadEmail();

            string password;
            string confirmation;
            try
            {
                (password, confirmation) = ReadAndConfirmPassword();
            }
            catch (InvalidOperationException ex)
            {
                ConsoleIO.WriteLine($"Error: {ex.Message}");
                return;
            }

            ConsoleIO.Write("Phone / USR_TELEFONO (opcional): ");
            var phone = ReadOptional(ConsoleIO.ReadLine(), 20, "El teléfono");
            var status = ReadStatus();

            ConsoleIO.WriteLine();
            ConsoleIO.WriteLine("Datos del usuario:");
            ConsoleIO.WriteLine($"NEG_ID: {businessId?.ToString() ?? "NULL"}");
            ConsoleIO.WriteLine($"USR_TIPO_DOC_ID: {documentTypeId}");
            ConsoleIO.WriteLine($"USR_NUMERO_DOCUMENTO: {documentNumber}");
            ConsoleIO.WriteLine($"USR_NOMBRE: {name}");
            ConsoleIO.WriteLine($"USR_CORREO: {email}");
            ConsoleIO.WriteLine($"USR_TELEFONO: {phone ?? "NULL"}");
            ConsoleIO.WriteLine($"USR_ESTADO: {status}");

            if (!Confirm("¿Crear este usuario? [S/N]: "))
            {
                password = string.Empty;
                confirmation = string.Empty;
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            string passwordHash;
            try
            {
                passwordHash = _passwordHasherService.Hash(password);
            }
            catch (Exception)
            {
                ConsoleIO.WriteLine("No fue posible procesar la contraseña.");
                return;
            }
            finally
            {
                password = string.Empty;
                confirmation = string.Empty;
            }

            var request = new UserCreateDto
            {
                BusinessId = businessId,
                DocumentTypeId = documentTypeId,
                DocumentNumber = documentNumber,
                Name = name,
                Email = email,
                PasswordHash = passwordHash,
                Phone = phone,
                Status = status
            };

            var response = await _application.CreateAsync(request);
            request.PasswordHash = string.Empty;
            passwordHash = string.Empty;

            if (!response.IsSuccess || response.Data is null)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            ConsoleIO.WriteLine(response.Message);
            ConsoleIO.WriteLine($"USR_ID: {response.Data.UserId}");
        }

        private static (string Password, string Confirmation) ReadAndConfirmPassword()
        {
            while (true)
            {
                var password = ConsolePasswordReader.Read("Password / USR_PASSWORD: ");
                if (password.Length < 6)
                {
                    password = string.Empty;
                    ConsoleIO.WriteLine("La contraseña debe tener al menos 6 caracteres.");
                    continue;
                }

                if (Encoding.UTF8.GetByteCount(password) > MaximumBcryptPasswordBytes)
                {
                    password = string.Empty;
                    ConsoleIO.WriteLine("La contraseña no puede superar 72 bytes UTF-8.");
                    continue;
                }

                var confirmation = ConsolePasswordReader.Read("Confirmar contraseña: ");
                if (string.Equals(password, confirmation, StringComparison.Ordinal))
                    return (password, confirmation);

                password = string.Empty;
                confirmation = string.Empty;
                ConsoleIO.WriteLine("Las contraseñas no coinciden.");
            }
        }

        private static string ReadEmail()
        {
            while (true)
            {
                var email = ReadRequired("Email / USR_CORREO: ", 80).ToLowerInvariant();
                if (MailAddress.TryCreate(email, out var parsedEmail) &&
                    string.Equals(parsedEmail.Address, email, StringComparison.OrdinalIgnoreCase))
                    return email;

                ConsoleIO.WriteLine("El formato del correo no es válido.");
            }
        }

        private static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                ConsoleIO.Write(prompt);
                if (int.TryParse(ConsoleIO.ReadLine(), out var value) && value > 0)
                    return value;

                ConsoleIO.WriteLine("Ingrese un número entero mayor que cero.");
            }
        }

        private static int? ReadOptionalPositiveInt(string prompt)
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

        private static string ReadRequired(string prompt, int maximumLength)
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

        private static string? ReadOptional(string? value, int maximumLength, string fieldName)
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            while (normalized?.Length > maximumLength)
            {
                ConsoleIO.WriteLine($"{fieldName} no puede superar {maximumLength} caracteres.");
                ConsoleIO.Write($"{fieldName} (opcional): ");
                var nextValue = ConsoleIO.ReadLine();
                normalized = string.IsNullOrWhiteSpace(nextValue) ? null : nextValue.Trim();
            }
            return normalized;
        }

        private static string ReadStatus()
        {
            while (true)
            {
                ConsoleIO.Write("Status / USR_ESTADO [A]: ");
                var status = ConsoleIO.ReadLine()?.Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(status))
                    return "A";
                if (status is "A" or "I")
                    return status;
                ConsoleIO.WriteLine("El estado debe ser A o I.");
            }
        }

        private static bool Confirm(string prompt)
        {
            ConsoleIO.Write(prompt);
            var value = ConsoleIO.ReadLine()?.Trim();
            return string.Equals(value, "S", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "SI", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "SÍ", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatBusinessId(int? businessId) => businessId?.ToString() ?? "NULL";

        private static string Truncate(string? value, int length)
        {
            var normalized = value ?? string.Empty;
            return normalized.Length <= length ? normalized : normalized[..(length - 3)] + "...";
        }
    }
}
