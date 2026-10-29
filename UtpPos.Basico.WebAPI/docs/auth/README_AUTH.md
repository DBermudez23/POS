# README - Autenticación JWT, Refresh Tokens y Sesiones

## 1. Objetivo

Este documento describe la arquitectura general de autenticación implementada en el backend del Sistema de Ventas.

La autenticación permite que un usuario pueda iniciar sesión, recibir un access token JWT, renovar su sesión mediante refresh token, consultar su perfil autenticado y cerrar sesión.

La implementación actual corresponde a la primera fase del módulo de autenticación.

```text
Authentication Phase 1:
- Login
- JWT mínimo
- Refresh token
- Rotación de refresh token
- Logout
- Logout all
- Consulta de perfil autenticado
- Registro de sesiones en TBL_SESIONES
```

La autorización por permisos todavía no hace parte de esta fase. Será implementada posteriormente usando el módulo RBAC y el procedure:

```sql
UspListaPermisosUsuario
```

---

## 2. Enfoque implementado

El sistema usa el siguiente enfoque:

```text
JWT mínimo + Refresh Token + TBL_SESIONES + RBAC futuro
```

Esto significa que el access token JWT no guarda todos los permisos del usuario. El token contiene únicamente datos básicos de identidad y sesión.

Los permisos se consultarán desde base de datos cuando se implemente la segunda fase de autorización.

---

## 3. Flujo general de autenticación

```text
Usuario envía credenciales
        ↓
Backend consulta usuario en SQL Server
        ↓
Backend valida contraseña con BCrypt
        ↓
Backend genera access token JWT
        ↓
Backend genera refresh token
        ↓
Backend hashea refresh token con HMAC-SHA256
        ↓
Backend guarda hash del refresh token en TBL_SESIONES
        ↓
Cliente usa access token para consumir endpoints protegidos
```

Cuando el access token expira:

```text
Request con access token expirado
        ↓
Backend responde 401 Unauthorized
        ↓
Frontend llama /api/auth/refresh
        ↓
Backend valida refresh token contra TBL_SESIONES
        ↓
Backend genera nuevo access token
        ↓
Backend genera nuevo refresh token
        ↓
Backend rota el refresh token en TBL_SESIONES
        ↓
Frontend reintenta la request original
```

El usuario solo debe volver al login cuando el refresh token también falla.

---

## 4. Capas involucradas

La autenticación respeta la arquitectura en capas existente del proyecto:

```text
UtpPos.WebAPI
    ↓
Application.Interface
    ↓
Application.Main
    ↓
InfraStructure.Interface
    ↓
InfraStructure.Repository
    ↓
Dapper
    ↓
Stored Procedures
    ↓
SQL Server
```

---

## 5. Estructura implementada

### 5.1. DTOs

Ubicación:

```text
UtpPos.Framework.Application.DTO/Auth/
```

DTOs creados:

```text
LoginRequestDto
LoginResponseDto
RefreshTokenRequestDto
RefreshTokenResponseDto
LogoutRequestDto
AuthenticatedUserDto
AuthSessionDto
```

Responsabilidad:

```text
Representar las entradas y salidas del flujo de autenticación.
```

---

### 5.2. Entidades

Ubicación:

```text
UtpPos.Framework.Domain.Entity/
```

Entidades creadas:

```text
AuthUser
UserSession
```

Responsabilidad:

```text
Representar los datos de dominio usados durante autenticación y sesiones.
```

---

### 5.3. Application

Ubicaciones:

```text
UtpPos.Framework.Application.Interface/Auth/IAuthApplication.cs
UtpPos.Framework.Application.Main/Auth/AuthApplication.cs
```

Responsabilidad:

```text
Coordinar el flujo de autenticación.
Validar datos de entrada.
Validar contraseña.
Generar tokens.
Registrar sesiones.
Rotar refresh tokens.
Revocar sesiones.
Retornar Response<T>.
```

La capa Application no ejecuta SQL directamente.

---

### 5.4. Repository

Ubicaciones:

```text
UtpPos.Framework.InfraStructure.Interface/Auth/IAuthRepository.cs
UtpPos.Framework.InfraStructure.Repository/Auth/AuthRepository.cs
```

Responsabilidad:

```text
Consumir Stored Procedures mediante Dapper.
Mapear resultados de SQL Server.
No validar contraseñas.
No generar tokens.
No contener lógica HTTP.
```

---

### 5.5. Servicios transversales

Ubicación:

```text
UtpPos.Framework.Transversal.Common/Auth/
```

Servicios implementados:

```text
IJwtTokenService
JwtTokenService

IRefreshTokenService
RefreshTokenService

IRefreshTokenHashService
RefreshTokenHashService

IPasswordHasherService
PasswordHasherService
```

Responsabilidades:

```text
JwtTokenService:
Generar access tokens JWT.

RefreshTokenService:
Generar refresh tokens aleatorios y seguros.

RefreshTokenHashService:
Generar hash determinístico del refresh token usando HMAC-SHA256.

PasswordHasherService:
Validar contraseñas usando BCrypt.
```

---

## 6. Controller

Ubicación:

```text
UtpPos.WebAPI/Controllers/AuthController.cs
```

Ruta base:

```text
api/auth
```

Endpoints implementados:

| Método | Endpoint               | Autenticación | Descripción                                        |
| ------ | ---------------------- | ------------- | -------------------------------------------------- |
| `POST` | `/api/auth/login`      | No            | Inicia sesión.                                     |
| `POST` | `/api/auth/refresh`    | No            | Renueva access token y refresh token.              |
| `POST` | `/api/auth/logout`     | No            | Revoca la sesión actual usando refresh token.      |
| `POST` | `/api/auth/logout-all` | Sí            | Revoca todas las sesiones del usuario autenticado. |
| `GET`  | `/api/auth/me`         | Sí            | Retorna el perfil del usuario autenticado.         |

---

## 7. Login

### 7.1. Endpoint

```http
POST /api/auth/login
```

### 7.2. Request

```json
{
  "Email": "admin@correo.com",
  "Password": "123456"
}
```

### 7.3. Flujo interno

```text
1. AuthController recibe LoginRequestDto.
2. AuthApplication valida correo y contraseña.
3. AuthRepository ejecuta UspConsultaUsuarioAutenticacion.
4. SQL Server retorna usuario y USR_PASSWORD_HASH.
5. PasswordHasherService valida la contraseña con BCrypt.
6. JwtTokenService genera access token.
7. RefreshTokenService genera refresh token.
8. RefreshTokenHashService hashea refresh token.
9. AuthRepository ejecuta UspRegistraSesionUsuario.
10. El backend retorna LoginResponseDto.
```

### 7.4. Response esperado

```json
{
  "IsSuccess": true,
  "Data": {
    "AccessToken": "jwt",
    "RefreshToken": "refresh-token",
    "TokenType": "Bearer",
    "ExpiresIn": 900,
    "AccessTokenExpiresAtUtc": "2026-06-19T15:32:45Z",
    "RefreshTokenExpiresAtUtc": "2026-06-26T15:17:45Z",
    "User": {
      "Id": 1,
      "Name": "Usuario",
      "Email": "admin@correo.com",
      "Status": "A"
    },
    "Session": {
      "Id": 1,
      "UserId": 1,
      "Ip": "::1",
      "UserAgent": "PostmanRuntime/7.54.0"
    }
  },
  "Message": "Inicio de sesión exitoso."
}
```

---

## 8. Refresh token

### 8.1. Endpoint

```http
POST /api/auth/refresh
```

### 8.2. Request

```json
{
  "RefreshToken": "refresh-token"
}
```

### 8.3. Flujo interno

```text
1. Cliente envía refresh token.
2. Backend calcula hash HMAC-SHA256 del refresh token recibido.
3. AuthRepository ejecuta UspConsultaSesionPorRefreshToken.
4. SQL Server valida que la sesión:
   - exista
   - no esté revocada
   - no esté expirada
   - pertenezca a un usuario activo
5. Backend genera nuevo access token.
6. Backend genera nuevo refresh token.
7. Backend hashea el nuevo refresh token.
8. AuthRepository ejecuta UspRotaRefreshTokenSesion.
9. Backend retorna nuevos tokens.
```

### 8.4. Comportamiento esperado

El refresh token anterior deja de ser válido después de la rotación.

```text
refresh token viejo -> inválido
refresh token nuevo -> válido
```

Esto mejora la seguridad porque evita reutilización indefinida del mismo refresh token.

---

## 9. Logout

### 9.1. Endpoint

```http
POST /api/auth/logout
```

### 9.2. Request

```json
{
  "RefreshToken": "refresh-token"
}
```

### 9.3. Flujo interno

```text
1. Cliente envía refresh token.
2. Backend calcula hash del refresh token.
3. AuthRepository ejecuta UspRevocaSesionPorRefreshToken.
4. SQL Server marca la sesión como revocada.
```

La sesión queda con:

```text
SES_REVOCADA = 'S'
SES_FECHA_REVOCACION = GETDATE()
```

---

## 10. Logout all

### 10.1. Endpoint

```http
POST /api/auth/logout-all
```

Este endpoint requiere JWT válido.

### 10.2. Flujo interno

```text
1. JwtBearer valida el access token.
2. AuthController obtiene usr_id desde claims.
3. AuthApplication solicita revocar todas las sesiones del usuario.
4. AuthRepository ejecuta UspRevocaSesionesUsuario.
5. SQL Server revoca todas las sesiones activas del usuario.
```

Este endpoint sirve para:

```text
Cerrar sesión en todos los dispositivos.
Forzar cierre después de cambio de contraseña.
Forzar cierre por acción administrativa.
```

---

## 11. Perfil autenticado

### 11.1. Endpoint

```http
GET /api/auth/me
```

Este endpoint requiere JWT válido.

### 11.2. Flujo interno

```text
1. JwtBearer valida el access token.
2. AuthController obtiene usr_id desde claims.
3. AuthApplication consulta perfil.
4. AuthRepository ejecuta UspConsultaPerfilUsuario.
5. Backend retorna UserDto.
```

---

## 12. Access token

El access token es un JWT de vida corta.

Uso:

```http
Authorization: Bearer ACCESS_TOKEN
```

Características:

```text
Se usa en cada request protegida.
No se guarda en TBL_SESIONES.
Se valida por firma, issuer, audience y expiración.
Si expira, el backend responde 401 Unauthorized.
```

Duración actual:

```text
15 minutos
```

Configuración:

```json
"Jwt": {
  "AccessTokenExpirationMinutes": 15
}
```

---

## 13. Refresh token

El refresh token es un token largo y aleatorio.

Características:

```text
Se usa solo para renovar tokens.
Tiene vida más larga que el access token.
No se envía en cada request protegida.
No se guarda plano en base de datos.
Se guarda hasheado en TBL_SESIONES.
Se puede revocar.
Se rota en cada refresh.
```

Duración actual:

```text
7 días
```

Configuración:

```json
"Jwt": {
  "RefreshTokenExpirationDays": 7
}
```

---

## 14. Claims JWT

Claims implementados:

```text
sub
jti
iat
exp
usr_id
usr_correo
usr_nombre
sid
usr_neg_id
```

Descripción:

| Claim        | Descripción                                   |
| ------------ | --------------------------------------------- |
| `sub`        | Identificador principal del usuario.          |
| `jti`        | Identificador único del token.                |
| `iat`        | Fecha de emisión del token.                   |
| `exp`        | Fecha de expiración del token.                |
| `usr_id`     | Identificador del usuario.                    |
| `usr_correo` | Correo del usuario.                           |
| `usr_nombre` | Nombre del usuario.                           |
| `sid`        | Identificador de la sesión en `TBL_SESIONES`. |
| `usr_neg_id` | Identificador del negocio, si aplica.         |

El token no contiene permisos en esta fase.

---

## 15. Configuración JWT

La configuración se encuentra en:

```text
UtpPos.WebAPI/appsettings.json
UtpPos.WebAPI/appsettings.Development.json
Variables de entorno
```

Sección esperada:

```json
"Jwt": {
  "Issuer": "UtpPos.WebAPI",
  "Audience": "UtpPos.Client",
  "SecretKey": "CHANGE_ME_USE_ENVIRONMENT_VARIABLE_MINIMUM_32_BYTES",
  "AccessTokenExpirationMinutes": 15,
  "RefreshTokenExpirationDays": 7,
  "RefreshTokenHashKey": "CHANGE_ME_USE_ENVIRONMENT_VARIABLE_MINIMUM_32_BYTES"
}
```

Variables de entorno recomendadas:

```text
Jwt__SecretKey
Jwt__RefreshTokenHashKey
Jwt__Issuer
Jwt__Audience
```

En desarrollo se puede usar `appsettings.Development.json`, siempre que no se suba al repositorio con secretos reales.

---

## 16. Program.cs

La autenticación se registra en `Program.cs` mediante:

```text
AddAuthentication
AddJwtBearer
AddAuthorization
```

El orden correcto de middleware es:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

`UseAuthentication` debe ejecutarse antes de `UseAuthorization`.

---

## 17. Swagger

Swagger permite probar endpoints protegidos usando Bearer token.

Formato del header:

```http
Authorization: Bearer ACCESS_TOKEN
```

El access token se obtiene desde:

```http
POST /api/auth/login
```

Luego se pega en Swagger usando la opción Authorize.

---

## 18. Stored Procedures utilizados

| Procedure                          | Uso                                                |
| ---------------------------------- | -------------------------------------------------- |
| `UspConsultaUsuarioAutenticacion`  | Consultar usuario y hash de contraseña para login. |
| `UspConsultaPerfilUsuario`         | Consultar perfil autenticado.                      |
| `UspRegistraSesionUsuario`         | Registrar sesión con refresh token hasheado.       |
| `UspConsultaSesionPorRefreshToken` | Validar sesión activa por refresh token hash.      |
| `UspRotaRefreshTokenSesion`        | Rotar refresh token.                               |
| `UspRevocaSesionPorRefreshToken`   | Revocar sesión actual.                             |
| `UspRevocaSesionUsuario`           | Revocar una sesión específica del usuario.         |
| `UspRevocaSesionesUsuario`         | Revocar todas las sesiones del usuario.            |
| `UspLimpiaSesionesExpiradas`       | Marcar sesiones expiradas como revocadas.          |

---

## 19. Seguridad aplicada

### 19.1. Contraseñas

```text
Las contraseñas no se guardan en texto plano.
Se almacenan como hash BCrypt en USR_PASSWORD.
La validación se realiza en C#.
SQL Server no valida contraseñas.
```

### 19.2. Refresh tokens

```text
El refresh token no se guarda plano.
Se guarda un hash HMAC-SHA256 en TBL_SESIONES.
El hash es determinístico para poder buscarlo en base de datos.
```

### 19.3. Access token

```text
El access token tiene vida corta.
El access token no se guarda en base de datos.
El access token se valida con JwtBearer.
```

---

## 20. Relación con RBAC

La autenticación responde a la pregunta:

```text
¿Quién es el usuario?
```

RBAC responde a la pregunta:

```text
¿Qué puede hacer el usuario?
```

En esta fase solo se implementó autenticación.

La autorización por permisos será implementada después usando:

```sql
UspListaPermisosUsuario
```

Flujo futuro:

```text
Endpoint protegido
        ↓
JwtBearer valida token
        ↓
Se obtiene usr_id desde claims
        ↓
Se consulta UspListaPermisosUsuario
        ↓
Se valida si el usuario tiene el permiso requerido
```

Ejemplo futuro:

```csharp
[RequirePermission("usuarios.crear")]
```

---

## 21. Redis futuro

Redis no se implementa en esta fase.

Será útil más adelante para:

```text
Cachear permisos efectivos del usuario.
Evitar consultar SQL Server en cada autorización.
Invalidar permisos cuando cambien roles.
Guardar blacklist temporal de tokens revocados.
Aplicar rate limiting sobre login.
Cachear sesiones activas.
```

Ejemplo de key futura:

```text
rbac:user:1:permissions
```

---

## 22. Flujo de frontend esperado

El frontend debe manejar expiración de access token de esta forma:

```text
Request normal con access token
        ↓
Si responde 200, continúa normal
        ↓
Si responde 401, intenta /api/auth/refresh
        ↓
Si refresh responde 200:
    guarda nuevos tokens
    reintenta request original
        ↓
Si refresh responde 401:
    limpia tokens
    redirige al login
```

El usuario no debe ser enviado al login solo porque expiró el access token.

Solo debe volver al login cuando el refresh token también sea inválido, esté vencido o esté revocado.

---

## 23. Estado actual

### Implementado

```text
DTOs Auth
AuthUser
UserSession
IAuthRepository
AuthRepository
IAuthApplication
AuthApplication
JwtTokenService
RefreshTokenService
RefreshTokenHashService
PasswordHasherService
AuthController
Configuración JWT
Swagger con Bearer
Login
Refresh
Logout
Logout all
Me
Registro de sesiones
Rotación de refresh token
Revocación de sesiones
```

### Pendiente

```text
Autorización por permisos.
RequirePermissionAttribute.
PermissionRequirement.
PermissionAuthorizationHandler.
Integración con UspListaPermisosUsuario.
Cache opcional con Redis.
Rate limiting de login.
Gestión administrativa de sesiones.
Cambio de contraseña.
Recuperación de contraseña.
Política fuerte de contraseñas.
Migraciones formales.
```

---

## 24. Orden recomendado de siguientes fases

```text
1. Probar login, refresh, logout, logout-all y me.
2. Documentar ejemplos de consumo desde frontend.
3. Implementar autorización por permisos.
4. Crear RequirePermissionAttribute.
5. Integrar UspListaPermisosUsuario.
6. Evaluar caché de permisos con Redis.
7. Implementar gestión administrativa de sesiones.
8. Implementar migraciones formales.
```

---

## 25. Notas técnicas

```text
El access token expirado produce 401.
El refresh token se usa para renovar sesión.
El refresh token viejo debe fallar después de rotación.
El logout revoca la sesión asociada al refresh token.
Logout all revoca todas las sesiones del usuario.
Los secretos JWT no deben subirse al repositorio.
En producción se deben usar variables de entorno o secret manager.
Los usuarios existentes deben tener USR_PASSWORD como hash BCrypt.
```
