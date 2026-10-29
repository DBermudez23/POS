# Autorización Phase 2 - RBAC dinámico

## 1. Objetivo

Esta fase autoriza endpoints mediante permisos RBAC efectivos consultados en SQL Server. No modifica el login, los refresh tokens ni el contenido del JWT.

La autenticación responde quién es el usuario. La autorización determina qué puede ejecutar ese usuario.

## 2. JWT mínimo

El JWT conserva únicamente claims de identidad y sesión, entre ellos `usr_id`. Los permisos no se incluyen en el token porque pueden cambiar durante su vigencia y deben reflejar el estado actual de roles, asignaciones y permisos.

## 3. Nombres de permisos

Un permiso se identifica mediante:

```text
MOD_CODIGO + "." + ACC_CODIGO
```

Ejemplo:

```text
ventas.registrar
```

Los componentes de autorización normalizan módulo, acción y permiso con `Trim()` y minúsculas. Solo admiten letras, números, guion bajo y guion medio.

## 4. Uso en controladores

```csharp
using UtpPos.Framework.Services.WebApi.Authorization;

[Authorize]
[RequirePermission("ventas", "registrar")]
[HttpPost]
public async Task<IActionResult> RegisterAsync(...)
```

También se admite:

```csharp
[RequirePermission("ventas.registrar")]
```

`RequirePermissionAttribute` genera una policy dinámica con el nombre `Permission:ventas.registrar`. No existe un catálogo estático de policies en código.

## 5. Flujo de autorización

```text
Endpoint
  -> RequirePermissionAttribute
  -> PermissionAuthorizationPolicyProvider
  -> PermissionRequirement
  -> PermissionAuthorizationHandler
  -> IRbacApplication.UserHasPermissionAsync
  -> IRbacRepository.GetUserPermissionsAsync
  -> dbo.UspListaPermisosUsuario
```

El handler obtiene el usuario exclusivamente desde `AuthClaimTypes.UserId`. No lee permisos del JWT y no ejecuta Dapper.

## 6. UspListaPermisosUsuario

El repositorio ejecuta `dbo.UspListaPermisosUsuario` con `@USR_ID`. Application comprueba que exista un permiso activo cuyo `PER_NOMBRE` coincida con el permiso solicitado.

Si el usuario tiene un rol con `ROL_ACCESO_TOTAL = 'S'`, el SP retorna todos los permisos activos. Application sigue comprobando la coincidencia retornada y no aplica un bypass independiente.

## 7. Policies dinámicas

`PermissionAuthorizationPolicyProvider` detecta nombres que comienzan con `Permission:`, valida el formato y construye una policy que:

- exige un usuario autenticado;
- agrega `PermissionRequirement`;
- delega policies no relacionadas al provider estándar de ASP.NET Core.

No es necesario registrar cada permiso en `Program.cs`.

## 8. Respuestas HTTP

- `401 Unauthorized`: no existe JWT, es inválido o expiró.
- `403 Forbidden`: el JWT es válido, pero el usuario no tiene el permiso.

El cuerpo de un `403` conserva el formato transversal:

```json
{
  "IsSuccess": false,
  "Data": null,
  "Message": "No tiene permisos para ejecutar esta acción."
}
```

## 9. Prueba manual

1. Crear un permiso real, por ejemplo `parametros.listar`.
2. Asignarlo a un rol activo.
3. Asignar el rol a un usuario activo.
4. Agregar `[RequirePermission("parametros", "listar")]` a un endpoint.
5. Probar sin token: debe retornar `401`.
6. Probar con un usuario sin el permiso: debe retornar `403`.
7. Probar con un usuario que tenga el permiso: el endpoint debe ejecutarse.
8. Revocar el permiso o rol y repetir: el cambio debe reflejarse sin emitir otro JWT.

## 10. Relación con RBAC

Los resultados dependen de `TBL_MODULOS`, `TBL_ACCIONES`, `TBL_PERMISOS`, `TBL_ROLES`, `TBL_ROL_PERMISOS`, `TBL_USUARIO_ROLES` y sus estados activos. La resolución efectiva permanece encapsulada en `UspListaPermisosUsuario`.

## 11. Pendiente

- Aplicar `RequirePermission` gradualmente a endpoints funcionales con permisos ya creados.
- Agregar pruebas automatizadas de integración contra una base de datos controlada.
- Evaluar caché de permisos y su invalidación. Redis no forma parte de esta fase.
- Diferenciar, si se requiere, fallos de infraestructura durante autorización de una denegación normal, manteniendo siempre una política de fallo cerrado.
