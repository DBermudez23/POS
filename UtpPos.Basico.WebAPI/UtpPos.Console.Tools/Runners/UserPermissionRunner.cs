using UtpPos.Console.Tools.Helpers;
using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.Interface.Rbac;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Runners
{
    public class UserPermissionRunner
    {
        private readonly IRbacApplication _application;

        public UserPermissionRunner(IRbacApplication application)
        {
            _application = application;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                ConsoleIO.WriteLine("==== Permisos Efectivos de Usuario ====");
                ConsoleIO.WriteLine();
                ConsoleIO.WriteLine("[1] Consultar permisos efectivos por usuario");
                ConsoleIO.WriteLine("[0] Volver");
                ConsoleIO.Write("Seleccione una opción: ");

                var option = ConsoleIO.ReadLine()?.Trim();
                ConsoleIO.WriteLine();

                switch (option)
                {
                    case "1":
                        await ListAsync();
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
            var userId = ConsoleInput.ReadPositiveInt("USR_ID: ");
            var response = await _application.GetUserPermissionsAsync(userId);
            if (!response.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            var permissions = response.Data?.ToList() ?? new List<RbacUserPermissionDto>();
            if (permissions.Count == 0)
            {
                ConsoleIO.WriteLine("El usuario no tiene permisos efectivos activos.");
                return;
            }

            ConsoleIO.WriteLine(
                $"{"USR_ID",-7} {"ACCESO_TOTAL",-13} {"PER_ID",-7} {"PERMISO",-34} " +
                $"{"MÓDULO",-20} {"ACCIÓN",-20} {"ESTADO",-7}");

            foreach (var permission in permissions)
            {
                ConsoleIO.WriteLine(
                    $"{permission.UserId,-7} {permission.UserAccessTotal,-13} {permission.PermissionId,-7} " +
                    $"{ConsoleInput.Truncate(permission.PermissionName, 34),-34} " +
                    $"{ConsoleInput.Truncate(permission.ModuleCode, 20),-20} " +
                    $"{ConsoleInput.Truncate(permission.ActionCode, 20),-20} {permission.PermissionStatus,-7}");
            }

            ConsoleIO.WriteLine();
            ConsoleIO.WriteLine("Estos son los permisos evaluados por [RequirePermission(\"módulo\", \"acción\")].");
        }
    }
}
