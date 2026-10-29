# Gestión de roles RBAC desde UtpPos.Console.Tools

## 1. Objetivo

Esta extensión permite administrar roles y sus asignaciones desde la consola técnica existente, sin ejecutar SQL manual en SSMS y sin crear otro proyecto.

La consola soporta:

- listar, crear, actualizar e inactivar roles;
- listar, asignar y revocar permisos de un rol;
- listar, asignar y revocar roles de un usuario;
- consultar los permisos efectivos de un usuario.

## 2. Arquitectura

Todas las operaciones mantienen el flujo:

```text
UtpPos.Console.Tools
    -> IRbacApplication
        -> RbacApplication
            -> IRbacRepository
                -> RbacRepository
                    -> Dapper
                        -> Stored Procedures
                            -> SQL Server
```

Los runners solo leen datos, presentan opciones y construyen DTOs. No referencian Dapper, no abren conexiones y no contienen SQL.

La consola conserva la configuración compartida de:

```text
UtpPos.WebAPI/appsettings.json
UtpPos.WebAPI/appsettings.Development.json
Variables de entorno
```

No tiene un `appsettings.json` propio.

## 3. Menú principal

```text
[1] Gestionar Módulos
[2] Gestionar Acciones
[3] Gestionar Permisos (Módulo + Acción)
[4] Gestionar Usuarios
[5] Gestionar Roles
[6] Gestionar Permisos por Rol
[7] Gestionar Roles por Usuario
[8] Consultar Permisos Efectivos de Usuario
[0] Salir
```

Las opciones existentes de módulos, acciones, permisos y usuarios permanecen disponibles.

## 4. Gestión de roles

`RoleRunner` permite:

1. listar roles globales y por negocio;
2. crear roles, usando un `BusinessId` vacío para roles globales;
3. consultar y actualizar un rol existente;
4. inactivar un rol con confirmación explícita.

Durante una actualización, Enter conserva el valor actual, `0` cambia el rol a alcance global y `-` borra la descripción. Los roles de sistema pueden ser rechazados por `UspEliminaRol` al intentar inactivarlos.

## 5. Permisos por rol

`RolePermissionRunner` permite consultar asignaciones, asignar y revocar permisos.

Para asignar:

1. consulta roles activos;
2. consulta permisos activos mediante `ListPermissionsAsync`;
3. muestra ambos catálogos numerados;
4. envía los identificadores seleccionados a `AssignPermissionToRoleAsync`.

El operador no escribe nombres de permisos ni utiliza una lista hardcodeada. Para revocar, solo se muestran permisos actualmente asignados y activos para el rol seleccionado.

## 6. Roles por usuario

`UserRoleRunner` usa `IUserApplication.ListAsync` para mostrar usuarios existentes. No solicita un ID sin contexto cuando el catálogo puede consultarse.

Para asignar un rol:

1. selecciona un usuario activo;
2. selecciona un rol activo consultado desde RBAC;
3. permite informar opcionalmente el usuario que realiza la asignación;
4. ejecuta `AssignRoleToUserAsync`.

La revocación muestra únicamente roles actualmente asignados y activos para el usuario seleccionado y exige confirmación.

## 7. Permisos efectivos

`UserPermissionRunner` solicita un `USR_ID` y ejecuta `GetUserPermissionsAsync`. El resultado muestra:

```text
UserId
UserAccessTotal
PermissionId
PermissionName
ModuleCode
ActionCode
PermissionStatus
```

Este listado representa lo que `PermissionAuthorizationHandler` evaluará para un atributo como:

```csharp
[RequirePermission("parametros", "listar")]
```

## 8. Stored procedures consumidos indirectamente

La consola no conoce estos nombres; son ejecutados por `RbacRepository`:

```text
UspListaRoles
UspConsultaRol
UspCreaRol
UspActualizaRol
UspEliminaRol
UspListaRolPermisos
UspAsignaPermisoRol
UspRevocaPermisoRol
UspListaUsuarioRoles
UspAsignaRolUsuario
UspRevocaRolUsuario
UspListaPermisosUsuario
```

También reutiliza `UspListaPermisos` para seleccionar permisos y el listado de usuarios encapsulado por `IUserApplication`.

## 9. Validaciones de entrada

- los identificadores requeridos deben ser enteros mayores que cero;
- los identificadores opcionales admiten Enter para `null`;
- los indicadores usan `S/N`;
- los estados usan `A/I`;
- los textos se recortan y respetan las longitudes de los DTOs;
- las revocaciones e inactivaciones requieren confirmación;
- los catálogos seleccionables se consultan desde Application;
- no se muestran contraseñas, hashes ni tokens.

Application y los stored procedures mantienen sus validaciones de negocio como defensa definitiva.

## 10. Flujo de prueba para RequirePermission

1. Verificar que exista `parametros.listar` en la opción de permisos técnicos.
2. Crear o seleccionar el rol `ADMINISTRADOR`.
3. Asignar `parametros.listar` al rol.
4. Asignar el rol al usuario requerido.
5. Consultar los permisos efectivos del usuario y verificar `parametros.listar`.
6. Iniciar sesión mediante `/api/auth/login`.
7. Consumir un endpoint con `[RequirePermission("parametros", "listar")]`.
8. Esperar `200` con el permiso activo.
9. Revocar el permiso o rol y esperar `403` en una nueva solicitud.

Como el JWT no contiene permisos, la revocación se refleja sin emitir otro token.

## 11. Ejecución

Desde la raíz de la solución:

```powershell
dotnet run --project .\UtpPos.Console.Tools\UtpPos.Console.Tools.csproj
```

La cadena `ConnectionStrings:DefaultConnection` debe estar configurada en WebAPI o mediante variables de entorno.

## 12. Pendiente

Esta consola es una herramienta técnica interna. Un frontend administrativo podrá reemplazar su interfaz en el futuro consumiendo controladores WebAPI protegidos, pero deberá reutilizar las mismas capas Application y Repository.

También permanecen pendientes pruebas automatizadas de integración y una posible caché de permisos con invalidación. Redis no forma parte de esta extensión.
