# Vista de administracion de negocios

## Ruta frontend

La administracion de negocios se encuentra en:

```text
/negocios
```

La pagina requiere autenticacion mediante `[Authorize]`. Antes de consultar datos verifica `negocios.listar`; si el permiso no esta disponible, aplica un valor seguro por defecto y no muestra la administracion.

## Endpoints consumidos

```http
GET    /api/negocios
GET    /api/negocios/{id}
POST   /api/negocios
PATCH  /api/negocios/{id}
DELETE /api/negocios/{id}
```

Todos los requests usan `IRestApi` con `authorized: true`. Los filtros `Nit`, `Name` y `Status` se envian como query string al listado. Las operaciones de escritura conservan el retorno backend `Response<int>`.

La vista no contiene datos simulados. Listado, detalle, creacion, actualizacion y desactivacion consumen el backend real.

## Permisos

Cada accion visual refleja el mismo permiso exigido por `BusinessController`:

| Accion | Permiso |
| --- | --- |
| Mostrar menu, pagina y listado | `negocios.listar` |
| Consultar detalle | `negocios.consultar` |
| Crear | `negocios.crear` |
| Editar | `negocios.actualizar` |
| Desactivar | `negocios.eliminar` |

`PermissionView` oculta botones y `SidebarNavItem` filtra la navegacion. Las comprobaciones directas antes de ejecutar eventos evitan acciones accidentales desde componentes ya renderizados.

Estas comprobaciones son exclusivamente UX. La autoridad de seguridad sigue siendo `[Authorize]` y `[RequirePermission]` en WebAPI.

## DTOs y servicios

Los DTO frontend replican los contratos backend:

```text
BusinessDto
BusinessListFilterDto
BusinessCreateDto
BusinessUpdateDto
```

`BusinessService` encapsula los cinco endpoints y devuelve el `Response<T>` existente. La pagina coordina estado, dialogos y mensajes, pero no construye solicitudes HTTP.

La UI se divide en:

```text
Businesses.razor
BusinessFilterBar.razor
BusinessTable.razor
BusinessFormDialog.razor
BusinessDetailsDialog.razor
```

## Roles del usuario actual

El backend expone:

```http
GET /api/auth/me/roles
Authorization: Bearer ACCESS_TOKEN
```

Este endpoint requiere `[Authorize]`, no requiere un permiso RBAC adicional y consulta `IRbacApplication.GetUserRolesAsync` con el identificador de `AuthClaimTypes.UserId`.

`CurrentUserRoleService` obtiene los roles y `CurrentUserRoleStateService`, registrado como `Scoped`, los carga una vez y expone dos propiedades independientes:

```text
HasSystemRole
HasFullAccessRole
```

`RoleIsSystem = S` identifica un rol tecnico o protegido. `RoleFullAccess = S` indica cobertura de todos los permisos activos. Ninguno de los dos valores se infiere del otro.

La pagina puede mostrar las etiquetas separadas `Rol de sistema` y `Acceso total`, pero nunca habilita botones a partir de ellas. Las acciones dependen siempre de `PermissionStateService`.

Un usuario global con `BusinessId = null` no es bloqueado si esta autenticado y posee `negocios.listar`. El alcance efectivo de datos permanece en backend y stored procedures.

## Sesion y errores

Los errores 403 muestran el mensaje amigable retornado por `RestApi`. Los errores de validacion muestran `Response.Message`.

Ante un 401, la pagina o el dialogo ejecutan el logout existente, limpian tokens, permisos y roles, y navegan a `/login`.

Los roles se cargan despues del login y al restaurar `MainLayout`. En logout, `CurrentUserRoleStateService.Clear()` evita conservar metadata del usuario anterior.

## Pendientes

- Agregar pruebas automatizadas de componentes y de integracion HTTP.
- Incorporar paginacion backend si el volumen de negocios supera lo razonable para un listado completo.
- Evaluar cache backend con Redis en una fase futura; no forma parte de esta implementacion.
