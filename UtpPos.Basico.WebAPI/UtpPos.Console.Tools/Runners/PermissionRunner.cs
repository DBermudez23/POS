using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.Interface.Rbac;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Runners
{
    public class PermissionRunner
    {
        private readonly IRbacApplication _application;

        public PermissionRunner(IRbacApplication application)
        {
            _application = application;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                ConsoleIO.WriteLine("[1] Listar permisos existentes");
                ConsoleIO.WriteLine("[2] Crear permiso seleccionando módulo y acción");
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
            var response = await _application.ListPermissionsAsync(new RbacPermissionFilterDto());
            if (!response.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            var permissions = response.Data?.ToList() ?? new List<RbacPermissionDto>();
            if (permissions.Count == 0)
            {
                ConsoleIO.WriteLine("No existen permisos registrados.");
                return;
            }

            ConsoleIO.WriteLine(
                $"{"PER_ID",-8} {"PER_NOMBRE",-34} {"MOD_CODIGO",-22} {"ACC_CODIGO",-22} " +
                $"{"PER_DESCRIPCION",-42} {"PER_ESTADO",-10}");
            foreach (var permission in permissions)
            {
                ConsoleIO.WriteLine(
                    $"{permission.Id,-8} {Truncate(permission.PermissionName, 34),-34} " +
                    $"{Truncate(permission.ModuleCode, 22),-22} {Truncate(permission.ActionCode, 22),-22} " +
                    $"{Truncate(permission.Description, 42),-42} {permission.Status,-10}");
            }
        }

        private async Task CreateAsync()
        {
            var modulesResponse = await _application.ListModulesAsync(new RbacModuleFilterDto { Status = "A" });
            if (!modulesResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al consultar módulos: {modulesResponse.Message}");
                return;
            }

            var modules = modulesResponse.Data?.ToList() ?? new List<RbacModuleDto>();
            if (modules.Count == 0)
            {
                ConsoleIO.WriteLine("No existen módulos activos. Cree o active un módulo antes de continuar.");
                return;
            }

            var actionsResponse = await _application.ListActionsAsync(new RbacActionFilterDto { Status = "A" });
            if (!actionsResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al consultar acciones: {actionsResponse.Message}");
                return;
            }

            var actions = actionsResponse.Data?.ToList() ?? new List<RbacActionDto>();
            if (actions.Count == 0)
            {
                ConsoleIO.WriteLine("No existen acciones activas. Cree o active una acción antes de continuar.");
                return;
            }

            ConsoleIO.WriteLine("Módulos activos:");
            for (var index = 0; index < modules.Count; index++)
                ConsoleIO.WriteLine($"[{index + 1}] {modules[index].Code} - {modules[index].Name}");

            var selectedModule = modules[ReadSelection("Seleccione un módulo: ", modules.Count) - 1];
            ConsoleIO.WriteLine();
            ConsoleIO.WriteLine("Acciones activas:");
            for (var index = 0; index < actions.Count; index++)
                ConsoleIO.WriteLine($"[{index + 1}] {actions[index].Code} - {actions[index].Name}");

            var selectedAction = actions[ReadSelection("Seleccione una acción: ", actions.Count) - 1];
            var permissionName = $"{selectedModule.Code}.{selectedAction.Code}";

            var existingResponse = await _application.ListPermissionsAsync(new RbacPermissionFilterDto
            {
                ModuleCode = selectedModule.Code,
                ActionCode = selectedAction.Code,
                Status = null
            });

            if (!existingResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al validar el permiso: {existingResponse.Message}");
                return;
            }

            if (existingResponse.Data?.Any(permission =>
                    string.Equals(permission.ModuleCode, selectedModule.Code, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(permission.ActionCode, selectedAction.Code, StringComparison.OrdinalIgnoreCase)) == true)
            {
                ConsoleIO.WriteLine($"El permiso [{permissionName}] ya existe. No se insertará nuevamente.");
                return;
            }

            ConsoleIO.Write("Descripción opcional: ");
            var description = EmptyToNull(ConsoleIO.ReadLine()) ?? $"Permiso para {permissionName}";

            if (!Confirm($"¿Crear el permiso [{permissionName}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _application.CreatePermissionAsync(new RbacPermissionCreateDto
            {
                ModuleId = selectedModule.Id,
                ModuleCode = selectedModule.Code,
                ActionId = selectedAction.Id,
                ActionCode = selectedAction.Code,
                Description = description,
                Status = "A"
            });

            if (!response.IsSuccess || response.Data is null)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            ConsoleIO.WriteLine(response.Message);
            ConsoleIO.WriteLine($"PER_ID: {response.Data.PermissionId}");
            ConsoleIO.WriteLine($"PER_NOMBRE: {response.Data.PermissionName}");
        }

        private static int ReadSelection(string prompt, int maximum)
        {
            while (true)
            {
                ConsoleIO.Write(prompt);
                if (int.TryParse(ConsoleIO.ReadLine(), out var selection) && selection >= 1 && selection <= maximum)
                    return selection;

                ConsoleIO.WriteLine($"Seleccione un número entre 1 y {maximum}.");
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
