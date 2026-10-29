using UtpPos.Console.Tools.Runners;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Menus
{
    public class MainMenu
    {
        private readonly ModuleRunner _moduleRunner;
        private readonly ActionRunner _actionRunner;
        private readonly PermissionRunner _permissionRunner;
        private readonly UserRunner _userRunner;
        private readonly RoleRunner _roleRunner;
        private readonly RolePermissionRunner _rolePermissionRunner;
        private readonly UserRoleRunner _userRoleRunner;
        private readonly UserPermissionRunner _userPermissionRunner;

        public MainMenu(
            ModuleRunner moduleRunner,
            ActionRunner actionRunner,
            PermissionRunner permissionRunner,
            UserRunner userRunner,
            RoleRunner roleRunner,
            RolePermissionRunner rolePermissionRunner,
            UserRoleRunner userRoleRunner,
            UserPermissionRunner userPermissionRunner)
        {
            _moduleRunner = moduleRunner;
            _actionRunner = actionRunner;
            _permissionRunner = permissionRunner;
            _userRunner = userRunner;
            _roleRunner = roleRunner;
            _rolePermissionRunner = rolePermissionRunner;
            _userRoleRunner = userRoleRunner;
            _userPermissionRunner = userPermissionRunner;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                ConsoleIO.WriteLine("==============================================");
                ConsoleIO.WriteLine("             UTP POS CONSOLE TOOLS");
                ConsoleIO.WriteLine("==============================================");
                ConsoleIO.WriteLine("[1] Gestionar Módulos");
                ConsoleIO.WriteLine("[2] Gestionar Acciones");
                ConsoleIO.WriteLine("[3] Gestionar Permisos (Módulo + Acción)");
                ConsoleIO.WriteLine("[4] Gestionar Usuarios");
                ConsoleIO.WriteLine("[5] Gestionar Roles");
                ConsoleIO.WriteLine("[6] Gestionar Permisos por Rol");
                ConsoleIO.WriteLine("[7] Gestionar Roles por Usuario");
                ConsoleIO.WriteLine("[8] Consultar Permisos Efectivos de Usuario");
                ConsoleIO.WriteLine("[0] Salir");
                ConsoleIO.WriteLine("==============================================");
                ConsoleIO.Write("Seleccione una opción: ");

                var option = ConsoleIO.ReadLine()?.Trim();
                ConsoleIO.WriteLine();

                switch (option)
                {
                    case "1":
                        await _moduleRunner.RunAsync();
                        break;
                    case "2":
                        await _actionRunner.RunAsync();
                        break;
                    case "3":
                        await _permissionRunner.RunAsync();
                        break;
                    case "4":
                        await _userRunner.RunAsync();
                        break;
                    case "5":
                        await _roleRunner.RunAsync();
                        break;
                    case "6":
                        await _rolePermissionRunner.RunAsync();
                        break;
                    case "7":
                        await _userRoleRunner.RunAsync();
                        break;
                    case "8":
                        await _userPermissionRunner.RunAsync();
                        break;
                    case "0":
                    case null:
                        return;
                    default:
                        ConsoleIO.WriteLine("Opción no válida.");
                        ConsoleIO.WriteLine();
                        break;
                }
            }
        }
    }
}
