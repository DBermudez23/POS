using AutoMapper;
using UtpPos.Framework.Application.DTO.Rbac;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Domain.Entity.Rbac;
using UtpPos.Framework.InfraStructure.Interface.Rbac;
using UtpPos.Framework.Transversal.Common;

namespace UtpPos.Framework.Application.Main.Rbac
{
    public class RbacApplication : IRbacApplication
    {
        private readonly IRbacRepository _repository;
        private readonly IMapper _mapper;

        public RbacApplication(IRbacRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<RbacModuleDto>>> ListModulesAsync(RbacModuleFilterDto filter)
        {
            try
            {
                var modules = await _repository.ListModulesAsync(
                    NormalizeCode(filter.Code),
                    EmptyToNull(filter.Name),
                    NormalizeStatus(filter.Status));

                return Success(_mapper.Map<IEnumerable<RbacModuleDto>>(modules), "Módulos consultados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<RbacModuleDto>>(ex.Message);
            }
        }

        public async Task<Response<int>> CreateModuleAsync(RbacModuleCreateDto dto)
        {
            try
            {
                var code = NormalizeCode(dto.Code);
                var validation = ValidateCatalogData(code, dto.Name, dto.Description, dto.IsSystem, dto.Status);
                if (validation is not null)
                    return Fail<int>(validation);

                var existing = await _repository.ListModulesAsync(code, null, null);
                if (existing.Any(module => CodesEqual(module.Code, code)))
                    return Fail<int>($"El módulo [{code}] ya existe. No se insertará nuevamente.");

                dto.Code = code!;
                dto.Name = dto.Name.Trim();
                dto.Description = EmptyToNull(dto.Description);
                dto.IsSystem = dto.IsSystem.Trim().ToUpperInvariant();
                dto.Status = dto.Status.Trim().ToUpperInvariant();

                var module = _mapper.Map<RbacModule>(dto);
                var id = await _repository.CreateModuleAsync(module);
                return Success(id, "Módulo creado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<RbacActionDto>>> ListActionsAsync(RbacActionFilterDto filter)
        {
            try
            {
                var actions = await _repository.ListActionsAsync(
                    NormalizeCode(filter.Code),
                    EmptyToNull(filter.Name),
                    NormalizeStatus(filter.Status));

                return Success(_mapper.Map<IEnumerable<RbacActionDto>>(actions), "Acciones consultadas correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<RbacActionDto>>(ex.Message);
            }
        }

        public async Task<Response<int>> CreateActionAsync(RbacActionCreateDto dto)
        {
            try
            {
                var code = NormalizeCode(dto.Code);
                var validation = ValidateCatalogData(code, dto.Name, dto.Description, dto.IsSystem, dto.Status);
                if (validation is not null)
                    return Fail<int>(validation);

                var existing = await _repository.ListActionsAsync(code, null, null);
                if (existing.Any(action => CodesEqual(action.Code, code)))
                    return Fail<int>($"La acción [{code}] ya existe. No se insertará nuevamente.");

                dto.Code = code!;
                dto.Name = dto.Name.Trim();
                dto.Description = EmptyToNull(dto.Description);
                dto.IsSystem = dto.IsSystem.Trim().ToUpperInvariant();
                dto.Status = dto.Status.Trim().ToUpperInvariant();

                var action = _mapper.Map<RbacAction>(dto);
                var id = await _repository.CreateActionAsync(action);
                return Success(id, "Acción creada correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<RbacPermissionDto>>> ListPermissionsAsync(RbacPermissionFilterDto filter)
        {
            try
            {
                var permissions = await _repository.ListPermissionsAsync(
                    NormalizePermissionName(filter.PermissionName),
                    NormalizeCode(filter.ModuleCode),
                    NormalizeCode(filter.ActionCode),
                    NormalizeStatus(filter.Status));

                return Success(
                    _mapper.Map<IEnumerable<RbacPermissionDto>>(permissions),
                    "Permisos consultados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<RbacPermissionDto>>(ex.Message);
            }
        }

        public async Task<Response<RbacPermissionCreateResultDto>> CreatePermissionAsync(RbacPermissionCreateDto dto)
        {
            try
            {
                var moduleCode = NormalizeCode(dto.ModuleCode);
                var actionCode = NormalizeCode(dto.ActionCode);

                if (dto.ModuleId <= 0 || dto.ActionId <= 0)
                    return Fail<RbacPermissionCreateResultDto>("El módulo y la acción seleccionados no son válidos.");

                if (!IsValidCode(moduleCode) || !IsValidCode(actionCode))
                    return Fail<RbacPermissionCreateResultDto>("Los códigos de módulo y acción no son válidos.");

                if (!IsStatus(dto.Status))
                    return Fail<RbacPermissionCreateResultDto>("PER_ESTADO debe ser A o I.");

                if (EmptyToNull(dto.Description)?.Length > 250)
                    return Fail<RbacPermissionCreateResultDto>("La descripción no puede superar 250 caracteres.");

                var activeModules = await _repository.ListModulesAsync(moduleCode, null, "A");
                if (!activeModules.Any(module => module.Id == dto.ModuleId && CodesEqual(module.Code, moduleCode)))
                    return Fail<RbacPermissionCreateResultDto>("El módulo no existe o se encuentra inactivo.");

                var activeActions = await _repository.ListActionsAsync(actionCode, null, "A");
                if (!activeActions.Any(action => action.Id == dto.ActionId && CodesEqual(action.Code, actionCode)))
                    return Fail<RbacPermissionCreateResultDto>("La acción no existe o se encuentra inactiva.");

                var existing = await _repository.ListPermissionsAsync(null, moduleCode, actionCode, null);
                if (existing.Any(permission =>
                        CodesEqual(permission.ModuleCode, moduleCode) &&
                        CodesEqual(permission.ActionCode, actionCode)))
                {
                    return Fail<RbacPermissionCreateResultDto>(
                        $"El permiso [{moduleCode}.{actionCode}] ya existe. No se insertará nuevamente.");
                }

                dto.ModuleCode = moduleCode!;
                dto.ActionCode = actionCode!;
                dto.Description = EmptyToNull(dto.Description) ?? $"Permiso para {moduleCode}.{actionCode}";
                dto.Status = dto.Status.Trim().ToUpperInvariant();

                var permission = _mapper.Map<RbacPermission>(dto);
                var result = await _repository.CreatePermissionAsync(permission);

                return Success(
                    new RbacPermissionCreateResultDto
                    {
                        PermissionId = result.PermissionId,
                        PermissionName = result.PermissionName
                    },
                    "Permiso creado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<RbacPermissionCreateResultDto>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<RbacRoleDto>>> GetRolesAsync(RbacRoleFilterDto filter)
        {
            filter ??= new RbacRoleFilterDto();

            if (filter.BusinessId is <= 0)
                return Fail<IEnumerable<RbacRoleDto>>("El negocio no es válido.");

            if (filter.OnlyGlobal == true && filter.BusinessId.HasValue)
                return Fail<IEnumerable<RbacRoleDto>>("No se puede filtrar simultáneamente por negocio y solo roles globales.");

            var status = NormalizeStatus(filter.Status);
            if (status is not null && !IsStatus(status))
                return Fail<IEnumerable<RbacRoleDto>>("El estado debe ser A o I.");

            try
            {
                var roles = await _repository.GetRolesAsync(
                    filter.BusinessId,
                    EmptyToNull(filter.Name),
                    status,
                    filter.OnlyGlobal);

                return Success(_mapper.Map<IEnumerable<RbacRoleDto>>(roles), "Roles consultados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<RbacRoleDto>>(ex.Message);
            }
        }

        public async Task<Response<RbacRoleDto>> GetRoleByIdAsync(int roleId)
        {
            if (roleId <= 0)
                return Fail<RbacRoleDto>("El rol no es válido.");

            try
            {
                var role = await _repository.GetRoleByIdAsync(roleId);
                return role is null
                    ? Fail<RbacRoleDto>("El rol no existe.")
                    : Success(_mapper.Map<RbacRoleDto>(role), "Rol consultado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<RbacRoleDto>(ex.Message);
            }
        }

        public async Task<Response<RbacRoleCreateResultDto>> CreateRoleAsync(RbacRoleCreateDto request)
        {
            var validation = ValidateRoleData(
                request.BusinessId,
                request.Name,
                request.Description,
                request.IsSystem,
                request.FullAccess,
                request.Status);

            if (validation is not null)
                return Fail<RbacRoleCreateResultDto>(validation);

            NormalizeRole(request);

            try
            {
                var role = _mapper.Map<RbacRole>(request);
                var roleId = await _repository.CreateRoleAsync(role);
                if (roleId <= 0)
                    return Fail<RbacRoleCreateResultDto>("El stored procedure no retornó un identificador de rol válido.");

                return Success(
                    new RbacRoleCreateResultDto { RoleId = roleId },
                    "Rol creado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<RbacRoleCreateResultDto>(ex.Message);
            }
        }

        public async Task<Response<bool>> UpdateRoleAsync(RbacRoleUpdateDto request)
        {
            if (request.RoleId <= 0)
                return Fail<bool>("El rol no es válido.");

            var validation = ValidateRoleData(
                request.BusinessId,
                request.Name,
                request.Description,
                request.IsSystem,
                request.FullAccess,
                request.Status);

            if (validation is not null)
                return Fail<bool>(validation);

            NormalizeRole(request);

            try
            {
                var role = _mapper.Map<RbacRole>(request);
                role.Id = request.RoleId;
                var updated = await _repository.UpdateRoleAsync(role);
                return updated
                    ? Success(true, "Rol actualizado correctamente.")
                    : Fail<bool>("No fue posible actualizar el rol.");
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

        public async Task<Response<bool>> DeactivateRoleAsync(int roleId)
        {
            if (roleId <= 0)
                return Fail<bool>("El rol no es válido.");

            try
            {
                var deactivated = await _repository.DeactivateRoleAsync(roleId);
                return deactivated
                    ? Success(true, "Rol inactivado correctamente.")
                    : Fail<bool>("No fue posible inactivar el rol.");
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<RbacRolePermissionDto>>> GetRolePermissionsAsync(
            RbacRolePermissionFilterDto filter)
        {
            filter ??= new RbacRolePermissionFilterDto();

            if (filter.RoleId is <= 0 || filter.PermissionId is <= 0)
                return Fail<IEnumerable<RbacRolePermissionDto>>("Los identificadores de rol y permiso deben ser mayores que cero.");

            var status = NormalizeStatus(filter.Status);
            if (status is not null && !IsStatus(status))
                return Fail<IEnumerable<RbacRolePermissionDto>>("El estado debe ser A o I.");

            var permissionName = NormalizePermissionName(filter.PermissionName);
            if (permissionName?.Length > 180)
                return Fail<IEnumerable<RbacRolePermissionDto>>("El nombre del permiso no puede superar 180 caracteres.");

            try
            {
                var assignments = await _repository.GetRolePermissionsAsync(
                    filter.RoleId,
                    filter.PermissionId,
                    permissionName,
                    status);

                return Success(
                    _mapper.Map<IEnumerable<RbacRolePermissionDto>>(assignments),
                    "Permisos asignados al rol consultados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<RbacRolePermissionDto>>(ex.Message);
            }
        }

        public async Task<Response<bool>> AssignPermissionToRoleAsync(RbacAssignPermissionToRoleDto request)
        {
            if (request.RoleId <= 0 || request.PermissionId <= 0)
                return Fail<bool>("El rol y el permiso deben ser válidos.");

            if (request.AssignedByUserId is <= 0)
                return Fail<bool>("El usuario que realiza la asignación no es válido.");

            try
            {
                var assignmentId = await _repository.AssignPermissionToRoleAsync(
                    request.RoleId,
                    request.PermissionId,
                    request.AssignedByUserId);

                return assignmentId > 0
                    ? Success(true, "Permiso asignado al rol correctamente.")
                    : Fail<bool>("El stored procedure no retornó un identificador de asignación válido.");
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

        public async Task<Response<bool>> RevokePermissionFromRoleAsync(RbacRevokePermissionFromRoleDto request)
        {
            if (request.RoleId <= 0 || request.PermissionId <= 0)
                return Fail<bool>("El rol y el permiso deben ser válidos.");

            try
            {
                var revoked = await _repository.RevokePermissionFromRoleAsync(request.RoleId, request.PermissionId);
                return revoked
                    ? Success(true, "Permiso revocado del rol correctamente.")
                    : Fail<bool>("No fue posible revocar el permiso del rol.");
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<RbacUserRoleDto>>> GetUserRolesAsync(RbacUserRoleFilterDto filter)
        {
            filter ??= new RbacUserRoleFilterDto();

            if (filter.UserId is <= 0 || filter.RoleId is <= 0)
                return Fail<IEnumerable<RbacUserRoleDto>>("Los identificadores de usuario y rol deben ser mayores que cero.");

            var status = NormalizeStatus(filter.Status);
            if (status is not null && !IsStatus(status))
                return Fail<IEnumerable<RbacUserRoleDto>>("El estado debe ser A o I.");

            var userEmail = EmptyToNull(filter.UserEmail)?.ToLowerInvariant();
            var roleName = EmptyToNull(filter.RoleName);

            try
            {
                var assignments = await _repository.GetUserRolesAsync(
                    filter.UserId,
                    filter.RoleId,
                    userEmail,
                    roleName,
                    status);

                return Success(
                    _mapper.Map<IEnumerable<RbacUserRoleDto>>(assignments),
                    "Roles asignados al usuario consultados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<RbacUserRoleDto>>(ex.Message);
            }
        }

        public async Task<Response<bool>> AssignRoleToUserAsync(RbacAssignRoleToUserDto request)
        {
            if (request.UserId <= 0 || request.RoleId <= 0)
                return Fail<bool>("El usuario y el rol deben ser válidos.");

            if (request.AssignedByUserId is <= 0)
                return Fail<bool>("El usuario que realiza la asignación no es válido.");

            try
            {
                var assignmentId = await _repository.AssignRoleToUserAsync(
                    request.UserId,
                    request.RoleId,
                    request.AssignedByUserId);

                return assignmentId > 0
                    ? Success(true, "Rol asignado al usuario correctamente.")
                    : Fail<bool>("El stored procedure no retornó un identificador de asignación válido.");
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

        public async Task<Response<bool>> RevokeRoleFromUserAsync(RbacRevokeRoleFromUserDto request)
        {
            if (request.UserId <= 0 || request.RoleId <= 0)
                return Fail<bool>("El usuario y el rol deben ser válidos.");

            try
            {
                var revoked = await _repository.RevokeRoleFromUserAsync(request.UserId, request.RoleId);
                return revoked
                    ? Success(true, "Rol revocado del usuario correctamente.")
                    : Fail<bool>("No fue posible revocar el rol del usuario.");
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<RbacUserPermissionDto>>> GetUserPermissionsAsync(int userId)
        {
            if (userId <= 0)
                return Fail<IEnumerable<RbacUserPermissionDto>>("El usuario no es válido.");

            try
            {
                var permissions = await _repository.GetUserPermissionsAsync(userId);
                return Success(
                    _mapper.Map<IEnumerable<RbacUserPermissionDto>>(permissions),
                    "Permisos efectivos consultados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<RbacUserPermissionDto>>(ex.Message);
            }
        }

        public async Task<Response<bool>> UserHasPermissionAsync(int userId, string permissionName)
        {
            if (userId <= 0)
                return Fail<bool>("El usuario no es válido.");

            var normalizedPermission = NormalizeRequiredPermission(permissionName);
            if (normalizedPermission is null)
                return Fail<bool>("El permiso debe tener el formato modulo.accion.");

            try
            {
                var permissions = await _repository.GetUserPermissionsAsync(userId);
                var hasPermission = permissions.Any(permission =>
                    IsStatus(permission.PermissionStatus) &&
                    string.Equals(permission.PermissionStatus.Trim(), "A", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        permission.PermissionName?.Trim(),
                        normalizedPermission,
                        StringComparison.OrdinalIgnoreCase));

                return Success(
                    hasPermission,
                    hasPermission
                        ? "El usuario tiene el permiso requerido."
                        : "El usuario no tiene el permiso requerido.");
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

        private static string? ValidateCatalogData(
            string? code,
            string? name,
            string? description,
            string? isSystem,
            string? status)
        {
            if (!IsValidCode(code))
                return "El código es obligatorio y solo admite letras minúsculas, números, guion bajo o guion medio.";

            if (code!.Length > 80)
                return "El código no puede superar 80 caracteres.";

            if (string.IsNullOrWhiteSpace(name))
                return "El nombre es obligatorio.";

            if (name.Trim().Length > 120)
                return "El nombre no puede superar 120 caracteres.";

            if (EmptyToNull(description)?.Length > 250)
                return "La descripción no puede superar 250 caracteres.";

            if (!IsSystemFlag(isSystem))
                return "El indicador de sistema debe ser S o N.";

            if (!IsStatus(status))
                return "El estado debe ser A o I.";

            return null;
        }

        private static string? ValidateRoleData(
            int? businessId,
            string? name,
            string? description,
            string? isSystem,
            string? fullAccess,
            string? status)
        {
            if (businessId is <= 0)
                return "El negocio no es válido.";

            if (string.IsNullOrWhiteSpace(name))
                return "El nombre del rol es obligatorio.";

            if (name.Trim().Length > 80)
                return "El nombre del rol no puede superar 80 caracteres.";

            if (EmptyToNull(description)?.Length > 250)
                return "La descripción del rol no puede superar 250 caracteres.";

            if (!IsSystemFlag(isSystem))
                return "ROL_ES_SISTEMA debe ser S o N.";

            if (!IsSystemFlag(fullAccess))
                return "ROL_ACCESO_TOTAL debe ser S o N.";

            if (!IsStatus(status))
                return "ROL_ESTADO debe ser A o I.";

            return null;
        }

        private static void NormalizeRole(RbacRoleCreateDto request)
        {
            request.Name = request.Name.Trim();
            request.Description = EmptyToNull(request.Description);
            request.IsSystem = request.IsSystem.Trim().ToUpperInvariant();
            request.FullAccess = request.FullAccess.Trim().ToUpperInvariant();
            request.Status = request.Status.Trim().ToUpperInvariant();
        }

        private static bool IsValidCode(string? code)
        {
            return !string.IsNullOrWhiteSpace(code) &&
                   code.All(character =>
                       character is >= 'a' and <= 'z' ||
                       character is >= '0' and <= '9' ||
                       character is '_' or '-');
        }

        private static bool IsSystemFlag(string? value)
        {
            return string.Equals(value?.Trim(), "S", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value?.Trim(), "N", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStatus(string? value)
        {
            return string.Equals(value?.Trim(), "A", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value?.Trim(), "I", StringComparison.OrdinalIgnoreCase);
        }

        private static bool CodesEqual(string? first, string? second)
        {
            return string.Equals(first?.Trim(), second?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string? NormalizeCode(string? value)
        {
            return EmptyToNull(value)?.ToLowerInvariant();
        }

        private static string? NormalizePermissionName(string? value)
        {
            return EmptyToNull(value)?.ToLowerInvariant();
        }

        private static string? NormalizeRequiredPermission(string? value)
        {
            var normalized = NormalizePermissionName(value);
            if (normalized is null)
                return null;

            var parts = normalized.Split('.', StringSplitOptions.None);
            return parts.Length == 2 && IsValidCode(parts[0]) && IsValidCode(parts[1])
                ? $"{parts[0]}.{parts[1]}"
                : null;
        }

        private static string? NormalizeStatus(string? value)
        {
            return EmptyToNull(value)?.ToUpperInvariant();
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static Response<T> Success<T>(T data, string message)
        {
            return new Response<T> { IsSuccess = true, Data = data, Message = message };
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T> { IsSuccess = false, Data = default, Message = message };
        }
    }
}
