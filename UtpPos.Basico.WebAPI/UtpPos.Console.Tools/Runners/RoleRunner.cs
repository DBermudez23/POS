using UtpPos.Console.Tools.Helpers;
using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.Interface.Rbac;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Runners
{
    public class RoleRunner
    {
        private readonly IRbacApplication _application;

        public RoleRunner(IRbacApplication application)
        {
            _application = application;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                ConsoleIO.WriteLine("==== Gestión de Roles ====");
                ConsoleIO.WriteLine();
                ConsoleIO.WriteLine("[1] Listar roles");
                ConsoleIO.WriteLine("[2] Crear rol");
                ConsoleIO.WriteLine("[3] Actualizar rol");
                ConsoleIO.WriteLine("[4] Inactivar rol");
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
                    case "3":
                        await UpdateAsync();
                        break;
                    case "4":
                        await DeactivateAsync();
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
            var response = await _application.GetRolesAsync(new RbacRoleFilterDto());
            if (!response.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            var roles = response.Data?.ToList() ?? new List<RbacRoleDto>();
            if (roles.Count == 0)
            {
                ConsoleIO.WriteLine("No existen roles registrados.");
                return;
            }

            PrintRoles(roles);
        }

        private async Task CreateAsync()
        {
            var request = new RbacRoleCreateDto
            {
                BusinessId = ConsoleInput.ReadOptionalPositiveInt("Business ID / NEG_ID (vacío = rol global): "),
                Name = ConsoleInput.ReadRequired("Nombre del rol: ", 80),
                Description = ConsoleInput.ReadOptional("Descripción opcional: ", 250),
                IsSystem = ConsoleInput.ReadFlag("¿Es rol de sistema? [N]: ", "N"),
                FullAccess = ConsoleInput.ReadFlag("¿Tiene acceso total? [N]: ", "N"),
                Status = ConsoleInput.ReadStatus("Estado [A]: ")
            };

            if (!ConsoleInput.Confirm($"¿Crear el rol [{request.Name}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _application.CreateRoleAsync(request);
            if (!response.IsSuccess || response.Data is null)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            ConsoleIO.WriteLine($"{response.Message} ROL_ID: {response.Data.RoleId}");
        }

        private async Task UpdateAsync()
        {
            var roleId = ConsoleInput.ReadPositiveInt("ROL_ID a actualizar: ");
            var currentResponse = await _application.GetRoleByIdAsync(roleId);
            if (!currentResponse.IsSuccess || currentResponse.Data is null)
            {
                ConsoleIO.WriteLine($"Error: {currentResponse.Message}");
                return;
            }

            var current = currentResponse.Data;
            PrintRoles(new[] { current });
            ConsoleIO.WriteLine();
            ConsoleIO.WriteLine("Enter conserva el valor actual. Use 0 para convertir el rol en global y '-' para borrar la descripción.");

            var request = new RbacRoleUpdateDto
            {
                RoleId = current.Id,
                BusinessId = ReadBusinessIdForUpdate(current.BusinessId),
                Name = ReadRequiredForUpdate("Nombre", current.Name, 80),
                Description = ReadDescriptionForUpdate(current.Description),
                IsSystem = ReadFlagForUpdate("Es sistema", current.IsSystem),
                FullAccess = ReadFlagForUpdate("Acceso total", current.FullAccess),
                Status = ReadStatusForUpdate(current.Status)
            };

            if (!ConsoleInput.Confirm($"¿Actualizar el rol [{current.Name}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _application.UpdateRoleAsync(request);
            ConsoleIO.WriteLine(response.IsSuccess ? response.Message : $"Error: {response.Message}");
        }

        private async Task DeactivateAsync()
        {
            var roleId = ConsoleInput.ReadPositiveInt("ROL_ID a inactivar: ");
            var currentResponse = await _application.GetRoleByIdAsync(roleId);
            if (!currentResponse.IsSuccess || currentResponse.Data is null)
            {
                ConsoleIO.WriteLine($"Error: {currentResponse.Message}");
                return;
            }

            var role = currentResponse.Data;
            ConsoleIO.WriteLine($"Rol: {role.Name} | Sistema: {role.IsSystem} | Estado: {role.Status}");
            ConsoleIO.WriteLine("Los roles de sistema pueden estar protegidos por el stored procedure.");

            if (!ConsoleInput.Confirm($"¿Inactivar el rol [{role.Name}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _application.DeactivateRoleAsync(role.Id);
            ConsoleIO.WriteLine(response.IsSuccess ? response.Message : $"Error: {response.Message}");
        }

        private static int? ReadBusinessIdForUpdate(int? current)
        {
            while (true)
            {
                ConsoleIO.Write($"Business ID [{ConsoleInput.FormatNullable(current)}]: ");
                var input = ConsoleIO.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input))
                    return current;
                if (input == "0")
                    return null;
                if (int.TryParse(input, out var value) && value > 0)
                    return value;
                ConsoleIO.WriteLine("Ingrese un entero mayor que cero, 0 para global o Enter para conservar.");
            }
        }

        private static string ReadRequiredForUpdate(string field, string current, int maximumLength)
        {
            while (true)
            {
                ConsoleIO.Write($"{field} [{current}]: ");
                var input = ConsoleIO.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input))
                    return current;
                if (input.Length <= maximumLength)
                    return input;
                ConsoleIO.WriteLine($"El valor no puede superar {maximumLength} caracteres.");
            }
        }

        private static string? ReadDescriptionForUpdate(string? current)
        {
            while (true)
            {
                ConsoleIO.Write($"Descripción [{current ?? "NULL"}]: ");
                var input = ConsoleIO.ReadLine();
                if (string.IsNullOrEmpty(input))
                    return current;
                if (input.Trim() == "-")
                    return null;
                if (input.Trim().Length <= 250)
                    return input.Trim();
                ConsoleIO.WriteLine("La descripción no puede superar 250 caracteres.");
            }
        }

        private static string ReadFlagForUpdate(string field, string current)
        {
            return ConsoleInput.ReadFlag($"{field} [{current}]: ", current);
        }

        private static string ReadStatusForUpdate(string current)
        {
            return ConsoleInput.ReadStatus($"Estado [{current}]: ", current);
        }

        private static void PrintRoles(IEnumerable<RbacRoleDto> roles)
        {
            ConsoleIO.WriteLine(
                $"{"ROL_ID",-7} {"NEG_ID",-7} {"NEGOCIO",-20} {"NOMBRE",-22} {"DESCRIPCIÓN",-30} " +
                $"{"SISTEMA",-8} {"TOTAL",-6} {"ESTADO",-7} {"CREADO",-19}");

            foreach (var role in roles)
            {
                ConsoleIO.WriteLine(
                    $"{role.Id,-7} {ConsoleInput.FormatNullable(role.BusinessId),-7} " +
                    $"{ConsoleInput.Truncate(role.BusinessName, 20),-20} {ConsoleInput.Truncate(role.Name, 22),-22} " +
                    $"{ConsoleInput.Truncate(role.Description, 30),-30} {role.IsSystem,-8} {role.FullAccess,-6} " +
                    $"{role.Status,-7} {role.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            }
        }
    }
}
