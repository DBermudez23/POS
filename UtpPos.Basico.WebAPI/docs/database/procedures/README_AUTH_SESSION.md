# README - Procedures de Autenticación, JWT y Sesiones

## 1. Objetivo

Este documento describe los procedimientos almacenados creados para soportar la autenticación del Sistema de Ventas mediante JWT mínimo, refresh tokens y control de sesiones en base de datos.

La autenticación se implementará bajo el siguiente enfoque:

```text
JWT mínimo + refresh token + consulta de permisos en BD
```

El access token JWT solo debe contener información básica del usuario. Los permisos no se guardan inicialmente dentro del JWT, sino que se consultan desde base de datos usando el módulo RBAC.

---

## 2. Enfoque general

El flujo general de autenticación será:

```text
Usuario inicia sesión
        ↓
Backend consulta usuario en BD
        ↓
Backend valida contraseña en C#
        ↓
Backend genera access token JWT
        ↓
Backend genera refresh token
        ↓
Backend guarda hash del refresh token en TBL_SESIONES
        ↓
Cliente usa access token para consumir endpoints protegidos
```

Cuando el access token expire:

```text
Request con access token expirado
        ↓
Backend responde 401 Unauthorized
        ↓
Frontend intenta renovar usando refresh token
        ↓
Backend valida refresh token contra TBL_SESIONES
        ↓
Si es válido, genera nuevo access token
        ↓
Frontend reintenta la request original
```

El usuario solo debe ser redirigido al login si el refresh token también está vencido, revocado o no existe.

---

## 3. Tablas relacionadas

Los procedures de autenticación trabajan principalmente con:

```sql
TBL_USUARIOS
TBL_SESIONES
TBL_NEGOCIO
TBL_PARAMETROS
```

También se apoyan en los procedures del módulo RBAC para autorización:

```sql
UspListaPermisosUsuario
```

---

## 4. Tabla principal de usuarios

La tabla `TBL_USUARIOS` contiene la información del usuario del sistema.

Campos relevantes para autenticación:

```sql
USR_ID
USR_NEG_ID
USR_TIPO_DOC_ID
USR_NUMERO_DOCUMENTO
USR_NOMBRE
USR_CORREO
USR_PASSWORD
USR_TELEFONO
USR_ESTADO
USR_FECHA_CREACION
```

El campo:

```sql
USR_PASSWORD
```

debe almacenar el hash de la contraseña.

La contraseña no debe validarse en SQL Server. La validación debe realizarse en C# usando un mecanismo de hashing seguro, por ejemplo BCrypt.

---

## 5. Tabla principal de sesiones

La tabla `TBL_SESIONES` debe guardar las sesiones activas y revocadas de los usuarios.

Estructura esperada:

```sql
SES_ID
SES_USR_ID
SES_REFRESH_TOKEN_HASH
SES_FECHA_EXPIRACION
SES_IP
SES_USER_AGENT
SES_REVOCADA
SES_FECHA_CREACION
SES_FECHA_REVOCACION
```

El refresh token nunca debe guardarse plano en base de datos.

Solo debe guardarse:

```text
Hash del refresh token
```

---

## 6. Procedures creados

Para autenticación y sesiones se crearon los siguientes procedures:

```text
UspConsultaUsuarioAutenticacion
UspConsultaPerfilUsuario
UspRegistraSesionUsuario
UspConsultaSesionPorRefreshToken
UspRotaRefreshTokenSesion
UspRevocaSesionPorRefreshToken
UspRevocaSesionUsuario
UspRevocaSesionesUsuario
UspLimpiaSesionesExpiradas
```

---

# 7. Detalle de procedures

---

## 7.1. UspConsultaUsuarioAutenticacion

### Objetivo

Consultar la información necesaria de un usuario para realizar el proceso de login.

Este procedure retorna el hash de contraseña almacenado en `USR_PASSWORD`.

La contraseña debe validarse en la capa de aplicación, no en SQL Server.

### Nombre

```sql
dbo.UspConsultaUsuarioAutenticacion
```

### Parámetros de entrada

| Parámetro     |          Tipo | Obligatorio | Descripción                                    |
| ------------- | ------------: | ----------: | ---------------------------------------------- |
| `@USR_CORREO` | `VARCHAR(80)` |          Sí | Correo del usuario que intenta iniciar sesión. |

### Campos retornados

| Campo                  | Descripción                        |
| ---------------------- | ---------------------------------- |
| `USR_ID`               | Identificador del usuario.         |
| `USR_NEG_ID`           | Negocio asociado al usuario.       |
| `NEG_NOMBRE`           | Nombre del negocio.                |
| `USR_TIPO_DOC_ID`      | Tipo de documento del usuario.     |
| `TIPO_DOCUMENTO`       | Descripción del tipo de documento. |
| `USR_NUMERO_DOCUMENTO` | Número de documento.               |
| `USR_NOMBRE`           | Nombre del usuario.                |
| `USR_CORREO`           | Correo del usuario.                |
| `USR_PASSWORD_HASH`    | Hash de la contraseña.             |
| `USR_TELEFONO`         | Teléfono del usuario.              |
| `USR_ESTADO`           | Estado del usuario.                |
| `USR_FECHA_CREACION`   | Fecha de creación.                 |

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaUsuarioAutenticacion
    @USR_CORREO = 'admin@correo.com';
```

### Uso esperado en backend

```text
1. Backend recibe correo y contraseña.
2. Ejecuta UspConsultaUsuarioAutenticacion.
3. Obtiene USR_PASSWORD_HASH.
4. Valida contraseña con BCrypt o hasher configurado.
5. Si es correcta, genera tokens.
```

---

## 7.2. UspConsultaPerfilUsuario

### Objetivo

Consultar la información básica del usuario autenticado.

Este procedure será usado por el endpoint:

```text
GET /api/auth/me
```

### Nombre

```sql
dbo.UspConsultaPerfilUsuario
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                            |
| --------- | ----: | ----------: | -------------------------------------- |
| `@USR_ID` | `INT` |          Sí | Identificador del usuario autenticado. |

### Campos retornados

| Campo                  | Descripción                        |
| ---------------------- | ---------------------------------- |
| `USR_ID`               | Identificador del usuario.         |
| `USR_NEG_ID`           | Negocio asociado.                  |
| `NEG_NOMBRE`           | Nombre del negocio.                |
| `USR_TIPO_DOC_ID`      | Tipo de documento.                 |
| `TIPO_DOCUMENTO`       | Descripción del tipo de documento. |
| `USR_NUMERO_DOCUMENTO` | Número de documento.               |
| `USR_NOMBRE`           | Nombre del usuario.                |
| `USR_CORREO`           | Correo del usuario.                |
| `USR_TELEFONO`         | Teléfono del usuario.              |
| `USR_ESTADO`           | Estado del usuario.                |
| `USR_FECHA_CREACION`   | Fecha de creación.                 |

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaPerfilUsuario
    @USR_ID = 1;
```

---

## 7.3. UspRegistraSesionUsuario

### Objetivo

Registrar una nueva sesión de usuario guardando el hash del refresh token.

Este procedure se ejecuta después de un login exitoso.

### Nombre

```sql
dbo.UspRegistraSesionUsuario
```

### Parámetros de entrada

| Parámetro                 |           Tipo | Obligatorio | Descripción                                 |
| ------------------------- | -------------: | ----------: | ------------------------------------------- |
| `@SES_USR_ID`             |          `INT` |          Sí | Usuario dueño de la sesión.                 |
| `@SES_REFRESH_TOKEN_HASH` | `VARCHAR(255)` |          Sí | Hash del refresh token generado en backend. |
| `@SES_FECHA_EXPIRACION`   |     `DATETIME` |          Sí | Fecha de expiración de la sesión.           |
| `@SES_IP`                 |  `VARCHAR(45)` |          No | IP desde donde se inició sesión.            |
| `@SES_USER_AGENT`         | `VARCHAR(250)` |          No | User Agent del cliente.                     |

### Validaciones realizadas

El procedure valida que:

1. El usuario sea obligatorio.
2. El hash del refresh token sea obligatorio.
3. La fecha de expiración sea obligatoria.
4. La fecha de expiración sea mayor a la fecha actual.
5. El usuario exista y esté activo.
6. No exista otra sesión activa con el mismo hash de refresh token.

### Resultado

| Campo     | Descripción                        |
| --------- | ---------------------------------- |
| `Exitoso` | `1` si la sesión fue registrada.   |
| `Mensaje` | Mensaje descriptivo del resultado. |
| `SES_ID`  | Identificador de la sesión creada. |

### Ejemplo de uso

```sql
EXEC dbo.UspRegistraSesionUsuario
    @SES_USR_ID = 1,
    @SES_REFRESH_TOKEN_HASH = 'HASH_GENERADO_EN_BACKEND',
    @SES_FECHA_EXPIRACION = '2026-06-25 23:59:59',
    @SES_IP = '192.168.1.10',
    @SES_USER_AGENT = 'Mozilla/5.0';
```

---

## 7.4. UspConsultaSesionPorRefreshToken

### Objetivo

Consultar una sesión activa mediante el hash del refresh token.

Este procedure será usado principalmente por:

```text
POST /api/auth/refresh
POST /api/auth/logout
```

### Nombre

```sql
dbo.UspConsultaSesionPorRefreshToken
```

### Parámetros de entrada

| Parámetro                 |           Tipo | Obligatorio | Descripción                                       |
| ------------------------- | -------------: | ----------: | ------------------------------------------------- |
| `@SES_REFRESH_TOKEN_HASH` | `VARCHAR(255)` |          Sí | Hash del refresh token recibido desde el cliente. |

### Validaciones aplicadas en consulta

Solo retorna sesión si:

```text
La sesión no está revocada
La sesión no está expirada
El usuario sigue activo
```

### Campos retornados

| Campo                    | Descripción                       |
| ------------------------ | --------------------------------- |
| `SES_ID`                 | Identificador de la sesión.       |
| `SES_USR_ID`             | Usuario dueño de la sesión.       |
| `USR_NEG_ID`             | Negocio asociado al usuario.      |
| `NEG_NOMBRE`             | Nombre del negocio.               |
| `USR_NOMBRE`             | Nombre del usuario.               |
| `USR_CORREO`             | Correo del usuario.               |
| `USR_ESTADO`             | Estado del usuario.               |
| `SES_REFRESH_TOKEN_HASH` | Hash del refresh token.           |
| `SES_FECHA_EXPIRACION`   | Fecha de expiración.              |
| `SES_IP`                 | IP registrada.                    |
| `SES_USER_AGENT`         | User Agent registrado.            |
| `SES_REVOCADA`           | Indica si la sesión fue revocada. |
| `SES_FECHA_CREACION`     | Fecha de creación de sesión.      |
| `SES_FECHA_REVOCACION`   | Fecha de revocación.              |

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaSesionPorRefreshToken
    @SES_REFRESH_TOKEN_HASH = 'HASH_DEL_REFRESH_TOKEN';
```

---

## 7.5. UspRotaRefreshTokenSesion

### Objetivo

Rotar el refresh token de una sesión existente.

La rotación consiste en reemplazar el hash del refresh token anterior por uno nuevo y actualizar su fecha de expiración.

### Nombre

```sql
dbo.UspRotaRefreshTokenSesion
```

### Parámetros de entrada

| Parámetro                       |           Tipo | Obligatorio | Descripción                   |
| ------------------------------- | -------------: | ----------: | ----------------------------- |
| `@SES_ID`                       |          `INT` |          Sí | Identificador de la sesión.   |
| `@SES_REFRESH_TOKEN_HASH_NUEVO` | `VARCHAR(255)` |          Sí | Nuevo hash del refresh token. |
| `@SES_FECHA_EXPIRACION_NUEVA`   |     `DATETIME` |          Sí | Nueva fecha de expiración.    |
| `@SES_IP`                       |  `VARCHAR(45)` |          No | IP actualizada.               |
| `@SES_USER_AGENT`               | `VARCHAR(250)` |          No | User Agent actualizado.       |

### Validaciones realizadas

1. La sesión es obligatoria.
2. El nuevo hash del refresh token es obligatorio.
3. La nueva fecha de expiración es obligatoria.
4. La nueva fecha de expiración debe ser mayor a la fecha actual.
5. La sesión debe existir, estar activa, no estar expirada y pertenecer a un usuario activo.
6. No debe existir otra sesión activa con el nuevo hash.

### Resultado

| Campo     | Descripción                             |
| --------- | --------------------------------------- |
| `Exitoso` | `1` si la rotación fue exitosa.         |
| `Mensaje` | Mensaje descriptivo.                    |
| `SES_ID`  | Identificador de la sesión actualizada. |

### Ejemplo de uso

```sql
EXEC dbo.UspRotaRefreshTokenSesion
    @SES_ID = 1,
    @SES_REFRESH_TOKEN_HASH_NUEVO = 'NUEVO_HASH_REFRESH_TOKEN',
    @SES_FECHA_EXPIRACION_NUEVA = '2026-06-26 23:59:59',
    @SES_IP = '192.168.1.10',
    @SES_USER_AGENT = 'Mozilla/5.0';
```

---

## 7.6. UspRevocaSesionPorRefreshToken

### Objetivo

Revocar una sesión a partir del hash del refresh token.

Este procedure se puede usar en logout cuando el cliente envía su refresh token.

### Nombre

```sql
dbo.UspRevocaSesionPorRefreshToken
```

### Parámetros de entrada

| Parámetro                 |           Tipo | Obligatorio | Descripción             |
| ------------------------- | -------------: | ----------: | ----------------------- |
| `@SES_REFRESH_TOKEN_HASH` | `VARCHAR(255)` |          Sí | Hash del refresh token. |

### Comportamiento

Si encuentra una sesión activa con ese refresh token, actualiza:

```sql
SES_REVOCADA = 'S'
SES_FECHA_REVOCACION = GETDATE()
```

### Resultado

| Campo     | Descripción                          |
| --------- | ------------------------------------ |
| `Exitoso` | `1` si se revocó correctamente.      |
| `Mensaje` | Mensaje descriptivo.                 |
| `SES_ID`  | Identificador de la sesión revocada. |

### Ejemplo de uso

```sql
EXEC dbo.UspRevocaSesionPorRefreshToken
    @SES_REFRESH_TOKEN_HASH = 'HASH_DEL_REFRESH_TOKEN';
```

---

## 7.7. UspRevocaSesionUsuario

### Objetivo

Revocar una sesión específica de un usuario.

### Nombre

```sql
dbo.UspRevocaSesionUsuario
```

### Parámetros de entrada

| Parámetro     |  Tipo | Obligatorio | Descripción                 |
| ------------- | ----: | ----------: | --------------------------- |
| `@SES_ID`     | `INT` |          Sí | Identificador de la sesión. |
| `@SES_USR_ID` | `INT` |          Sí | Usuario dueño de la sesión. |

### Resultado

| Campo     | Descripción                          |
| --------- | ------------------------------------ |
| `Exitoso` | `1` si se revocó correctamente.      |
| `Mensaje` | Mensaje descriptivo.                 |
| `SES_ID`  | Identificador de la sesión revocada. |

### Ejemplo de uso

```sql
EXEC dbo.UspRevocaSesionUsuario
    @SES_ID = 1,
    @SES_USR_ID = 1;
```

---

## 7.8. UspRevocaSesionesUsuario

### Objetivo

Revocar todas las sesiones activas de un usuario.

Este procedure sirve para escenarios como:

```text
Cerrar sesión en todos los dispositivos
Cambio de contraseña
Bloqueo de usuario
Acción administrativa de seguridad
```

### Nombre

```sql
dbo.UspRevocaSesionesUsuario
```

### Parámetros de entrada

| Parámetro     |  Tipo | Obligatorio | Descripción                                  |
| ------------- | ----: | ----------: | -------------------------------------------- |
| `@SES_USR_ID` | `INT` |          Sí | Usuario al que se le revocarán las sesiones. |

### Resultado

| Campo                | Descripción                      |
| -------------------- | -------------------------------- |
| `Exitoso`            | `1` si se ejecutó correctamente. |
| `Mensaje`            | Mensaje descriptivo.             |
| `USR_ID`             | Usuario afectado.                |
| `SESIONES_REVOCADAS` | Cantidad de sesiones revocadas.  |

### Ejemplo de uso

```sql
EXEC dbo.UspRevocaSesionesUsuario
    @SES_USR_ID = 1;
```

---

## 7.9. UspLimpiaSesionesExpiradas

### Objetivo

Marcar como revocadas las sesiones que ya expiraron y aún aparecen activas.

Este procedure puede ejecutarse desde:

```text
Job programado
Tarea administrativa
Proceso de mantenimiento
Consola técnica futura
```

### Nombre

```sql
dbo.UspLimpiaSesionesExpiradas
```

### Parámetros de entrada

No recibe parámetros.

### Resultado

| Campo                | Descripción                                             |
| -------------------- | ------------------------------------------------------- |
| `Exitoso`            | `1` si se ejecutó correctamente.                        |
| `Mensaje`            | Mensaje descriptivo.                                    |
| `SESIONES_REVOCADAS` | Cantidad de sesiones expiradas marcadas como revocadas. |

### Ejemplo de uso

```sql
EXEC dbo.UspLimpiaSesionesExpiradas;
```

---

# 8. Flujo de login esperado

## 8.1. Request

```http
POST /api/auth/login
```

Body:

```json
{
  "email": "admin@correo.com",
  "password": "123456"
}
```

## 8.2. Backend

```text
1. Ejecuta UspConsultaUsuarioAutenticacion por correo.
2. Valida que el usuario exista.
3. Valida que el usuario esté activo.
4. Valida la contraseña en C# contra USR_PASSWORD_HASH.
5. Genera access token JWT mínimo.
6. Genera refresh token.
7. Hashea refresh token.
8. Ejecuta UspRegistraSesionUsuario.
9. Retorna tokens al cliente.
```

## 8.3. Response

```json
{
  "accessToken": "jwt_access_token",
  "refreshToken": "refresh_token",
  "expiresIn": 900,
  "tokenType": "Bearer"
}
```

---

# 9. Flujo de refresh token esperado

## 9.1. Cuándo se usa

El refresh token se usa cuando el access token expiró y el cliente necesita renovarlo sin obligar al usuario a iniciar sesión nuevamente.

## 9.2. Flujo

```text
Request normal con access token expirado
        ↓
Backend responde 401 Unauthorized
        ↓
Frontend llama POST /api/auth/refresh
        ↓
Backend hashea el refresh token recibido
        ↓
Ejecuta UspConsultaSesionPorRefreshToken
        ↓
Si la sesión existe y está activa, genera nuevo access token
        ↓
Genera nuevo refresh token
        ↓
Hashea nuevo refresh token
        ↓
Ejecuta UspRotaRefreshTokenSesion
        ↓
Retorna tokens nuevos
        ↓
Frontend reintenta request original
```

## 9.3. Si el refresh token falla

Si el refresh token está vencido, revocado o no coincide:

```text
Backend responde 401 Unauthorized
Frontend elimina tokens locales
Frontend redirige al login
```

---

# 10. Flujo de logout esperado

## 10.1. Request

```http
POST /api/auth/logout
```

## 10.2. Backend

```text
1. Recibe refresh token.
2. Hashea refresh token.
3. Ejecuta UspRevocaSesionPorRefreshToken.
4. Marca la sesión como revocada.
5. Retorna respuesta exitosa.
```

---

# 11. JWT mínimo

El JWT inicial debe ser pequeño.

Claims sugeridos:

```text
usr_id
usr_correo
usr_nombre
usr_neg_id
ses_id
```

No se recomienda incluir todos los permisos dentro del JWT en esta fase.

---

# 12. Consulta de permisos

Para autorización se debe usar:

```sql
dbo.UspListaPermisosUsuario
```

Este procedure retorna los permisos efectivos del usuario según sus roles activos.

Flujo futuro:

```text
Endpoint protegido
        ↓
JwtBearer valida JWT
        ↓
Backend obtiene USR_ID del claim
        ↓
Consulta permisos con UspListaPermisosUsuario
        ↓
Valida si tiene el permiso requerido
```

Ejemplo futuro:

```csharp
[RequirePermission("usuarios.crear")]
```

---

# 13. Diferencia entre access token y refresh token

## 13.1. Access token

```text
Se usa en cada request protegida.
Tiene vida corta.
No se guarda en TBL_SESIONES.
Se valida por firma y expiración.
Si expira, backend responde 401.
```

## 13.2. Refresh token

```text
Se usa solo para renovar sesión.
Tiene vida más larga.
Se guarda hasheado en TBL_SESIONES.
Se puede revocar.
Se valida contra la base de datos.
Si falla, el usuario debe iniciar sesión nuevamente.
```

---

# 14. Redis futuro

Redis no es necesario para la primera versión.

Más adelante puede implementarse para:

```text
Cachear permisos efectivos del usuario.
Evitar consultar SQL Server en cada validación de permisos.
Invalidar permisos cuando cambian roles.
Guardar blacklist de tokens revocados.
Aplicar rate limiting sobre login.
Cachear sesiones activas.
```

Ejemplo futuro de key:

```text
rbac:user:1:permissions
```

---

# 15. Consideraciones para Backend

## 15.1. Capas esperadas

La implementación debe seguir el patrón:

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
```

## 15.2. Componentes sugeridos

```text
IAuthApplication
AuthApplication
IAuthRepository
AuthRepository
JwtTokenService
PasswordHasherService
RefreshTokenService
AuthController
```

## 15.3. Endpoints sugeridos

| Método | Endpoint               | Descripción                    |
| ------ | ---------------------- | ------------------------------ |
| `POST` | `/api/auth/login`      | Iniciar sesión.                |
| `POST` | `/api/auth/refresh`    | Renovar tokens.                |
| `POST` | `/api/auth/logout`     | Cerrar sesión actual.          |
| `POST` | `/api/auth/logout-all` | Cerrar todas las sesiones.     |
| `GET`  | `/api/auth/me`         | Consultar usuario autenticado. |

---

# 16. Notas técnicas

* No validar contraseñas en SQL Server.
* No guardar refresh tokens planos.
* Guardar solo hash del refresh token.
* El access token debe tener vida corta.
* El refresh token debe tener vida más larga.
* El access token expirado no implica logout inmediato.
* El frontend debe intentar refresh antes de redirigir al login.
* Si el refresh token falla, ahí sí se debe limpiar sesión y volver al login.
* `TBL_SESIONES` se consulta para refresh, logout y revocación.
* `TBL_SESIONES` no debe consultarse en cada request normal en la primera versión.
* `UspListaPermisosUsuario` se usará para autorización por permisos.
* Redis será útil más adelante para cachear permisos y sesiones.
