using UtpPos.Console.Tools.Helpers;
using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.Interface.Rbac;
using ConsoleIO = System.Console;

namespace UtpPos.Console.Tools.Runners
{
    public class RolePermissionRunner
    {
        private readonly IRbacApplication _application;

        public RolePermissionRunner(IRbacApplication application)
        {
            _application = application;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                ConsoleIO.WriteLine("==== Gestión de Permisos por Rol ====");
                ConsoleIO.WriteLine();
                ConsoleIO.WriteLine("[1] Listar permisos de un rol");
                ConsoleIO.WriteLine("[2] Asignar permiso a rol");
                ConsoleIO.WriteLine("[3] Revocar permiso de rol");
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
            var roleId = ConsoleInput.ReadPositiveInt("ROL_ID: ");
            var response = await _application.GetRolePermissionsAsync(new RbacRolePermissionFilterDto
            {
                RoleId = roleId
            });

            if (!response.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error: {response.Message}");
                return;
            }

            var assignments = response.Data?.ToList() ?? new List<RbacRolePermissionDto>();
            if (assignments.Count == 0)
            {
                ConsoleIO.WriteLine("El rol no tiene asignaciones de permisos.");
                return;
            }

            PrintAssignments(assignments);
        }

        private async Task AssignAsync()
        {
            var role = await SelectRoleAsync(activeOnly: true);
            if (role is null)
                return;

            var permissionsResponse = await _application.ListPermissionsAsync(
                new RbacPermissionFilterDto { Status = "A" });

            if (!permissionsResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al consultar permisos: {permissionsResponse.Message}");
                return;
            }

            var permissions = permissionsResponse.Data?.ToList() ?? new List<RbacPermissionDto>();
            if (permissions.Count == 0)
            {
                ConsoleIO.WriteLine("No existen permisos activos para asignar.");
                return;
            }

            ConsoleIO.WriteLine("Permisos activos:");
            for (var index = 0; index < permissions.Count; index++)
            {
                var listedPermission = permissions[index];
                ConsoleIO.WriteLine(
                    $"[{index + 1}] PER_ID {listedPermission.Id} | {listedPermission.PermissionName} | " +
                    $"{listedPermission.ModuleCode} / {listedPermission.ActionCode}");
            }

            var permissionSelection = ConsoleInput.ReadSelection("Seleccione un permiso: ", permissions.Count);
            var selectedPermission = permissions[permissionSelection - 1];
            var assignedByUserId = ConsoleInput.ReadOptionalPositiveInt(
                "Usuario que realiza la asignación (USR_ID, opcional): ");

            if (!ConsoleInput.Confirm(
                    $"¿Asignar [{selectedPermission.PermissionName}] al rol [{role.Name}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _application.AssignPermissionToRoleAsync(
                new RbacAssignPermissionToRoleDto
                {
                    RoleId = role.Id,
                    PermissionId = selectedPermission.Id,
                    AssignedByUserId = assignedByUserId
                });

            ConsoleIO.WriteLine(response.IsSuccess ? response.Message : $"Error: {response.Message}");
        }

        private async Task RevokeAsync()
        {
            var role = await SelectRoleAsync(activeOnly: false);
            if (role is null)
                return;

            var assignmentsResponse = await _application.GetRolePermissionsAsync(
                new RbacRolePermissionFilterDto
                {
                    RoleId = role.Id,
                    Status = "A"
                });

            if (!assignmentsResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al consultar asignaciones: {assignmentsResponse.Message}");
                return;
            }

            var assignments = assignmentsResponse.Data?.ToList() ?? new List<RbacRolePermissionDto>();
            if (assignments.Count == 0)
            {
                ConsoleIO.WriteLine("El rol no tiene permisos activos para revocar.");
                return;
            }

            ConsoleIO.WriteLine("Permisos activos asignados:");
            for (var index = 0; index < assignments.Count; index++)
            {
                var assignment = assignments[index];
                ConsoleIO.WriteLine(
                    $"[{index + 1}] PER_ID {assignment.PermissionId} | {assignment.PermissionName} | " +
                    $"{assignment.ModuleCode} / {assignment.ActionCode}");
            }

            var selection = ConsoleInput.ReadSelection("Seleccione el permiso a revocar: ", assignments.Count);
            var selected = assignments[selection - 1];
            if (!ConsoleInput.Confirm(
                    $"¿Revocar [{selected.PermissionName}] del rol [{role.Name}]? [S/N]: "))
            {
                ConsoleIO.WriteLine("Operación cancelada.");
                return;
            }

            var response = await _application.RevokePermissionFromRoleAsync(
                new RbacRevokePermissionFromRoleDto
                {
                    RoleId = role.Id,
                    PermissionId = selected.PermissionId
                });

            ConsoleIO.WriteLine(response.IsSuccess ? response.Message : $"Error: {response.Message}");
        }

        private async Task<RbacRoleDto?> SelectRoleAsync(bool activeOnly)
        {
            var rolesResponse = await _application.GetRolesAsync(new RbacRoleFilterDto
            {
                Status = activeOnly ? "A" : null
            });

            if (!rolesResponse.IsSuccess)
            {
                ConsoleIO.WriteLine($"Error al consultar roles: {rolesResponse.Message}");
                return null;
            }

            var roles = rolesResponse.Data?.ToList() ?? new List<RbacRoleDto>();
            if (roles.Count == 0)
            {
                ConsoleIO.WriteLine(activeOnly ? "No existen roles activos." : "No existen roles registrados.");
                return null;
            }

            ConsoleIO.WriteLine(activeOnly ? "Roles activos:" : "Roles registrados:");
            for (var index = 0; index < roles.Count; index++)
            {
                var role = roles[index];
                ConsoleIO.WriteLine(
                    $"[{index + 1}] ROL_ID {role.Id} | {role.Name} | " +
                    $"NEG_ID {ConsoleInput.FormatNullable(role.BusinessId)} | Estado {role.Status}");
            }

            var selection = ConsoleInput.ReadSelection("Seleccione un rol: ", roles.Count);
            return roles[selection - 1];
        }

        private static void PrintAssignments(IEnumerable<RbacRolePermissionDto> assignments)
        {
            ConsoleIO.WriteLine(
                $"{"ASIG_ID",-8} {"ROL_ID",-7} {"ROL",-20} {"PER_ID",-7} {"PERMISO",-32} " +
                $"{"MÓDULO",-18} {"ACCIÓN",-18} {"ESTADO",-7} {"ASIGNADO",-19}");

            foreach (var assignment in assignments)
            {
                ConsoleIO.WriteLine(
                    $"{assignment.Id,-8} {assignment.RoleId,-7} " +
                    $"{ConsoleInput.Truncate(assignment.RoleName, 20),-20} {assignment.PermissionId,-7} " +
                    $"{ConsoleInput.Truncate(assignment.PermissionName, 32),-32} " +
                    $"{ConsoleInput.Truncate(assignment.ModuleCode, 18),-18} " +
                    $"{ConsoleInput.Truncate(assignment.ActionCode, 18),-18} " +
                    $"{assignment.Status,-7} {assignment.AssignedAt:yyyy-MM-dd HH:mm:ss}");
            }
        }
    }
}
