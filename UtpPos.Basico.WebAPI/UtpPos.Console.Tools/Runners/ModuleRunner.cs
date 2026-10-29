using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.Interface.Rbac;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Runners
{
    public class ModuleRunner
    {
        private readonly IRbacApplication _application;

        public ModuleRunner(IRbacApplication application)
        {
            _application = application;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                ConsoleIO.WriteLine("[1] Listar módulos existentes");
                ConsoleIO.WriteLine("[2] Crear módulo");
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
            var response = await _application.ListModulesAsync(new RbacModuleFilterDto());
            if (!response.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            var modules = response.Data?.ToList() ?? new List<RbacModuleDto>();
            if (modules.Count == 0)
            {
                ConsoleIO.WriteLine("No existen módulos registrados.");
                return;
            }

            ConsoleIO.WriteLine($"{"MOD_ID",-8} {"MOD_CODIGO",-24} {"MOD_NOMBRE",-28} {"MOD_DESCRIPCION",-42} {"MOD_ESTADO",-10}");
            foreach (var module in modules)
            {
                ConsoleIO.WriteLine(
                    $"{module.Id,-8} {Truncate(module.Code, 24),-24} {Truncate(module.Name, 28),-28} " +
                    $"{Truncate(module.Description, 42),-42} {module.Status,-10}");
            }
        }

        private async Task CreateAsync()
        {
            var code = ReadRequired("Código técnico del módulo: ").ToLowerInvariant();

            var existingResponse = await _application.ListModulesAsync(new RbacModuleFilterDto
            {
                Code = code,
                Status = null
            });

            if (!existingResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al validar el módulo: {existingResponse.Message}");
                return;
            }

            if (existingResponse.Data?.Any(module =>
                    string.Equals(module.Code, code, StringComparison.OrdinalIgnoreCase)) == true)
            {
                ConsoleIO.WriteLine($"El módulo [{code}] ya existe. No se insertará nuevamente.");
                return;
            }

            var name = ReadRequired("Nombre visible del módulo: ");
            ConsoleIO.Write("Descripción opcional: ");
            var description = EmptyToNull(ConsoleIO.ReadLine());

            if (!Confirm($"¿Crear el módulo [{code}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _application.CreateModuleAsync(new RbacModuleCreateDto
            {
                Code = code,
                Name = name,
                Description = description,
                IsSystem = "S",
                Status = "A"
            });

            if (!response.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            ConsoleIO.WriteLine($"{response.Message} MOD_ID: {response.Data}");
        }

        private static string ReadRequired(string prompt)
        {
            while (true)
            {
                ConsoleIO.Write(prompt);
                var value = ConsoleIO.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;

                ConsoleIO.WriteLine("El valor es obligatorio.");
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

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string Truncate(string? value, int length)
        {
            var normalized = value ?? string.Empty;
            return normalized.Length <= length ? normalized : normalized[..(length - 3)] + "...";
        }
    }
}
