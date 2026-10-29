# README - Procedures y Vistas de Roles, Rol-Permisos y Usuario-Roles

## 1. Objetivo

Este documento describe las vistas y procedimientos almacenados creados para completar la segunda fase del módulo RBAC del Sistema de Ventas.

En la primera fase del RBAC se construyeron los catálogos técnicos de:

```text
Módulos
Acciones
Permisos
```

En esta segunda fase se implementa la relación entre:

```text
Roles
Permisos asignados a roles
Roles asignados a usuarios
Permisos efectivos de un usuario
```

Esto deja preparada la base de datos para la futura implementación de autenticación con JWT y autorización por permisos.

---

## 2. Tablas relacionadas

Las vistas y procedures de este documento trabajan principalmente con las siguientes tablas:

```sql
TBL_ROLES
TBL_ROL_PERMISOS
TBL_USUARIO_ROLES
TBL_USUARIOS
TBL_PERMISOS
TBL_MODULOS
TBL_ACCIONES
```

También se apoya en la vista existente:

```sql
VW_RBAC_PERMISOS
```

---

## 3. Relación general del módulo

El flujo completo del RBAC queda planteado así:

```text
Módulos + Acciones
        ↓
Permisos
        ↓
Permisos asignados a roles
        ↓
Roles asignados a usuarios
        ↓
Permisos efectivos del usuario
        ↓
Autorización en backend
```

Ejemplo:

```text
Módulo: usuarios
Acción: crear
Permiso: usuarios.crear

Rol: ADMINISTRADOR
Permiso asignado al rol: usuarios.crear

Usuario: Juan Pérez
Rol asignado al usuario: ADMINISTRADOR

Resultado:
Juan Pérez tiene el permiso usuarios.crear
```

---

## 4. Vistas creadas

Para facilitar la consulta del RBAC se crearon las siguientes vistas:

```text
VW_RBAC_ROL_PERMISOS
VW_RBAC_USUARIO_ROLES
```

---

# 5. Vistas

---

## 5.1. VW_RBAC_ROL_PERMISOS

### Objetivo

Consultar los permisos asignados a cada rol, incluyendo información del rol, negocio, permiso, módulo, acción y usuario que realizó la asignación.

### Nombre

```sql
dbo.VW_RBAC_ROL_PERMISOS
```

### Tablas utilizadas

```sql
TBL_ROL_PERMISOS
TBL_ROLES
TBL_NEGOCIO
VW_RBAC_PERMISOS
TBL_USUARIOS
```

### Campos principales retornados

| Campo                            | Descripción                                                    |
| -------------------------------- | -------------------------------------------------------------- |
| `ROLPER_ID`                      | Identificador de la asignación rol-permiso.                    |
| `ROLPER_ROL_ID`                  | Identificador del rol.                                         |
| `ROL_NEG_ID`                     | Negocio asociado al rol. Puede ser `NULL` para roles globales. |
| `NEG_NOMBRE`                     | Nombre del negocio asociado.                                   |
| `ROL_NOMBRE`                     | Nombre del rol.                                                |
| `ROL_DESCRIPCION`                | Descripción del rol.                                           |
| `ROL_ES_SISTEMA`                 | Indica si el rol es de sistema.                                |
| `ROL_ACCESO_TOTAL`               | Indica si el rol tiene acceso total.                           |
| `ROL_ESTADO`                     | Estado del rol.                                                |
| `ROLPER_PER_ID`                  | Identificador del permiso asignado.                            |
| `PER_NOMBRE`                     | Nombre lógico del permiso. Ejemplo: `usuarios.crear`.          |
| `MOD_CODIGO`                     | Código técnico del módulo.                                     |
| `MOD_NOMBRE`                     | Nombre visible del módulo.                                     |
| `ACC_CODIGO`                     | Código técnico de la acción.                                   |
| `ACC_NOMBRE`                     | Nombre visible de la acción.                                   |
| `PER_DESCRIPCION`                | Descripción del permiso.                                       |
| `PER_ESTADO`                     | Estado del permiso.                                            |
| `ROLPER_ASIGNADO_POR_USUARIO_ID` | Usuario que asignó el permiso al rol.                          |
| `ASIGNADO_POR_USUARIO`           | Nombre del usuario que realizó la asignación.                  |
| `ROLPER_ESTADO`                  | Estado de la asignación rol-permiso.                           |
| `ROLPER_FECHA_ASIGNACION`        | Fecha de asignación del permiso al rol.                        |

### Ejemplo de uso

```sql
SELECT *
FROM dbo.VW_RBAC_ROL_PERMISOS
ORDER BY
    ROL_NOMBRE,
    PER_NOMBRE;
```

---

## 5.2. VW_RBAC_USUARIO_ROLES

### Objetivo

Consultar los roles asignados a cada usuario, incluyendo información del usuario, negocio, rol y usuario que realizó la asignación.

### Nombre

```sql
dbo.VW_RBAC_USUARIO_ROLES
```

### Tablas utilizadas

```sql
TBL_USUARIO_ROLES
TBL_USUARIOS
TBL_NEGOCIO
TBL_ROLES
```

### Campos principales retornados

| Campo                            | Descripción                                   |
| -------------------------------- | --------------------------------------------- |
| `USRROL_ID`                      | Identificador de la asignación usuario-rol.   |
| `USRROL_USR_ID`                  | Identificador del usuario.                    |
| `USR_NEG_ID`                     | Negocio asociado al usuario.                  |
| `NEG_NOMBRE`                     | Nombre del negocio asociado.                  |
| `USR_NUMERO_DOCUMENTO`           | Número de documento del usuario.              |
| `USR_NOMBRE`                     | Nombre del usuario.                           |
| `USR_CORREO`                     | Correo del usuario.                           |
| `USR_ESTADO`                     | Estado del usuario.                           |
| `USRROL_ROL_ID`                  | Identificador del rol asignado.               |
| `ROL_NOMBRE`                     | Nombre del rol.                               |
| `ROL_DESCRIPCION`                | Descripción del rol.                          |
| `ROL_ES_SISTEMA`                 | Indica si el rol es de sistema.               |
| `ROL_ACCESO_TOTAL`               | Indica si el rol tiene acceso total.          |
| `ROL_ESTADO`                     | Estado del rol.                               |
| `USRROL_ASIGNADO_POR_USUARIO_ID` | Usuario que asignó el rol.                    |
| `ASIGNADO_POR_USUARIO`           | Nombre del usuario que realizó la asignación. |
| `USRROL_ESTADO`                  | Estado de la asignación usuario-rol.          |
| `USRROL_FECHA_ASIGNACION`        | Fecha de asignación del rol al usuario.       |

### Ejemplo de uso

```sql
SELECT *
FROM dbo.VW_RBAC_USUARIO_ROLES
ORDER BY
    USR_NOMBRE,
    ROL_NOMBRE;
```

---

# 6. Procedures creados

Se crearon los siguientes procedimientos almacenados:

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

---

# 7. Procedures de Roles

---

## 7.1. UspListaRoles

### Objetivo

Listar roles registrados en el sistema, permitiendo filtrar por negocio, nombre, estado o roles globales.

### Nombre

```sql
dbo.UspListaRoles
```

### Parámetros de entrada

| Parámetro        |          Tipo | Obligatorio | Descripción                                                    |
| ---------------- | ------------: | ----------: | -------------------------------------------------------------- |
| `@ROL_NEG_ID`    |         `INT` |          No | Filtra roles por negocio.                                      |
| `@ROL_NOMBRE`    | `VARCHAR(80)` |          No | Filtra roles por nombre.                                       |
| `@ROL_ESTADO`    |     `CHAR(1)` |          No | Filtra por estado: `A` o `I`.                                  |
| `@SOLO_GLOBALES` |         `BIT` |          No | Si es `1`, lista solo roles globales con `ROL_NEG_ID IS NULL`. |

### Ejemplos de uso

Listar todos los roles:

```sql
EXEC dbo.UspListaRoles;
```

Listar roles activos:

```sql
EXEC dbo.UspListaRoles
    @ROL_ESTADO = 'A';
```

Listar roles de un negocio:

```sql
EXEC dbo.UspListaRoles
    @ROL_NEG_ID = 1;
```

Listar roles globales:

```sql
EXEC dbo.UspListaRoles
    @SOLO_GLOBALES = 1;
```

---

## 7.2. UspConsultaRol

### Objetivo

Consultar un rol específico mediante su identificador.

### Nombre

```sql
dbo.UspConsultaRol
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                        |
| --------- | ----: | ----------: | ---------------------------------- |
| `@ROL_ID` | `INT` |          Sí | Identificador del rol a consultar. |

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaRol
    @ROL_ID = 1;
```

---

## 7.3. UspCreaRol

### Objetivo

Crear un nuevo rol en el sistema.

Un rol puede ser global o asociado a un negocio específico.

```text
Rol global:
ROL_NEG_ID = NULL

Rol por negocio:
ROL_NEG_ID = ID del negocio
```

### Nombre

```sql
dbo.UspCreaRol
```

### Parámetros de entrada

| Parámetro           |           Tipo | Obligatorio | Descripción                                              |
| ------------------- | -------------: | ----------: | -------------------------------------------------------- |
| `@ROL_NEG_ID`       |          `INT` |          No | Negocio asociado al rol. Si es `NULL`, el rol es global. |
| `@ROL_NOMBRE`       |  `VARCHAR(80)` |          Sí | Nombre del rol.                                          |
| `@ROL_DESCRIPCION`  | `VARCHAR(250)` |          No | Descripción del rol.                                     |
| `@ROL_ES_SISTEMA`   |      `CHAR(1)` |          No | Indica si es rol de sistema. Valores: `S` o `N`.         |
| `@ROL_ACCESO_TOTAL` |      `CHAR(1)` |          No | Indica si el rol tiene acceso total. Valores: `S` o `N`. |
| `@ROL_ESTADO`       |      `CHAR(1)` |          No | Estado del rol. Valores: `A` o `I`.                      |

### Validaciones realizadas

El procedimiento valida que:

1. El nombre del rol sea obligatorio.
2. `ROL_ES_SISTEMA` sea `S` o `N`.
3. `ROL_ACCESO_TOTAL` sea `S` o `N`.
4. `ROL_ESTADO` sea `A` o `I`.
5. Si se envía negocio, el negocio debe existir y estar activo.
6. No exista otro rol con el mismo nombre para el mismo alcance.

### Resultado

| Campo     | Descripción                        |
| --------- | ---------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa.   |
| `Mensaje` | Mensaje descriptivo del resultado. |
| `ROL_ID`  | Identificador del rol creado.      |

### Ejemplo de uso

Crear rol global:

```sql
EXEC dbo.UspCreaRol
    @ROL_NEG_ID = NULL,
    @ROL_NOMBRE = 'ADMINISTRADOR',
    @ROL_DESCRIPCION = 'Rol administrador del sistema',
    @ROL_ES_SISTEMA = 'S',
    @ROL_ACCESO_TOTAL = 'S',
    @ROL_ESTADO = 'A';
```

Crear rol asociado a negocio:

```sql
EXEC dbo.UspCreaRol
    @ROL_NEG_ID = 1,
    @ROL_NOMBRE = 'CAJERO',
    @ROL_DESCRIPCION = 'Rol para usuarios de caja',
    @ROL_ES_SISTEMA = 'N',
    @ROL_ACCESO_TOTAL = 'N',
    @ROL_ESTADO = 'A';
```

---

## 7.4. UspActualizaRol

### Objetivo

Actualizar la información de un rol existente.

### Nombre

```sql
dbo.UspActualizaRol
```

### Parámetros de entrada

| Parámetro           |           Tipo | Obligatorio | Descripción                   |
| ------------------- | -------------: | ----------: | ----------------------------- |
| `@ROL_ID`           |          `INT` |          Sí | Identificador del rol.        |
| `@ROL_NEG_ID`       |          `INT` |          No | Negocio asociado al rol.      |
| `@ROL_NOMBRE`       |  `VARCHAR(80)` |          Sí | Nombre del rol.               |
| `@ROL_DESCRIPCION`  | `VARCHAR(250)` |          No | Descripción del rol.          |
| `@ROL_ES_SISTEMA`   |      `CHAR(1)` |          No | Indica si es rol de sistema.  |
| `@ROL_ACCESO_TOTAL` |      `CHAR(1)` |          No | Indica si tiene acceso total. |
| `@ROL_ESTADO`       |      `CHAR(1)` |          No | Estado del rol.               |

### Resultado

| Campo     | Descripción                        |
| --------- | ---------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa.   |
| `Mensaje` | Mensaje descriptivo del resultado. |
| `ROL_ID`  | Identificador del rol actualizado. |

### Ejemplo de uso

```sql
EXEC dbo.UspActualizaRol
    @ROL_ID = 2,
    @ROL_NEG_ID = 1,
    @ROL_NOMBRE = 'CAJERO',
    @ROL_DESCRIPCION = 'Rol operativo para caja',
    @ROL_ES_SISTEMA = 'N',
    @ROL_ACCESO_TOTAL = 'N',
    @ROL_ESTADO = 'A';
```

---

## 7.5. UspEliminaRol

### Objetivo

Inactivar lógicamente un rol.

Este procedimiento no elimina físicamente el registro. Cambia el estado del rol a `I`.

Además, inactiva las asignaciones relacionadas en:

```text
TBL_USUARIO_ROLES
TBL_ROL_PERMISOS
```

### Nombre

```sql
dbo.UspEliminaRol
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                        |
| --------- | ----: | ----------: | ---------------------------------- |
| `@ROL_ID` | `INT` |          Sí | Identificador del rol a inactivar. |

### Validaciones realizadas

1. El identificador del rol es obligatorio.
2. El rol debe existir.
3. No se puede inactivar un rol marcado como rol de sistema.

### Ejemplo de uso

```sql
EXEC dbo.UspEliminaRol
    @ROL_ID = 2;
```

---

# 8. Procedures de Rol-Permisos

---

## 8.1. UspListaRolPermisos

### Objetivo

Listar permisos asignados a roles.

### Nombre

```sql
dbo.UspListaRolPermisos
```

### Parámetros de entrada

| Parámetro        |           Tipo | Obligatorio | Descripción                           |
| ---------------- | -------------: | ----------: | ------------------------------------- |
| `@ROLPER_ROL_ID` |          `INT` |          No | Filtra por rol.                       |
| `@ROLPER_PER_ID` |          `INT` |          No | Filtra por permiso.                   |
| `@PER_NOMBRE`    | `VARCHAR(180)` |          No | Filtra por nombre lógico del permiso. |
| `@ROLPER_ESTADO` |      `CHAR(1)` |          No | Filtra por estado de la asignación.   |

### Ejemplos de uso

Listar todas las asignaciones:

```sql
EXEC dbo.UspListaRolPermisos;
```

Listar permisos de un rol:

```sql
EXEC dbo.UspListaRolPermisos
    @ROLPER_ROL_ID = 1,
    @ROLPER_ESTADO = 'A';
```

Buscar asignación por permiso:

```sql
EXEC dbo.UspListaRolPermisos
    @PER_NOMBRE = 'usuarios.crear';
```

---

## 8.2. UspAsignaPermisoRol

### Objetivo

Asignar un permiso a un rol.

Si la relación rol-permiso ya existe pero está inactiva, el procedure la reactiva.

### Nombre

```sql
dbo.UspAsignaPermisoRol
```

### Parámetros de entrada

| Parámetro                         |  Tipo | Obligatorio | Descripción                        |
| --------------------------------- | ----: | ----------: | ---------------------------------- |
| `@ROLPER_ROL_ID`                  | `INT` |          Sí | Identificador del rol.             |
| `@ROLPER_PER_ID`                  | `INT` |          Sí | Identificador del permiso.         |
| `@ROLPER_ASIGNADO_POR_USUARIO_ID` | `INT` |          No | Usuario que realiza la asignación. |

### Validaciones realizadas

1. El rol es obligatorio.
2. El permiso es obligatorio.
3. El rol debe existir y estar activo.
4. El permiso debe existir y estar activo.
5. Si se envía usuario asignador, debe existir y estar activo.
6. Si el permiso ya está asignado activamente al rol, no se duplica.

### Resultado

| Campo       | Descripción                        |
| ----------- | ---------------------------------- |
| `Exitoso`   | `1` si la operación fue exitosa.   |
| `Mensaje`   | Mensaje descriptivo del resultado. |
| `ROLPER_ID` | Identificador de la asignación.    |

### Ejemplo de uso

```sql
EXEC dbo.UspAsignaPermisoRol
    @ROLPER_ROL_ID = 1,
    @ROLPER_PER_ID = 3,
    @ROLPER_ASIGNADO_POR_USUARIO_ID = 1;
```

---

## 8.3. UspRevocaPermisoRol

### Objetivo

Revocar un permiso asignado a un rol.

No elimina físicamente el registro. Cambia la asignación a estado `I`.

### Nombre

```sql
dbo.UspRevocaPermisoRol
```

### Parámetros de entrada

| Parámetro        |  Tipo | Obligatorio | Descripción                |
| ---------------- | ----: | ----------: | -------------------------- |
| `@ROLPER_ROL_ID` | `INT` |          Sí | Identificador del rol.     |
| `@ROLPER_PER_ID` | `INT` |          Sí | Identificador del permiso. |

### Ejemplo de uso

```sql
EXEC dbo.UspRevocaPermisoRol
    @ROLPER_ROL_ID = 1,
    @ROLPER_PER_ID = 3;
```

---

# 9. Procedures de Usuario-Roles

---

## 9.1. UspListaUsuarioRoles

### Objetivo

Listar roles asignados a usuarios.

### Nombre

```sql
dbo.UspListaUsuarioRoles
```

### Parámetros de entrada

| Parámetro        |          Tipo | Obligatorio | Descripción                         |
| ---------------- | ------------: | ----------: | ----------------------------------- |
| `@USRROL_USR_ID` |         `INT` |          No | Filtra por usuario.                 |
| `@USRROL_ROL_ID` |         `INT` |          No | Filtra por rol.                     |
| `@USR_CORREO`    | `VARCHAR(80)` |          No | Filtra por correo de usuario.       |
| `@ROL_NOMBRE`    | `VARCHAR(80)` |          No | Filtra por nombre del rol.          |
| `@USRROL_ESTADO` |     `CHAR(1)` |          No | Filtra por estado de la asignación. |

### Ejemplos de uso

Listar todas las asignaciones:

```sql
EXEC dbo.UspListaUsuarioRoles;
```

Listar roles de un usuario:

```sql
EXEC dbo.UspListaUsuarioRoles
    @USRROL_USR_ID = 1,
    @USRROL_ESTADO = 'A';
```

Buscar roles por correo:

```sql
EXEC dbo.UspListaUsuarioRoles
    @USR_CORREO = 'admin@correo.com';
```

---

## 9.2. UspAsignaRolUsuario

### Objetivo

Asignar un rol a un usuario.

Si la relación usuario-rol ya existe pero está inactiva, el procedure la reactiva.

### Nombre

```sql
dbo.UspAsignaRolUsuario
```

### Parámetros de entrada

| Parámetro                         |  Tipo | Obligatorio | Descripción                        |
| --------------------------------- | ----: | ----------: | ---------------------------------- |
| `@USRROL_USR_ID`                  | `INT` |          Sí | Identificador del usuario.         |
| `@USRROL_ROL_ID`                  | `INT` |          Sí | Identificador del rol.             |
| `@USRROL_ASIGNADO_POR_USUARIO_ID` | `INT` |          No | Usuario que realiza la asignación. |

### Validaciones realizadas

1. El usuario es obligatorio.
2. El rol es obligatorio.
3. El usuario debe existir y estar activo.
4. El rol debe existir y estar activo.
5. Si el rol pertenece a un negocio, el usuario debe pertenecer al mismo negocio.
6. Si se envía usuario asignador, debe existir y estar activo.
7. Si el rol ya está asignado activamente al usuario, no se duplica.

### Resultado

| Campo       | Descripción                        |
| ----------- | ---------------------------------- |
| `Exitoso`   | `1` si la operación fue exitosa.   |
| `Mensaje`   | Mensaje descriptivo del resultado. |
| `USRROL_ID` | Identificador de la asignación.    |

### Ejemplo de uso

```sql
EXEC dbo.UspAsignaRolUsuario
    @USRROL_USR_ID = 1,
    @USRROL_ROL_ID = 1,
    @USRROL_ASIGNADO_POR_USUARIO_ID = 1;
```

---

## 9.3. UspRevocaRolUsuario

### Objetivo

Revocar un rol asignado a un usuario.

No elimina físicamente el registro. Cambia la asignación a estado `I`.

### Nombre

```sql
dbo.UspRevocaRolUsuario
```

### Parámetros de entrada

| Parámetro        |  Tipo | Obligatorio | Descripción                |
| ---------------- | ----: | ----------: | -------------------------- |
| `@USRROL_USR_ID` | `INT` |          Sí | Identificador del usuario. |
| `@USRROL_ROL_ID` | `INT` |          Sí | Identificador del rol.     |

### Ejemplo de uso

```sql
EXEC dbo.UspRevocaRolUsuario
    @USRROL_USR_ID = 1,
    @USRROL_ROL_ID = 1;
```

---

# 10. Procedure de permisos efectivos de usuario

---

## 10.1. UspListaPermisosUsuario

### Objetivo

Consultar los permisos efectivos de un usuario.

Este procedure será clave para la futura implementación de autorización con JWT, policies, middleware o filtros por permiso.

### Nombre

```sql
dbo.UspListaPermisosUsuario
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                |
| --------- | ----: | ----------: | -------------------------- |
| `@USR_ID` | `INT` |          Sí | Identificador del usuario. |

### Comportamiento

El procedimiento realiza el siguiente flujo:

```text
1. Valida que el usuario exista y esté activo.
2. Consulta los roles activos del usuario.
3. Verifica si alguno de sus roles tiene ROL_ACCESO_TOTAL = 'S'.
4. Si tiene acceso total, retorna todos los permisos activos del sistema.
5. Si no tiene acceso total, retorna solo los permisos activos asignados a sus roles activos.
```

### Campos retornados

| Campo              | Descripción                                      |
| ------------------ | ------------------------------------------------ |
| `USR_ID`           | Identificador del usuario.                       |
| `USR_ACCESO_TOTAL` | Indica si el usuario tiene acceso total por rol. |
| `PER_ID`           | Identificador del permiso.                       |
| `PER_NOMBRE`       | Nombre lógico del permiso.                       |
| `MOD_CODIGO`       | Código técnico del módulo.                       |
| `MOD_NOMBRE`       | Nombre visible del módulo.                       |
| `ACC_CODIGO`       | Código técnico de la acción.                     |
| `ACC_NOMBRE`       | Nombre visible de la acción.                     |
| `PER_DESCRIPCION`  | Descripción del permiso.                         |
| `PER_ESTADO`       | Estado del permiso.                              |

### Ejemplo de uso

```sql
EXEC dbo.UspListaPermisosUsuario
    @USR_ID = 1;
```

### Ejemplo de resultado

```text
USR_ID | USR_ACCESO_TOTAL | PER_ID | PER_NOMBRE
------ | ---------------- | ------ | ----------------
1      | N                | 3      | usuarios.crear
1      | N                | 4      | usuarios.actualizar
1      | N                | 9      | ventas.registrar
```

---

# 11. Reglas de negocio aplicadas

## 11.1. Roles globales y roles por negocio

Un rol puede ser global o pertenecer a un negocio.

```text
Rol global:
ROL_NEG_ID = NULL

Rol por negocio:
ROL_NEG_ID = ID del negocio
```

Un rol global puede ser usado por usuarios de cualquier negocio.

Un rol asociado a negocio solo debe asignarse a usuarios del mismo negocio.

---

## 11.2. Rol de sistema

Un rol con:

```text
ROL_ES_SISTEMA = 'S'
```

representa un rol protegido o propio del sistema.

El procedure `UspEliminaRol` no permite inactivar roles marcados como rol de sistema.

---

## 11.3. Rol con acceso total

Un rol con:

```text
ROL_ACCESO_TOTAL = 'S'
```

permite que el usuario obtenga todos los permisos activos del sistema mediante `UspListaPermisosUsuario`.

Este campo no debe confundirse con `ROL_ES_SISTEMA`.

```text
ROL_ES_SISTEMA:
Indica que el rol es protegido o propio del sistema.

ROL_ACCESO_TOTAL:
Indica que el rol concede todos los permisos.
```

---

## 11.4. Asignaciones sin eliminación física

Las asignaciones de permisos a roles y roles a usuarios no se eliminan físicamente.

Se revocan cambiando su estado a:

```text
I
```

Esto permite conservar trazabilidad básica de relaciones que existieron previamente.

---

## 11.5. Reactivación de asignaciones

Si una relación ya existe pero está inactiva, los procedures de asignación la reactivan.

Aplica para:

```text
UspAsignaPermisoRol
UspAsignaRolUsuario
```

Esto evita duplicar relaciones históricas.

---

## 11.6. Permisos efectivos

Los permisos efectivos de un usuario son el resultado de:

```text
Roles activos del usuario
        ↓
Permisos activos asignados a esos roles
        ↓
Permisos activos del sistema
```

Si el usuario tiene un rol con acceso total, se retornan todos los permisos activos.

---

# 12. Flujo recomendado de prueba

## 12.1. Crear un rol

```sql
EXEC dbo.UspCreaRol
    @ROL_NEG_ID = NULL,
    @ROL_NOMBRE = 'ADMINISTRADOR',
    @ROL_DESCRIPCION = 'Rol administrador del sistema',
    @ROL_ES_SISTEMA = 'S',
    @ROL_ACCESO_TOTAL = 'S',
    @ROL_ESTADO = 'A';
```

---

## 12.2. Listar roles

```sql
EXEC dbo.UspListaRoles
    @ROL_ESTADO = 'A';
```

---

## 12.3. Consultar permisos existentes

```sql
EXEC dbo.UspListaPermisos
    @PER_ESTADO = 'A';
```

---

## 12.4. Asignar permiso a rol

```sql
EXEC dbo.UspAsignaPermisoRol
    @ROLPER_ROL_ID = 1,
    @ROLPER_PER_ID = 1,
    @ROLPER_ASIGNADO_POR_USUARIO_ID = 1;
```

---

## 12.5. Listar permisos del rol

```sql
EXEC dbo.UspListaRolPermisos
    @ROLPER_ROL_ID = 1,
    @ROLPER_ESTADO = 'A';
```

---

## 12.6. Asignar rol a usuario

```sql
EXEC dbo.UspAsignaRolUsuario
    @USRROL_USR_ID = 1,
    @USRROL_ROL_ID = 1,
    @USRROL_ASIGNADO_POR_USUARIO_ID = 1;
```

---

## 12.7. Listar roles del usuario

```sql
EXEC dbo.UspListaUsuarioRoles
    @USRROL_USR_ID = 1,
    @USRROL_ESTADO = 'A';
```

---

## 12.8. Consultar permisos efectivos del usuario

```sql
EXEC dbo.UspListaPermisosUsuario
    @USR_ID = 1;
```

---

# 13. Consideraciones para Backend

Estos procedures serán usados por las capas del backend para completar la autorización.

## 13.1. Uso esperado

```text
Repository
    ↓
Application Service
    ↓
Controller / Auth Service / Authorization Service
```

## 13.2. Uso futuro con JWT

Durante el login, el backend podrá:

```text
1. Validar credenciales del usuario.
2. Consultar roles del usuario.
3. Consultar permisos efectivos con UspListaPermisosUsuario.
4. Generar access token JWT.
5. Guardar refresh token hasheado en TBL_SESIONES.
6. Validar permisos en endpoints protegidos.
```

---

## 13.3. Claims sugeridos

En una fase futura, el JWT podría incluir claims como:

```text
usr_id
usr_nombre
usr_correo
neg_id
roles
permissions
```

Sin embargo, para evitar tokens demasiado grandes, también se puede guardar solo información básica en el JWT y consultar permisos desde caché o base de datos.

---

## 13.4. Redis futuro

Redis puede ser útil para cachear permisos efectivos del usuario.

Ejemplo:

```text
user:1:permissions
```

Esto evitaría consultar SQL Server en cada request protegido.

También puede servir para:

```text
blacklist de tokens revocados
rate limiting de login
cache de sesiones activas
```

---

# 14. Estado actual

## Implementado

```text
Vistas:
- VW_RBAC_ROL_PERMISOS
- VW_RBAC_USUARIO_ROLES

Roles:
- UspListaRoles
- UspConsultaRol
- UspCreaRol
- UspActualizaRol
- UspEliminaRol

Rol-Permisos:
- UspListaRolPermisos
- UspAsignaPermisoRol
- UspRevocaPermisoRol

Usuario-Roles:
- UspListaUsuarioRoles
- UspAsignaRolUsuario
- UspRevocaRolUsuario

Permisos efectivos:
- UspListaPermisosUsuario
```

## Pendiente / futuro

```text
Implementar capa Repository/Application para roles.
Extender UtpPos.Console.Tools para gestionar roles.
Implementar login.
Implementar JWT.
Implementar refresh tokens.
Implementar TBL_SESIONES en backend.
Implementar autorización por permisos.
Evaluar Redis.
```

---

# 15. Notas técnicas

* Ejecutar el script en SSMS o Azure Data Studio sin copiar bloques Markdown.
* No copiar triple backticks dentro de SQL Server.
* Los procedures usan `TRY/CATCH` y transacciones en operaciones de escritura.
* No hay eliminación física para roles ni asignaciones.
* La revocación de permisos y roles se maneja por estado `I`.
* `UspListaPermisosUsuario` será la base para autorización por permisos.
* `ROL_ACCESO_TOTAL = 'S'` concede todos los permisos activos.
* `ROL_ES_SISTEMA = 'S'` protege el rol contra inactivación lógica mediante `UspEliminaRol`.
* Los roles asociados a negocio solo deben asignarse a usuarios del mismo negocio.
* Los roles globales pueden aplicarse a cualquier negocio.
