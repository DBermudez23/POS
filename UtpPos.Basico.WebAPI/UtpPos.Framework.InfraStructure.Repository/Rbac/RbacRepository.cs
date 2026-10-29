using System.Data;
using Dapper;
using UtpPos.Framework.Domain.Entity.Rbac;
using UtpPos.Framework.InfraStructure.Interface.Rbac;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Interface;

namespace UtpPos.Framework.InfraStructure.Repository.Rbac
{
    public class RbacRepository : IRbacRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public RbacRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<RbacModule>> ListModulesAsync(string? code, string? name, string? status)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@MOD_CODIGO", EmptyToNull(code), DbType.String);
            parameters.Add("@MOD_NOMBRE", EmptyToNull(name), DbType.String);
            parameters.Add("@MOD_ESTADO", EmptyToNull(status), DbType.String);

            var results = await connection.QueryAsync<RbacModuleDbModel>(
                "dbo.UspListaModulos",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapModule).ToList();
        }

        public async Task<int> CreateModuleAsync(RbacModule module)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@MOD_CODIGO", module.Code, DbType.String);
            parameters.Add("@MOD_NOMBRE", module.Name, DbType.String);
            parameters.Add("@MOD_DESCRIPCION", EmptyToNull(module.Description), DbType.String);
            parameters.Add("@MOD_ES_SISTEMA", module.IsSystem, DbType.String);
            parameters.Add("@MOD_ESTADO", module.Status, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<CreateModuleResult>(
                "dbo.UspCreaModulo",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return result!.MOD_ID;
        }

        public async Task<IEnumerable<RbacAction>> ListActionsAsync(string? code, string? name, string? status)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ACC_CODIGO", EmptyToNull(code), DbType.String);
            parameters.Add("@ACC_NOMBRE", EmptyToNull(name), DbType.String);
            parameters.Add("@ACC_ESTADO", EmptyToNull(status), DbType.String);

            var results = await connection.QueryAsync<RbacActionDbModel>(
                "dbo.UspListaAcciones",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapAction).ToList();
        }

        public async Task<int> CreateActionAsync(RbacAction action)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ACC_CODIGO", action.Code, DbType.String);
            parameters.Add("@ACC_NOMBRE", action.Name, DbType.String);
            parameters.Add("@ACC_DESCRIPCION", EmptyToNull(action.Description), DbType.String);
            parameters.Add("@ACC_ES_SISTEMA", action.IsSystem, DbType.String);
            parameters.Add("@ACC_ESTADO", action.Status, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<CreateActionResult>(
                "dbo.UspCreaAccion",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return result!.ACC_ID;
        }

        public async Task<IEnumerable<RbacPermission>> ListPermissionsAsync(
            string? permissionName,
            string? moduleCode,
            string? actionCode,
            string? status)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PER_NOMBRE", EmptyToNull(permissionName), DbType.String);
            parameters.Add("@MOD_CODIGO", EmptyToNull(moduleCode), DbType.String);
            parameters.Add("@ACC_CODIGO", EmptyToNull(actionCode), DbType.String);
            parameters.Add("@PER_ESTADO", EmptyToNull(status), DbType.String);

            var results = await connection.QueryAsync<RbacPermissionDbModel>(
                "dbo.UspListaPermisos",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapPermission).ToList();
        }

        public async Task<(int PermissionId, string PermissionName)> CreatePermissionAsync(RbacPermission permission)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@PER_MOD_ID", permission.ModuleId, DbType.Int32);
            parameters.Add("@PER_ACC_ID", permission.ActionId, DbType.Int32);
            parameters.Add("@PER_DESCRIPCION", EmptyToNull(permission.Description), DbType.String);
            parameters.Add("@PER_ESTADO", permission.Status, DbType.String);

            var result = await connection.QueryFirstOrDefaultAsync<CreatePermissionResult>(
                "dbo.UspCreaPermiso",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return (result!.PER_ID, result.PER_NOMBRE ?? string.Empty);
        }

        public async Task<IEnumerable<RbacRole>> GetRolesAsync(
            int? businessId,
            string? name,
            string? status,
            bool? onlyGlobal)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ROL_NEG_ID", businessId, DbType.Int32);
            parameters.Add("@ROL_NOMBRE", EmptyToNull(name), DbType.String);
            parameters.Add("@ROL_ESTADO", EmptyToNull(status), DbType.String);
            parameters.Add("@SOLO_GLOBALES", onlyGlobal, DbType.Boolean);

            var results = await connection.QueryAsync<RbacRoleDbModel>(
                "dbo.UspListaRoles",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapRole).ToList();
        }

        public async Task<RbacRole?> GetRoleByIdAsync(int roleId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ROL_ID", roleId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<RbacRoleDbModel>(
                "dbo.UspConsultaRol",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapRole(result);
        }

        public async Task<int> CreateRoleAsync(RbacRole role)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = BuildRoleParameters(role);

            var result = await connection.QueryFirstOrDefaultAsync<RoleOperationResult>(
                "dbo.UspCreaRol",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return result!.ROL_ID;
        }

        public async Task<bool> UpdateRoleAsync(RbacRole role)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = BuildRoleParameters(role);
            parameters.Add("@ROL_ID", role.Id, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<RoleOperationResult>(
                "dbo.UspActualizaRol",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return true;
        }

        public async Task<bool> DeactivateRoleAsync(int roleId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ROL_ID", roleId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<OperationResult>(
                "dbo.UspEliminaRol",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return true;
        }

        public async Task<IEnumerable<RbacRolePermission>> GetRolePermissionsAsync(
            int? roleId,
            int? permissionId,
            string? permissionName,
            string? status)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ROLPER_ROL_ID", roleId, DbType.Int32);
            parameters.Add("@ROLPER_PER_ID", permissionId, DbType.Int32);
            parameters.Add("@PER_NOMBRE", EmptyToNull(permissionName), DbType.String);
            parameters.Add("@ROLPER_ESTADO", EmptyToNull(status), DbType.String);

            var results = await connection.QueryAsync<RbacRolePermissionDbModel>(
                "dbo.UspListaRolPermisos",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapRolePermission).ToList();
        }

        public async Task<int> AssignPermissionToRoleAsync(
            int roleId,
            int permissionId,
            int? assignedByUserId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ROLPER_ROL_ID", roleId, DbType.Int32);
            parameters.Add("@ROLPER_PER_ID", permissionId, DbType.Int32);
            parameters.Add("@ROLPER_ASIGNADO_POR_USUARIO_ID", assignedByUserId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<RolePermissionOperationResult>(
                "dbo.UspAsignaPermisoRol",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return result!.ROLPER_ID;
        }

        public async Task<bool> RevokePermissionFromRoleAsync(int roleId, int permissionId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ROLPER_ROL_ID", roleId, DbType.Int32);
            parameters.Add("@ROLPER_PER_ID", permissionId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<OperationResult>(
                "dbo.UspRevocaPermisoRol",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return true;
        }

        public async Task<IEnumerable<RbacUserRole>> GetUserRolesAsync(
            int? userId,
            int? roleId,
            string? userEmail,
            string? roleName,
            string? status)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USRROL_USR_ID", userId, DbType.Int32);
            parameters.Add("@USRROL_ROL_ID", roleId, DbType.Int32);
            parameters.Add("@USR_CORREO", EmptyToNull(userEmail), DbType.String);
            parameters.Add("@ROL_NOMBRE", EmptyToNull(roleName), DbType.String);
            parameters.Add("@USRROL_ESTADO", EmptyToNull(status), DbType.String);

            var results = await connection.QueryAsync<RbacUserRoleDbModel>(
                "dbo.UspListaUsuarioRoles",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapUserRole).ToList();
        }

        public async Task<int> AssignRoleToUserAsync(int userId, int roleId, int? assignedByUserId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USRROL_USR_ID", userId, DbType.Int32);
            parameters.Add("@USRROL_ROL_ID", roleId, DbType.Int32);
            parameters.Add("@USRROL_ASIGNADO_POR_USUARIO_ID", assignedByUserId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<UserRoleOperationResult>(
                "dbo.UspAsignaRolUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return result!.USRROL_ID;
        }

        public async Task<bool> RevokeRoleFromUserAsync(int userId, int roleId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USRROL_USR_ID", userId, DbType.Int32);
            parameters.Add("@USRROL_ROL_ID", roleId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<OperationResult>(
                "dbo.UspRevocaRolUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            EnsureSuccessful(result?.Exitoso, result?.Mensaje);
            return true;
        }

        public async Task<IEnumerable<RbacUserPermission>> GetUserPermissionsAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@USR_ID", userId, DbType.Int32);

            var results = await connection.QueryAsync<RbacUserPermissionDbModel>(
                "dbo.UspListaPermisosUsuario",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapUserPermission).ToList();
        }

        private static void EnsureSuccessful(int? successful, string? message)
        {
            if (successful is null)
                throw new InvalidOperationException("El stored procedure no retornó un resultado de operación.");

            if (successful != 1)
                throw new InvalidOperationException(message ?? "El stored procedure reportó una operación fallida.");
        }

        private static RbacModule MapModule(RbacModuleDbModel model)
        {
            return new RbacModule
            {
                Id = model.MOD_ID,
                Code = model.MOD_CODIGO ?? string.Empty,
                Name = model.MOD_NOMBRE ?? string.Empty,
                Description = model.MOD_DESCRIPCION,
                IsSystem = model.MOD_ES_SISTEMA ?? string.Empty,
                Status = model.MOD_ESTADO ?? string.Empty,
                CreatedAt = model.MOD_FECHA_CREACION
            };
        }

        private static RbacAction MapAction(RbacActionDbModel model)
        {
            return new RbacAction
            {
                Id = model.ACC_ID,
                Code = model.ACC_CODIGO ?? string.Empty,
                Name = model.ACC_NOMBRE ?? string.Empty,
                Description = model.ACC_DESCRIPCION,
                IsSystem = model.ACC_ES_SISTEMA ?? string.Empty,
                Status = model.ACC_ESTADO ?? string.Empty,
                CreatedAt = model.ACC_FECHA_CREACION
            };
        }

        private static RbacPermission MapPermission(RbacPermissionDbModel model)
        {
            return new RbacPermission
            {
                Id = model.PER_ID,
                ModuleId = model.PER_MOD_ID,
                ModuleCode = model.MOD_CODIGO ?? string.Empty,
                ModuleName = model.MOD_NOMBRE ?? string.Empty,
                ActionId = model.PER_ACC_ID,
                ActionCode = model.ACC_CODIGO ?? string.Empty,
                ActionName = model.ACC_NOMBRE ?? string.Empty,
                PermissionName = model.PER_NOMBRE ?? string.Empty,
                Description = model.PER_DESCRIPCION,
                Status = model.PER_ESTADO ?? string.Empty,
                CreatedAt = model.PER_FECHA_CREACION
            };
        }

        private static RbacUserPermission MapUserPermission(RbacUserPermissionDbModel model)
        {
            return new RbacUserPermission
            {
                UserId = model.USR_ID,
                UserAccessTotal = model.USR_ACCESO_TOTAL ?? "N",
                PermissionId = model.PER_ID,
                PermissionName = model.PER_NOMBRE ?? string.Empty,
                ModuleCode = model.MOD_CODIGO ?? string.Empty,
                ModuleName = model.MOD_NOMBRE ?? string.Empty,
                ActionCode = model.ACC_CODIGO ?? string.Empty,
                ActionName = model.ACC_NOMBRE ?? string.Empty,
                PermissionDescription = model.PER_DESCRIPCION,
                PermissionStatus = model.PER_ESTADO ?? string.Empty
            };
        }

        private static RbacRole MapRole(RbacRoleDbModel model)
        {
            return new RbacRole
            {
                Id = model.ROL_ID,
                BusinessId = model.ROL_NEG_ID,
                BusinessName = model.NEG_NOMBRE,
                Name = model.ROL_NOMBRE ?? string.Empty,
                Description = model.ROL_DESCRIPCION,
                IsSystem = model.ROL_ES_SISTEMA ?? "N",
                FullAccess = model.ROL_ACCESO_TOTAL ?? "N",
                Status = model.ROL_ESTADO ?? string.Empty,
                CreatedAt = model.ROL_FECHA_CREACION
            };
        }

        private static RbacRolePermission MapRolePermission(RbacRolePermissionDbModel model)
        {
            return new RbacRolePermission
            {
                Id = model.ROLPER_ID,
                RoleId = model.ROLPER_ROL_ID,
                BusinessId = model.ROL_NEG_ID,
                BusinessName = model.NEG_NOMBRE,
                RoleName = model.ROL_NOMBRE ?? string.Empty,
                RoleDescription = model.ROL_DESCRIPCION,
                RoleIsSystem = model.ROL_ES_SISTEMA ?? "N",
                RoleFullAccess = model.ROL_ACCESO_TOTAL ?? "N",
                RoleStatus = model.ROL_ESTADO ?? string.Empty,
                PermissionId = model.ROLPER_PER_ID,
                PermissionName = model.PER_NOMBRE ?? string.Empty,
                ModuleCode = model.MOD_CODIGO ?? string.Empty,
                ModuleName = model.MOD_NOMBRE ?? string.Empty,
                ActionCode = model.ACC_CODIGO ?? string.Empty,
                ActionName = model.ACC_NOMBRE ?? string.Empty,
                PermissionDescription = model.PER_DESCRIPCION,
                PermissionStatus = model.PER_ESTADO ?? string.Empty,
                AssignedByUserId = model.ROLPER_ASIGNADO_POR_USUARIO_ID,
                AssignedByUserName = model.ASIGNADO_POR_USUARIO,
                Status = model.ROLPER_ESTADO ?? string.Empty,
                AssignedAt = model.ROLPER_FECHA_ASIGNACION
            };
        }

        private static RbacUserRole MapUserRole(RbacUserRoleDbModel model)
        {
            return new RbacUserRole
            {
                Id = model.USRROL_ID,
                UserId = model.USRROL_USR_ID,
                BusinessId = model.USR_NEG_ID,
                BusinessName = model.NEG_NOMBRE,
                UserDocumentNumber = model.USR_NUMERO_DOCUMENTO ?? string.Empty,
                UserName = model.USR_NOMBRE ?? string.Empty,
                UserEmail = model.USR_CORREO ?? string.Empty,
                UserStatus = model.USR_ESTADO ?? string.Empty,
                RoleId = model.USRROL_ROL_ID,
                RoleName = model.ROL_NOMBRE ?? string.Empty,
                RoleDescription = model.ROL_DESCRIPCION,
                RoleIsSystem = model.ROL_ES_SISTEMA ?? "N",
                RoleFullAccess = model.ROL_ACCESO_TOTAL ?? "N",
                RoleStatus = model.ROL_ESTADO ?? string.Empty,
                AssignedByUserId = model.USRROL_ASIGNADO_POR_USUARIO_ID,
                AssignedByUserName = model.ASIGNADO_POR_USUARIO,
                Status = model.USRROL_ESTADO ?? string.Empty,
                AssignedAt = model.USRROL_FECHA_ASIGNACION
            };
        }

        private static DynamicParameters BuildRoleParameters(RbacRole role)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ROL_NEG_ID", role.BusinessId, DbType.Int32);
            parameters.Add("@ROL_NOMBRE", role.Name, DbType.String);
            parameters.Add("@ROL_DESCRIPCION", EmptyToNull(role.Description), DbType.String);
            parameters.Add("@ROL_ES_SISTEMA", role.IsSystem, DbType.String);
            parameters.Add("@ROL_ACCESO_TOTAL", role.FullAccess, DbType.String);
            parameters.Add("@ROL_ESTADO", role.Status, DbType.String);
            return parameters;
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class RbacModuleDbModel
        {
            public int MOD_ID { get; set; }
            public string? MOD_CODIGO { get; set; }
            public string? MOD_NOMBRE { get; set; }
            public string? MOD_DESCRIPCION { get; set; }
            public string? MOD_ES_SISTEMA { get; set; }
            public string? MOD_ESTADO { get; set; }
            public DateTime MOD_FECHA_CREACION { get; set; }
        }

        private sealed class RbacActionDbModel
        {
            public int ACC_ID { get; set; }
            public string? ACC_CODIGO { get; set; }
            public string? ACC_NOMBRE { get; set; }
            public string? ACC_DESCRIPCION { get; set; }
            public string? ACC_ES_SISTEMA { get; set; }
            public string? ACC_ESTADO { get; set; }
            public DateTime ACC_FECHA_CREACION { get; set; }
        }

        private sealed class RbacPermissionDbModel
        {
            public int PER_ID { get; set; }
            public int PER_MOD_ID { get; set; }
            public string? MOD_CODIGO { get; set; }
            public string? MOD_NOMBRE { get; set; }
            public int PER_ACC_ID { get; set; }
            public string? ACC_CODIGO { get; set; }
            public string? ACC_NOMBRE { get; set; }
            public string? PER_NOMBRE { get; set; }
            public string? PER_DESCRIPCION { get; set; }
            public string? PER_ESTADO { get; set; }
            public DateTime PER_FECHA_CREACION { get; set; }
        }

        private sealed class RbacUserPermissionDbModel
        {
            public int USR_ID { get; set; }
            public string? USR_ACCESO_TOTAL { get; set; }
            public int PER_ID { get; set; }
            public string? PER_NOMBRE { get; set; }
            public string? MOD_CODIGO { get; set; }
            public string? MOD_NOMBRE { get; set; }
            public string? ACC_CODIGO { get; set; }
            public string? ACC_NOMBRE { get; set; }
            public string? PER_DESCRIPCION { get; set; }
            public string? PER_ESTADO { get; set; }
        }

        private sealed class RbacRoleDbModel
        {
            public int ROL_ID { get; set; }
            public int? ROL_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public string? ROL_NOMBRE { get; set; }
            public string? ROL_DESCRIPCION { get; set; }
            public string? ROL_ES_SISTEMA { get; set; }
            public string? ROL_ACCESO_TOTAL { get; set; }
            public string? ROL_ESTADO { get; set; }
            public DateTime ROL_FECHA_CREACION { get; set; }
        }

        private sealed class RbacRolePermissionDbModel
        {
            public int ROLPER_ID { get; set; }
            public int ROLPER_ROL_ID { get; set; }
            public int? ROL_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public string? ROL_NOMBRE { get; set; }
            public string? ROL_DESCRIPCION { get; set; }
            public string? ROL_ES_SISTEMA { get; set; }
            public string? ROL_ACCESO_TOTAL { get; set; }
            public string? ROL_ESTADO { get; set; }
            public int ROLPER_PER_ID { get; set; }
            public string? PER_NOMBRE { get; set; }
            public string? MOD_CODIGO { get; set; }
            public string? MOD_NOMBRE { get; set; }
            public string? ACC_CODIGO { get; set; }
            public string? ACC_NOMBRE { get; set; }
            public string? PER_DESCRIPCION { get; set; }
            public string? PER_ESTADO { get; set; }
            public int? ROLPER_ASIGNADO_POR_USUARIO_ID { get; set; }
            public string? ASIGNADO_POR_USUARIO { get; set; }
            public string? ROLPER_ESTADO { get; set; }
            public DateTime ROLPER_FECHA_ASIGNACION { get; set; }
        }

        private sealed class RbacUserRoleDbModel
        {
            public int USRROL_ID { get; set; }
            public int USRROL_USR_ID { get; set; }
            public int? USR_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public string? USR_NUMERO_DOCUMENTO { get; set; }
            public string? USR_NOMBRE { get; set; }
            public string? USR_CORREO { get; set; }
            public string? USR_ESTADO { get; set; }
            public int USRROL_ROL_ID { get; set; }
            public string? ROL_NOMBRE { get; set; }
            public string? ROL_DESCRIPCION { get; set; }
            public string? ROL_ES_SISTEMA { get; set; }
            public string? ROL_ACCESO_TOTAL { get; set; }
            public string? ROL_ESTADO { get; set; }
            public int? USRROL_ASIGNADO_POR_USUARIO_ID { get; set; }
            public string? ASIGNADO_POR_USUARIO { get; set; }
            public string? USRROL_ESTADO { get; set; }
            public DateTime USRROL_FECHA_ASIGNACION { get; set; }
        }

        private sealed class CreateModuleResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int MOD_ID { get; set; }
        }

        private sealed class CreateActionResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int ACC_ID { get; set; }
        }

        private sealed class CreatePermissionResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int PER_ID { get; set; }
            public string? PER_NOMBRE { get; set; }
        }

        private class OperationResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
        }

        private sealed class RoleOperationResult : OperationResult
        {
            public int ROL_ID { get; set; }
        }

        private sealed class RolePermissionOperationResult : OperationResult
        {
            public int ROLPER_ID { get; set; }
        }

        private sealed class UserRoleOperationResult : OperationResult
        {
            public int USRROL_ID { get; set; }
        }
    }
}
