using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.Interface.Rbac;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Runners
{
    public class ActionRunner
    {
        private readonly IRbacApplication _application;

        public ActionRunner(IRbacApplication application)
        {
            _application = application;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                ConsoleIO.WriteLine("[1] Listar acciones existentes");
                ConsoleIO.WriteLine("[2] Crear acción");
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
            var response = await _application.ListActionsAsync(new RbacActionFilterDto());
            if (!response.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            var actions = response.Data?.ToList() ?? new List<RbacActionDto>();
            if (actions.Count == 0)
            {
                ConsoleIO.WriteLine("No existen acciones registradas.");
                return;
            }

            ConsoleIO.WriteLine($"{"ACC_ID",-8} {"ACC_CODIGO",-24} {"ACC_NOMBRE",-28} {"ACC_DESCRIPCION",-42} {"ACC_ESTADO",-10}");
            foreach (var action in actions)
            {
                ConsoleIO.WriteLine(
                    $"{action.Id,-8} {Truncate(action.Code, 24),-24} {Truncate(action.Name, 28),-28} " +
                    $"{Truncate(action.Description, 42),-42} {action.Status,-10}");
            }
        }

        private async Task CreateAsync()
        {
            var code = ReadRequired("Código técnico de la acción: ").ToLowerInvariant();

            var existingResponse = await _application.ListActionsAsync(new RbacActionFilterDto
            {
                Code = code,
                Status = null
            });

            if (!existingResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al validar la acción: {existingResponse.Message}");
                return;
            }

            if (existingResponse.Data?.Any(action =>
                    string.Equals(action.Code, code, StringComparison.OrdinalIgnoreCase)) == true)
            {
                ConsoleIO.WriteLine($"La acción [{code}] ya existe. No se insertará nuevamente.");
                return;
            }

            var name = ReadRequired("Nombre visible de la acción: ");
            ConsoleIO.Write("Descripción opcional: ");
            var description = EmptyToNull(ConsoleIO.ReadLine());

            if (!Confirm($"¿Crear la acción [{code}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _application.CreateActionAsync(new RbacActionCreateDto
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

            ConsoleIO.WriteLine($"{response.Message} ACC_ID: {response.Data}");
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
