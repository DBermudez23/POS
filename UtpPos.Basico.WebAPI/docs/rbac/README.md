# README - Módulo RBAC

## 1. Objetivo del módulo RBAC

> Actualización: el núcleo C# de roles, rol-permisos, usuario-roles y permisos efectivos ya está implementado. La arquitectura y los contratos vigentes de esa fase se documentan en [README_RBAC_MANAGEMENT_CORE.md](README_RBAC_MANAGEMENT_CORE.md). Las referencias posteriores de este documento a esas capacidades como "pendientes" describen el alcance histórico de la primera fase de catálogos técnicos.

RBAC significa **Role Based Access Control**, o control de acceso basado en roles. Su objetivo es definir qué capacidades puede ejecutar un usuario dentro del Sistema de Ventas sin asignar permisos individuales de forma arbitraria.

El modelo previsto relaciona:

```text
Usuarios
    ↓
Roles
    ↓
Permisos
    ↓
Módulos + Acciones
```

Las sesiones complementan este modelo al representar el ciclo de autenticación del usuario. El alcance completo contempla:

```text
Usuarios
Roles
Permisos
Módulos
Acciones
Sesiones
```

La primera fase implementada en las capas C# se concentra en los catálogos técnicos de módulos, acciones y permisos.

---

## 2. Conceptos principales

| Concepto | Descripción | Estado actual |
| -------- | ----------- | ------------- |
| Módulo | Área funcional real del sistema, por ejemplo `usuarios`, `productos` o `ventas`. | Implementado para consulta y creación. |
| Acción | Operación técnica reutilizable, por ejemplo `listar`, `crear`, `registrar` o `anular`. | Implementada para consulta y creación. |
| Permiso | Combinación válida entre un módulo y una acción. Su nombre lógico tiene el formato `modulo.accion`. | Implementado para consulta y creación. |
| Rol | Agrupación de permisos que posteriormente puede asignarse a uno o más usuarios. | Pendiente en las capas C# y sin SP documentados en esta fase. |
| Usuario | Identidad interna que puede estar asociada a un negocio y que, en el modelo futuro, recibe roles. | La tabla y sus SP están documentados fuera de la implementación RBAC actual. |
| Sesión | Registro del ciclo de autenticación de un usuario, necesario para controlar vigencia y revocación. | `TBL_SESIONES` está mencionada para una fase posterior; no se documenta todavía su estructura. |
| Access token | Credencial de corta duración enviada al backend para autenticar solicitudes. WebAPI tiene configurada validación JWT Bearer. | La validación está configurada; no se encontró emisión de tokens integrada con RBAC. |
| Refresh token | Credencial destinada a obtener un nuevo access token sin repetir el inicio de sesión. | No implementado todavía y sin estructura documentada en `TBL_SESIONES`. |
| Rol de sistema | Rol técnico administrado como parte de la seguridad base del sistema. | Concepto futuro; no se encontró contrato, columna o SP que defina su comportamiento actual. |
| Rol con acceso total | Rol administrativo que podría autorizar todas las capacidades técnicas permitidas. | La documentación lo menciona como protección posible para endpoints técnicos futuros; no está implementado. |

Los conceptos marcados como pendientes no deben interpretarse como objetos disponibles en Application o Repository.

---

## 3. Tablas del módulo RBAC

La documentación disponible distingue tablas base de la primera fase y tablas relacionadas para fases posteriores.

| Tabla | Responsabilidad documentada | Estado documentado |
| ----- | --------------------------- | ------------------ |
| `TBL_MODULOS` | Catálogo de áreas funcionales del sistema. | Tabla principal de la primera fase. |
| `TBL_ACCIONES` | Catálogo de operaciones técnicas reutilizables. | Tabla principal de la primera fase. |
| `TBL_PERMISOS` | Relación entre un módulo y una acción. | Tabla principal de la primera fase. |
| `TBL_ROLES` | Catálogo futuro de roles. | Relacionada para una fase posterior; no hay estructura ni SP documentados en este repositorio. |
| `TBL_USUARIO_ROLES` | Asignación futura de roles a usuarios. | Relacionada para una fase posterior; no hay estructura ni SP documentados. |
| `TBL_ROL_PERMISOS` | Asignación futura de permisos a roles. | Relacionada para una fase posterior; no hay estructura ni SP documentados. |
| `TBL_SESIONES` | Persistencia futura del ciclo de sesiones. | Relacionada para una fase posterior; no hay estructura ni SP documentados. |
| `TBL_USUARIOS` | Usuarios internos, credenciales hasheadas y asociación opcional a negocio. | Tabla y SP CRUD documentados; todavía no está integrada con roles ni sesiones en las capas RBAC. |

`TBL_NEGOCIO` también aparece como tabla relacionada porque los usuarios pueden asociarse a un negocio, pero no es una tabla propia de RBAC.

## 3.1. Estructuras documentadas en la primera fase

### TBL_MODULOS

```sql
MOD_ID             INT IDENTITY(1,1) PRIMARY KEY
MOD_CODIGO         VARCHAR(80) NOT NULL
MOD_NOMBRE         VARCHAR(120) NOT NULL
MOD_DESCRIPCION    VARCHAR(250) NULL
MOD_ES_SISTEMA     CHAR(1) NOT NULL
MOD_ESTADO         CHAR(1) NOT NULL
MOD_FECHA_CREACION DATETIME NOT NULL
```

### TBL_ACCIONES

```sql
ACC_ID             INT IDENTITY(1,1) PRIMARY KEY
ACC_CODIGO         VARCHAR(80) NOT NULL
ACC_NOMBRE         VARCHAR(120) NOT NULL
ACC_DESCRIPCION    VARCHAR(250) NULL
ACC_ES_SISTEMA     CHAR(1) NOT NULL
ACC_ESTADO         CHAR(1) NOT NULL
ACC_FECHA_CREACION DATETIME NOT NULL
```

### TBL_PERMISOS

```sql
PER_ID             INT IDENTITY(1,1) PRIMARY KEY
PER_MOD_ID         INT NOT NULL
PER_ACC_ID         INT NOT NULL
PER_DESCRIPCION    VARCHAR(250) NULL
PER_ESTADO         CHAR(1) NOT NULL
PER_FECHA_CREACION DATETIME NOT NULL
```

No se incluyen columnas para roles o sesiones porque su estructura no está documentada en el repositorio.

---

# 4. Relación entre módulos, acciones y permisos

Un permiso representa una combinación explícita entre un módulo y una acción:

```text
TBL_MODULOS + TBL_ACCIONES = TBL_PERMISOS
```

Las relaciones físicas documentadas son:

```sql
TBL_PERMISOS.PER_MOD_ID -> TBL_MODULOS.MOD_ID
TBL_PERMISOS.PER_ACC_ID -> TBL_ACCIONES.ACC_ID
```

Ejemplo:

```text
Módulo: usuarios
Acción: crear
Permiso: usuarios.crear
```

El valor `usuarios.crear` no se almacena en una columna física `PER_NOMBRE` de `TBL_PERMISOS`. Se genera al consultar la vista `VW_RBAC_PERMISOS`:

```text
PER_NOMBRE = MOD_CODIGO + '.' + ACC_CODIGO
```

Esto mantiene el nombre lógico sincronizado con los códigos técnicos del módulo y la acción.

---

# 5. Vista RBAC

La vista actual es:

```sql
dbo.VW_RBAC_PERMISOS
```

La vista une:

```text
TBL_PERMISOS
TBL_MODULOS
TBL_ACCIONES
```

Los campos documentados son:

| Campo | Descripción |
| ----- | ----------- |
| `PER_ID` | Identificador del permiso. |
| `PER_MOD_ID` | Identificador del módulo asociado. |
| `MOD_CODIGO` | Código técnico del módulo. |
| `MOD_NOMBRE` | Nombre visible del módulo. |
| `PER_ACC_ID` | Identificador de la acción asociada. |
| `ACC_CODIGO` | Código técnico de la acción. |
| `ACC_NOMBRE` | Nombre visible de la acción. |
| `PER_NOMBRE` | Nombre lógico generado como `modulo.accion`. |
| `PER_DESCRIPCION` | Descripción del permiso. |
| `PER_ESTADO` | Estado del permiso. |
| `PER_FECHA_CREACION` | Fecha de creación del permiso. |

`UspListaPermisos` utiliza esta representación enriquecida para retornar el módulo, la acción y el nombre lógico del permiso.

---

# 6. Stored Procedures actuales

La primera fase usa exclusivamente los siguientes stored procedures:

| Stored Procedure | Responsabilidad |
| ---------------- | --------------- |
| `dbo.UspListaModulos` | Listar módulos con filtros opcionales por código, nombre y estado. |
| `dbo.UspCreaModulo` | Crear un módulo técnico y retornar `Exitoso`, `Mensaje` y `MOD_ID`. |
| `dbo.UspListaAcciones` | Listar acciones con filtros opcionales por código, nombre y estado. |
| `dbo.UspCreaAccion` | Crear una acción técnica y retornar `Exitoso`, `Mensaje` y `ACC_ID`. |
| `dbo.UspListaPermisos` | Listar permisos con filtros opcionales por nombre lógico, módulo, acción y estado. |
| `dbo.UspCreaPermiso` | Crear una relación módulo-acción y retornar `Exitoso`, `Mensaje`, `PER_ID` y `PER_NOMBRE`. |

No se encontraron SP de roles, asignación de roles, asignación de permisos a roles ni sesiones en la documentación actual.

---

# 7. Flujo de creación RBAC

El flujo funcional completo previsto es:

```text
1. Crear módulos.
2. Crear acciones.
3. Crear permisos combinando módulo + acción.
4. Crear roles.
5. Asignar permisos a roles.
6. Asignar roles a usuarios.
7. Validar permisos desde el backend.
```

Estado por etapa:

| Etapa | Estado |
| ----- | ------ |
| Módulos | Implementada en base de datos, Repository, Application y Console.Tools. |
| Acciones | Implementada en base de datos, Repository, Application y Console.Tools. |
| Permisos | Implementada en base de datos, Repository, Application y Console.Tools. |
| Roles | Pendiente. |
| Permisos por rol | Pendiente. |
| Roles por usuario | Pendiente. |
| Validación de permisos en endpoints | Pendiente. |

No deben generarse todas las combinaciones posibles entre módulos y acciones. Solo deben crearse permisos que correspondan a validaciones reales del backend.

---

# 8. Arquitectura en capas

El módulo se integra en la solución mediante las siguientes capas:

| Capa o proyecto | Responsabilidad RBAC |
| ---------------- | -------------------- |
| `UtpPos.Framework.Domain.Entity` | Define `RbacModule`, `RbacAction` y `RbacPermission` como entidades limpias de dominio. |
| `UtpPos.Framework.Application.DTO` | Define DTOs de consulta, filtros, creación y resultado para los tres catálogos. |
| `UtpPos.Framework.Application.Interface` | Expone el contrato `IRbacApplication`. |
| `UtpPos.Framework.Application.Main` | Implementa `RbacApplication`, normaliza datos, valida existencia y retorna `Response<T>`. |
| `UtpPos.Framework.InfraStructure.Interface` | Expone el contrato `IRbacRepository`. |
| `UtpPos.Framework.InfraStructure.Repository` | Implementa `RbacRepository`, usa Dapper y ejecuta los stored procedures RBAC. |
| `UtpPos.Framework.Transversal.Mapper` | Configura los mappings entre entidades y DTOs. |
| `UtpPos.Console.Tools` | Proporciona la interfaz técnica interactiva para listar y crear módulos, acciones y permisos. |
| `UtpPos.WebAPI` | No expone endpoints RBAC actualmente. Su integración de autorización por permisos queda para una fase futura. |

El flujo implementado es:

```text
Console.Tools
    -> IRbacApplication / RbacApplication
        -> IRbacRepository / RbacRepository
            -> Dapper
                -> Stored Procedure
                    -> SQL Server
```

---

# 9. Entidades y DTOs

## 9.1. Entidades

| Entidad | Información representada |
| ------- | ------------------------ |
| `RbacModule` | ID, código, nombre, descripción, indicador de sistema, estado y fecha de creación. |
| `RbacAction` | ID, código, nombre, descripción, indicador de sistema, estado y fecha de creación. |
| `RbacPermission` | ID, módulo, acción, nombre lógico, descripción, estado y fecha de creación. |

Las entidades usan nombres de propiedades C# y no exponen los nombres físicos `MOD_*`, `ACC_*` o `PER_*`.

## 9.2. DTOs

| Grupo | DTOs existentes |
| ----- | --------------- |
| Módulos | `RbacModuleDto`, `RbacModuleFilterDto`, `RbacModuleCreateDto` |
| Acciones | `RbacActionDto`, `RbacActionFilterDto`, `RbacActionCreateDto` |
| Permisos | `RbacPermissionDto`, `RbacPermissionFilterDto`, `RbacPermissionCreateDto`, `RbacPermissionCreateResultDto` |

Los DTOs de filtro permiten valores nulos. Los DTOs de creación declaran los datos obligatorios y sus longitudes mediante Data Annotations.

Los mappings existentes son:

```text
RbacModule <-> RbacModuleDto
RbacModuleCreateDto -> RbacModule

RbacAction <-> RbacActionDto
RbacActionCreateDto -> RbacAction

RbacPermission <-> RbacPermissionDto
RbacPermissionCreateDto -> RbacPermission
```

---

# 10. Repositorio y Application

## 10.1. IRbacRepository y RbacRepository

`IRbacRepository` define las operaciones de listado y creación. `RbacRepository`:

* inyecta `IConnectionFactory`;
* crea una conexión por operación;
* usa `DynamicParameters` con los nombres SQL documentados;
* ejecuta todos los comandos como `CommandType.StoredProcedure`;
* mapea modelos internos de base de datos a entidades de dominio;
* interpreta `Exitoso`, `Mensaje` y los identificadores retornados;
* genera `InvalidOperationException` con el mensaje del SP cuando `Exitoso` no es `1`.

Los modelos con columnas `MOD_*`, `ACC_*` y `PER_*` son privados al repositorio.

## 10.2. IRbacApplication y RbacApplication

`IRbacApplication` retorna `Response<T>` para todas las operaciones. `RbacApplication`:

* normaliza códigos a minúscula y estados a mayúscula;
* valida campos obligatorios, longitudes y caracteres admitidos;
* verifica duplicados antes de crear;
* comprueba que módulo y acción estén activos antes de crear un permiso;
* usa AutoMapper para convertir DTOs y entidades;
* captura errores de Repository y los retorna mediante `Response<T>`.

`UtpPos.Console.Tools` solo consume `IRbacApplication` y no accede directamente a Repository ni a SQL.

---

# 11. Validaciones de existencia

No existen ni se utilizan stored procedures especiales como:

```text
UspValidaModulo
UspValidaAccion
UspValidaPermiso
```

La validación se realiza mediante los SP de listado:

| Registro | Validación previa | Escritura posterior |
| -------- | ----------------- | ------------------- |
| Módulo | `UspListaModulos` por `MOD_CODIGO`, sin filtro de estado. | `UspCreaModulo` |
| Acción | `UspListaAcciones` por `ACC_CODIGO`, sin filtro de estado. | `UspCreaAccion` |
| Permiso | `UspListaPermisos` por `MOD_CODIGO` y `ACC_CODIGO`, sin filtro de estado. | `UspCreaPermiso` |

Un registro inactivo también bloquea la creación de un duplicado. Esta validación se ejecuta en la consola para informar al operador y vuelve a ejecutarse en Application para proteger el caso de otros consumidores del contrato.

Los SP de escritura conservan su propia validación y actúan como última defensa ante concurrencia.

---

# 12. Reglas de negocio

* Un módulo representa un área funcional real del sistema.
* Una acción representa una operación técnica reutilizable.
* Un permiso representa una combinación válida entre un módulo y una acción.
* No se deben crear permisos que el backend no vaya a validar.
* No se deben crear módulos, acciones ni permisos duplicados.
* Los códigos técnicos se normalizan a minúscula.
* Los códigos solo admiten letras minúsculas, números, guion bajo y guion medio.
* El estado activo se representa con `A`.
* El estado inactivo se representa con `I`.
* Los elementos marcados como parte del sistema usan `S` en `MOD_ES_SISTEMA` o `ACC_ES_SISTEMA`.
* La creación técnica actual usa estado `A`.
* Un permiso solo puede crearse con un módulo activo y una acción activa existentes.
* `PER_NOMBRE` es un valor derivado; no debe agregarse como columna física a `TBL_PERMISOS`.
* El usuario administrador funcional podrá gestionar roles y asignaciones en una fase posterior, pero no debería crear permisos arbitrarios.

---

# 13. Seguridad y sesiones

## 13.1. Usuarios y contraseñas

La documentación de `TBL_USUARIOS` establece que `USR_PASSWORD` debe almacenar únicamente el hash generado por el backend. Los SP no deben recibir ni procesar contraseñas en texto plano, y los listados no retornan `USR_PASSWORD`.

La implementación RBAC actual no asigna roles ni permisos a estos usuarios.

## 13.2. Access token JWT

`UtpPos.WebAPI/Program.cs` registra autenticación JWT Bearer y valida firma, emisor, audiencia y vigencia. También informa expiración mediante el encabezado `Token-Expired`.

No se encontró en el módulo RBAC actual un flujo que emita access tokens, incorpore permisos como claims o resuelva permisos desde roles.

## 13.3. TBL_SESIONES y refresh token

`TBL_SESIONES` aparece únicamente como tabla relacionada para una fase posterior. No se encontró en el repositorio:

```text
- Estructura documentada de TBL_SESIONES.
- Stored procedures de sesiones.
- Persistencia o rotación de refresh tokens.
- Revocación de sesiones.
- Refresh token hasheado.
```

Por tanto, estas capacidades están **no implementadas todavía**.

Como criterio futuro de seguridad, si se incorporan refresh tokens, la persistencia debería guardar una representación hasheada y permitir revocación y rotación. Esta es una recomendación futura, no una descripción de funcionalidad existente.

---

# 14. Integración futura con WebAPI

En una fase futura, WebAPI podrá proteger operaciones mediante permisos técnicos como:

```text
usuarios.crear
productos.listar
ventas.registrar
ventas.anular
```

La integración requerirá, como mínimo:

1. Resolver los roles activos del usuario.
2. Resolver los permisos activos asociados a esos roles.
3. Incorporar o consultar el contexto de autorización durante cada solicitud.
4. Aplicar filtros, policies, atributos o middleware de autorización por permiso.
5. Definir el comportamiento de usuarios o roles con acceso total.
6. Administrar expiración y revocación de sesiones.

Todo lo anterior está pendiente. `UtpPos.Console.Tools` es una herramienta de administración de catálogos técnicos y no es el mecanismo final de autenticación ni autorización HTTP.

Si se crean endpoints para administrar RBAC, deben estar protegidos y mantener el flujo Application -> Repository -> Stored Procedure. No deben permitir permisos arbitrarios al usuario final.

---

# 15. Estado actual del módulo

## 15.1. Implementado actualmente

* Tablas base documentadas para módulos, acciones y permisos.
* Vista `dbo.VW_RBAC_PERMISOS`.
* SP de listado y creación de módulos.
* SP de listado y creación de acciones.
* SP de listado y creación de permisos.
* Entidades, DTOs, contratos, mappings, Application y Repository RBAC.
* Validación de existencia mediante los SP de listado.
* Consola técnica `UtpPos.Console.Tools`.
* Configuración de validación JWT Bearer en WebAPI, independiente todavía del catálogo RBAC.
* Tabla y SP de usuarios documentados, todavía sin integración con roles RBAC.

## 15.2. Pendiente / futuro

* Gestión completa de `TBL_ROLES`.
* Asignación de permisos mediante `TBL_ROL_PERMISOS`.
* Asignación de roles mediante `TBL_USUARIO_ROLES`.
* Integración entre usuarios y RBAC en Application/Repository.
* Estructura y SP de `TBL_SESIONES`.
* Flujo de login que emita JWT.
* Emisión, persistencia hasheada, rotación y revocación de refresh tokens.
* Inclusión o resolución de permisos durante la autenticación.
* Policies, filtros o middleware de autorización por permiso.
* Endpoints WebAPI RBAC, si se requieren.
* Gestión funcional de roles y asignaciones.

## 15.3. Documentación relacionada

```text
docs/database/procedures/README_RBAC_PROCEDURES.md
docs/database/procedures/README_TBL_USUARIOS.md
docs/database/params/README_PARAMETRIZACION_INICIAL.md
docs/console-tools/README_RBAC_TOOL.md
```
