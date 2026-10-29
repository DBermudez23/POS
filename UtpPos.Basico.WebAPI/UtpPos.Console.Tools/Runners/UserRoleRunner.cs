using UtpPos.Console.Tools.Helpers;
using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.DTO.User;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Application.Interface.User;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Runners
{
    public class UserRoleRunner
    {
        private readonly IRbacApplication _rbacApplication;
        private readonly IUserApplication _userApplication;

        public UserRoleRunner(IRbacApplication rbacApplication, IUserApplication userApplication)
        {
            _rbacApplication = rbacApplication;
            _userApplication = userApplication;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                ConsoleIO.WriteLine("==== Gestión de Roles por Usuario ====");
                ConsoleIO.WriteLine();
                ConsoleIO.WriteLine("[1] Listar roles de un usuario");
                ConsoleIO.WriteLine("[2] Asignar rol a usuario");
                ConsoleIO.WriteLine("[3] Revocar rol de usuario");
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
                        await AssignAsync();
                        break;
                    case "3":
                        await RevokeAsync();
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
            var user = await SelectUserAsync(activeOnly: false);
            if (user is null)
                return;

            var response = await _rbacApplication.GetUserRolesAsync(new RbacUserRoleFilterDto
            {
                UserId = user.Id
            });

            if (!response.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            var assignments = response.Data?.ToList() ?? new List<RbacUserRoleDto>();
            if (assignments.Count == 0)
            {
                ConsoleIO.WriteLine("El usuario no tiene asignaciones de roles.");
                return;
            }

            PrintAssignments(assignments);
        }

        private async Task AssignAsync()
        {
            var user = await SelectUserAsync(activeOnly: true);
            if (user is null)
                return;

            var role = await SelectRoleAsync();
            if (role is null)
                return;

            var assignedByUserId = ConsoleInput.ReadOptionalPositiveInt(
                "Usuario que realiza la asignación (USR_ID, opcional): ");

            if (!ConsoleInput.Confirm($"¿Asignar el rol [{role.Name}] a [{user.Name}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _rbacApplication.AssignRoleToUserAsync(new RbacAssignRoleToUserDto
            {
                UserId = user.Id,
                RoleId = role.Id,
                AssignedByUserId = assignedByUserId
            });

            ConsoleIO.WriteLine(response.IsSuccess ? response.Message : $"Error: {response.Message}");
        }

        private async Task RevokeAsync()
        {
            var user = await SelectUserAsync(activeOnly: false);
            if (user is null)
                return;

            var assignmentsResponse = await _rbacApplication.GetUserRolesAsync(new RbacUserRoleFilterDto
            {
                UserId = user.Id,
                Status = "A"
            });

            if (!assignmentsResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al consultar asignaciones: {assignmentsResponse.Message}");
                return;
            }

            var assignments = assignmentsResponse.Data?.ToList() ?? new List<RbacUserRoleDto>();
            if (assignments.Count == 0)
            {
                ConsoleIO.WriteLine("El usuario no tiene roles activos para revocar.");
                return;
            }

            ConsoleIO.WriteLine("Roles activos asignados:");
            for (var index = 0; index < assignments.Count; index++)
            {
                var assignment = assignments[index];
                ConsoleIO.WriteLine(
                    $"[{index + 1}] ROL_ID {assignment.RoleId} | {assignment.RoleName} | " +
                    $"Acceso total {assignment.RoleFullAccess}");
            }

            var selection = ConsoleInput.ReadSelection("Seleccione el rol a revocar: ", assignments.Count);
            var selected = assignments[selection - 1];
            if (!ConsoleInput.Confirm(
                    $"¿Revocar el rol [{selected.RoleName}] de [{user.Name}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _rbacApplication.RevokeRoleFromUserAsync(new RbacRevokeRoleFromUserDto
            {
                UserId = user.Id,
                RoleId = selected.RoleId
            });

            ConsoleIO.WriteLine(response.IsSuccess ? response.Message : $"Error: {response.Message}");
        }

        private async Task<UserDto?> SelectUserAsync(bool activeOnly)
        {
            var usersResponse = await _userApplication.ListAsync(new UserListFilterDto
            {
                Status = activeOnly ? "A" : null
            });

            if (!usersResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al consultar usuarios: {usersResponse.Message}");
                return null;
            }

            var users = usersResponse.Data?.ToList() ?? new List<UserDto>();
            if (users.Count == 0)
            {
                ConsoleIO.WriteLine(activeOnly ? "No existen usuarios activos." : "No existen usuarios registrados.");
                return null;
            }

            ConsoleIO.WriteLine(activeOnly ? "Usuarios activos:" : "Usuarios registrados:");
            for (var index = 0; index < users.Count; index++)
            {
                var user = users[index];
                ConsoleIO.WriteLine(
                    $"[{index + 1}] USR_ID {user.Id} | {user.Name} | {user.Email} | Estado {user.Status}");
            }

            var selection = ConsoleInput.ReadSelection("Seleccione un usuario: ", users.Count);
            return users[selection - 1];
        }

        private async Task<RbacRoleDto?> SelectRoleAsync()
        {
            var rolesResponse = await _rbacApplication.GetRolesAsync(new RbacRoleFilterDto { Status = "A" });
            if (!rolesResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al consultar roles: {rolesResponse.Message}");
                return null;
            }

            var roles = rolesResponse.Data?.ToList() ?? new List<RbacRoleDto>();
            if (roles.Count == 0)
            {
                ConsoleIO.WriteLine("No existen roles activos para asignar.");
                return null;
            }

            ConsoleIO.WriteLine("Roles activos:");
            for (var index = 0; index < roles.Count; index++)
            {
                var role = roles[index];
                ConsoleIO.WriteLine(
                    $"[{index + 1}] ROL_ID {role.Id} | {role.Name} | " +
                    $"NEG_ID {ConsoleInput.FormatNullable(role.BusinessId)} | Acceso total {role.FullAccess}");
            }

            var selection = ConsoleInput.ReadSelection("Seleccione un rol: ", roles.Count);
            return roles[selection - 1];
        }

        private static void PrintAssignments(IEnumerable<RbacUserRoleDto> assignments)
        {
            ConsoleIO.WriteLine(
                $"{"ASIG_ID",-8} {"USR_ID",-7} {"USUARIO",-22} {"CORREO",-30} {"ROL_ID",-7} " +
                $"{"ROL",-22} {"TOTAL",-6} {"ESTADO",-7} {"ASIGNADO",-19}");

            foreach (var assignment in assignments)
            {
                ConsoleIO.WriteLine(
                    $"{assignment.Id,-8} {assignment.UserId,-7} " +
                    $"{ConsoleInput.Truncate(assignment.UserName, 22),-22} " +
                    $"{ConsoleInput.Truncate(assignment.UserEmail, 30),-30} {assignment.RoleId,-7} " +
                    $"{ConsoleInput.Truncate(assignment.RoleName, 22),-22} {assignment.RoleFullAccess,-6} " +
                    $"{assignment.Status,-7} {assignment.AssignedAt:yyyy-MM-dd HH:mm:ss}");
            }
        }
    }
}
