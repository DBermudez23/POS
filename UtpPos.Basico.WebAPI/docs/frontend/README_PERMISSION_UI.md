# Permisos del usuario actual en Blazor

## Objetivo

El frontend consulta los permisos efectivos del usuario autenticado para mejorar la experiencia de uso: filtra opciones de navegacion y oculta acciones que el usuario no puede ejecutar.

Estas validaciones no reemplazan la seguridad del backend. Cada operacion protegida debe conservar `[Authorize]` y, cuando corresponda, `[RequirePermission]`.

## Endpoint backend

```http
GET /api/auth/me/permissions
Authorization: Bearer ACCESS_TOKEN
```

El endpoint requiere autenticacion, pero no un permiso RBAC adicional. Lee el identificador desde `AuthClaimTypes.UserId` y delega en:

```text
IRbacApplication.GetUserPermissionsAsync(userId)
    -> IRbacRepository.GetUserPermissionsAsync(userId)
    -> dbo.UspListaPermisosUsuario
```

La respuesta usa `Response<IEnumerable<RbacUserPermissionDto>>`. El DTO existente expone `UserAccessTotal`, `PermissionId`, `PermissionName`, `ModuleCode`, `ModuleName`, `ActionCode`, `ActionName`, `PermissionDescription` y `PermissionStatus`.

## Servicios frontend

`PermissionService` consume `GET api/auth/me/permissions` mediante `IRestApi` con `authorized: true`. No administra estado global ni agrega manualmente el bearer token.

`PermissionStateService` conserva solamente los valores `PermissionName` en un `HashSet<string>` con `StringComparer.OrdinalIgnoreCase`. Sus responsabilidades son:

- Cargar los permisos una vez por sesion de la aplicacion.
- Reutilizar una carga en curso para evitar solicitudes concurrentes duplicadas.
- Permitir una recarga explicita con `LoadAsync(forceReload: true)`.
- Resolver permisos por nombre o por `moduleCode` y `actionCode`.
- Mantener el conjunto vacio cuando el endpoint falla.
- Notificar cambios mediante `OnChange`.
- Borrar el estado en logout.

Los dos servicios se registran como `Scoped`. En WebAssembly esto representa estado por pestana; en el host evita compartir permisos entre usuarios o circuitos. No deben registrarse como singleton en un entorno server-side.

## Ciclo de carga

Despues de un login exitoso, `AuthService` guarda los tokens, actualiza `CustomAuthenticationStateProvider` y ejecuta una carga forzada de permisos. Si la carga falla, el login conserva su resultado exitoso y la interfaz protegida por permisos permanece oculta.

Al restaurar o recargar la aplicacion, `MainLayout` comprueba el estado autenticado y llama `LoadAsync()`. Como el servicio conserva `_isLoaded` y `_loadingTask`, no se repite la llamada cuando los permisos ya estan disponibles.

Durante el logout, `AuthService` intenta cerrar la sesion backend y, en el bloque `finally`, elimina tokens, permisos y estado de autenticacion.

La metadata de roles se administra por separado en `CurrentUserRoleStateService`. `RoleIsSystem` describe un rol tecnico/protegido y `RoleFullAccess` describe cobertura de permisos; ninguna bandera reemplaza las comprobaciones de `PermissionStateService`.

## Navegacion

`SidebarNavItem` acepta `PermissionName`. Un elemento sin permiso requerido se considera comun para usuarios autenticados. Un elemento con permiso solo se renderiza cuando:

```csharp
PermissionState.HasPermission(PermissionName)
```

Los nombres deben coincidir con el catalogo dinamico `modulo.accion`. El Dashboard queda comun; el resto del menu usa permisos como `ventas.registrar`, `productos.listar` y `kardex.listar`.

## PermissionView

El componente no consulta el backend. Solo observa `PermissionStateService` y vuelve a renderizar cuando cambia el conjunto.

```razor
<PermissionView Permission="negocios.crear">
    <MudButton>Crear negocio</MudButton>
</PermissionView>

<PermissionView Module="negocios" Action="crear">
    <ChildContent>
        <MudButton>Crear negocio</MudButton>
    </ChildContent>
    <NotAuthorized>
        <MudText>No tienes permisos para esta accion.</MudText>
    </NotAuthorized>
</PermissionView>
```

`NotAuthorized` es opcional; si no se proporciona, el componente no renderiza contenido alternativo.

## Pendiente

TODO: incorporar cache de permisos en backend con Redis cuando exista la infraestructura correspondiente. Esta fase no agrega cache backend, paquetes Redis ni permisos dentro del JWT.
