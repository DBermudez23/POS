# README - Procedures y Vista de RBAC

## 1. Objetivo

Este documento describe la vista y los procedimientos almacenados creados para la gestión técnica del sistema RBAC dentro de la base de datos del Sistema de Ventas.

RBAC significa **Role Based Access Control**, o control de acceso basado en roles. El objetivo de este módulo es permitir que el sistema pueda definir permisos técnicos, agruparlos en roles y posteriormente asignar esos roles a usuarios.

En esta primera fase se documentan los objetos técnicos relacionados con:

```text
Módulos
Acciones
Permisos
```

Los módulos representan áreas funcionales del sistema, por ejemplo:

```text
usuarios
productos
ventas
compras
inventario
kardex
```

Las acciones representan operaciones reutilizables, por ejemplo:

```text
listar
consultar
crear
actualizar
eliminar
registrar
anular
asignar
revocar
exportar
```

Los permisos representan la combinación entre un módulo y una acción.

Ejemplo:

```text
usuarios.crear
productos.listar
ventas.registrar
ventas.anular
```

El nombre lógico del permiso no se almacena físicamente en la tabla `TBL_PERMISOS`. Se genera mediante la vista `VW_RBAC_PERMISOS` concatenando:

```text
MOD_CODIGO + '.' + ACC_CODIGO
```

---

## 2. Tablas relacionadas

### Tablas principales

```sql
TBL_MODULOS
TBL_ACCIONES
TBL_PERMISOS
```

### Tablas relacionadas para fases posteriores

```sql
TBL_ROLES
TBL_USUARIO_ROLES
TBL_ROL_PERMISOS
TBL_SESIONES
TBL_USUARIOS
TBL_NEGOCIO
```

---

## 3. Estructura base

### 3.1. TBL_MODULOS

```sql
MOD_ID             INT IDENTITY(1,1) PRIMARY KEY
MOD_CODIGO         VARCHAR(80) NOT NULL
MOD_NOMBRE         VARCHAR(120) NOT NULL
MOD_DESCRIPCION    VARCHAR(250) NULL
MOD_ES_SISTEMA     CHAR(1) NOT NULL
MOD_ESTADO         CHAR(1) NOT NULL
MOD_FECHA_CREACION DATETIME NOT NULL
```

### 3.2. TBL_ACCIONES

```sql
ACC_ID             INT IDENTITY(1,1) PRIMARY KEY
ACC_CODIGO         VARCHAR(80) NOT NULL
ACC_NOMBRE         VARCHAR(120) NOT NULL
ACC_DESCRIPCION    VARCHAR(250) NULL
ACC_ES_SISTEMA     CHAR(1) NOT NULL
ACC_ESTADO         CHAR(1) NOT NULL
ACC_FECHA_CREACION DATETIME NOT NULL
```

### 3.3. TBL_PERMISOS

```sql
PER_ID             INT IDENTITY(1,1) PRIMARY KEY
PER_MOD_ID         INT NOT NULL
PER_ACC_ID         INT NOT NULL
PER_DESCRIPCION    VARCHAR(250) NULL
PER_ESTADO         CHAR(1) NOT NULL
PER_FECHA_CREACION DATETIME NOT NULL
```

---

## 4. Relaciones principales

```sql
TBL_PERMISOS.PER_MOD_ID -> TBL_MODULOS.MOD_ID
TBL_PERMISOS.PER_ACC_ID -> TBL_ACCIONES.ACC_ID
```

Estas relaciones permiten que un permiso solo pueda existir si el módulo y la acción existen previamente.

Ejemplo:

```text
Módulo: usuarios
Acción: crear
Permiso lógico: usuarios.crear
```

---

## 5. Vista creada

Para el módulo RBAC se creó la siguiente vista:

```text
VW_RBAC_PERMISOS
```

Esta vista permite consultar los permisos ya enriquecidos con la información del módulo, la acción y el nombre lógico del permiso.

---

## 6. Procedures creados

Para la gestión técnica de módulos, acciones y permisos se crearon los siguientes procedimientos almacenados:

```text
UspListaModulos
UspCreaModulo
UspListaAcciones
UspCreaAccion
UspListaPermisos
UspCreaPermiso
```

Estos procedures están pensados para ser usados por herramientas internas, scripts técnicos, consola administrativa o procesos de seed.

No se recomienda exponer libremente la creación de módulos, acciones o permisos al usuario final, ya que estos elementos representan capacidades reales del sistema y deben estar alineados con el backend.

---

# 7. Consideraciones generales

* Los módulos son catálogos técnicos del sistema.
* Las acciones son catálogos técnicos reutilizables.
* Los permisos se crean combinando un módulo y una acción.
* El permiso lógico se representa como `modulo.accion`.
* `PER_NOMBRE` no existe como columna física en `TBL_PERMISOS`.
* `PER_NOMBRE` se genera desde la vista `VW_RBAC_PERMISOS`.
* No se deben crear permisos que el backend no vaya a validar.
* Los módulos, acciones y permisos deben ser creados por el desarrollador, mediante script SQL, seed, migración futura o consola técnica.
* El usuario administrador funcional podrá crear roles y asignar permisos existentes a roles, pero no debería crear permisos arbitrarios.
* Los procedures de escritura usan `TRY/CATCH`.
* Los procedures de escritura usan transacciones.
* Los procedures de escritura retornan una estructura estándar con `Exitoso`, `Mensaje` y el ID de la entidad creada.
* Los códigos técnicos se normalizan a minúscula.
* Los códigos técnicos solo permiten letras minúsculas, números, guion bajo y guion medio.
* Los estados válidos son:

  * `A`: Activo.
  * `I`: Inactivo.
* Los campos `MOD_ES_SISTEMA` y `ACC_ES_SISTEMA` usan:

  * `S`: Elemento del sistema.
  * `N`: Elemento no marcado como sistema.

---

# 8. Detalle de la vista

---

## 8.1. VW_RBAC_PERMISOS

### Objetivo

Consultar los permisos del sistema junto con la información descriptiva del módulo y la acción asociada.

Esta vista genera el nombre lógico del permiso como:

```text
MOD_CODIGO + '.' + ACC_CODIGO
```

### Nombre

```sql
dbo.VW_RBAC_PERMISOS
```

### Tablas utilizadas

```sql
TBL_PERMISOS
TBL_MODULOS
TBL_ACCIONES
```

### Campos retornados

| Campo                | Descripción                                              |
| -------------------- | -------------------------------------------------------- |
| `PER_ID`             | Identificador del permiso.                               |
| `PER_MOD_ID`         | Identificador del módulo asociado.                       |
| `MOD_CODIGO`         | Código técnico del módulo.                               |
| `MOD_NOMBRE`         | Nombre visible del módulo.                               |
| `PER_ACC_ID`         | Identificador de la acción asociada.                     |
| `ACC_CODIGO`         | Código técnico de la acción.                             |
| `ACC_NOMBRE`         | Nombre visible de la acción.                             |
| `PER_NOMBRE`         | Nombre lógico del permiso generado como `modulo.accion`. |
| `PER_DESCRIPCION`    | Descripción del permiso.                                 |
| `PER_ESTADO`         | Estado del permiso.                                      |
| `PER_FECHA_CREACION` | Fecha de creación del permiso.                           |

### Ejemplo de uso

```sql
SELECT
    PER_ID,
    PER_NOMBRE,
    MOD_CODIGO,
    ACC_CODIGO,
    PER_DESCRIPCION,
    PER_ESTADO
FROM dbo.VW_RBAC_PERMISOS
ORDER BY
    MOD_CODIGO,
    ACC_CODIGO;
```

### Ejemplo de resultado

```text
PER_ID | PER_NOMBRE        | MOD_CODIGO | ACC_CODIGO | PER_ESTADO
------ | ----------------- | ---------- | ---------- | ----------
1      | usuarios.listar   | usuarios   | listar     | A
2      | usuarios.crear    | usuarios   | crear      | A
3      | ventas.registrar  | ventas     | registrar  | A
```

---

# 9. Detalle de procedures

---

## 9.1. UspListaModulos

### Objetivo

Listar los módulos técnicos registrados en el sistema, permitiendo aplicar filtros opcionales por código, nombre y estado.

### Nombre

```sql
dbo.UspListaModulos
```

### Parámetros de entrada

| Parámetro     |           Tipo | Obligatorio | Descripción                                 |
| ------------- | -------------: | ----------: | ------------------------------------------- |
| `@MOD_CODIGO` |  `VARCHAR(80)` |          No | Permite filtrar módulos por código técnico. |
| `@MOD_NOMBRE` | `VARCHAR(120)` |          No | Permite filtrar módulos por nombre visible. |
| `@MOD_ESTADO` |      `CHAR(1)` |          No | Permite filtrar por estado: `A` o `I`.      |

### Resultado

Retorna un listado de módulos registrados.

### Campos retornados

| Campo                | Descripción                         |
| -------------------- | ----------------------------------- |
| `MOD_ID`             | Identificador del módulo.           |
| `MOD_CODIGO`         | Código técnico del módulo.          |
| `MOD_NOMBRE`         | Nombre visible del módulo.          |
| `MOD_DESCRIPCION`    | Descripción del módulo.             |
| `MOD_ES_SISTEMA`     | Indica si el módulo es del sistema. |
| `MOD_ESTADO`         | Estado del módulo.                  |
| `MOD_FECHA_CREACION` | Fecha de creación del módulo.       |

### Ejemplos de uso

Listar todos los módulos:

```sql
EXEC dbo.UspListaModulos;
```

Listar módulos activos:

```sql
EXEC dbo.UspListaModulos
    @MOD_ESTADO = 'A';
```

Buscar módulo por código:

```sql
EXEC dbo.UspListaModulos
    @MOD_CODIGO = 'usuarios';
```

---

## 9.2. UspCreaModulo

### Objetivo

Crear un nuevo módulo técnico del sistema.

Los módulos representan áreas funcionales del software. Ejemplos:

```text
usuarios
roles
permisos
productos
ventas
compras
inventario
kardex
```

### Nombre

```sql
dbo.UspCreaModulo
```

### Parámetros de entrada

| Parámetro          |           Tipo | Obligatorio | Descripción                                             |
| ------------------ | -------------: | ----------: | ------------------------------------------------------- |
| `@MOD_CODIGO`      |  `VARCHAR(80)` |          Sí | Código técnico del módulo. Debe ir en minúscula.        |
| `@MOD_NOMBRE`      | `VARCHAR(120)` |          Sí | Nombre visible del módulo.                              |
| `@MOD_DESCRIPCION` | `VARCHAR(250)` |          No | Descripción funcional o técnica del módulo.             |
| `@MOD_ES_SISTEMA`  |      `CHAR(1)` |          No | Indica si el módulo es del sistema. Valores: `S` o `N`. |
| `@MOD_ESTADO`      |      `CHAR(1)` |          No | Estado del módulo. Valores: `A` o `I`.                  |

### Validaciones realizadas

El procedimiento valida que:

1. El código del módulo sea obligatorio.
2. El nombre del módulo sea obligatorio.
3. El código solo contenga letras minúsculas, números, guion bajo o guion medio.
4. `MOD_ES_SISTEMA` sea `S` o `N`.
5. `MOD_ESTADO` sea `A` o `I`.
6. No exista otro módulo con el mismo código.

### Resultado

Retorna una respuesta estándar.

| Campo     | Descripción                                    |
| --------- | ---------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si falló. |
| `Mensaje` | Mensaje descriptivo del resultado.             |
| `MOD_ID`  | Identificador del módulo creado.               |

### Ejemplo de uso

```sql
EXEC dbo.UspCreaModulo
    @MOD_CODIGO = 'usuarios',
    @MOD_NOMBRE = 'Usuarios',
    @MOD_DESCRIPCION = 'Gestión de usuarios del sistema',
    @MOD_ES_SISTEMA = 'S',
    @MOD_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                       | MOD_ID
------- | ----------------------------- | ------
1       | Módulo creado correctamente.   | 1
```

---

## 9.3. UspListaAcciones

### Objetivo

Listar las acciones técnicas registradas en el sistema, permitiendo aplicar filtros opcionales por código, nombre y estado.

### Nombre

```sql
dbo.UspListaAcciones
```

### Parámetros de entrada

| Parámetro     |           Tipo | Obligatorio | Descripción                                  |
| ------------- | -------------: | ----------: | -------------------------------------------- |
| `@ACC_CODIGO` |  `VARCHAR(80)` |          No | Permite filtrar acciones por código técnico. |
| `@ACC_NOMBRE` | `VARCHAR(120)` |          No | Permite filtrar acciones por nombre visible. |
| `@ACC_ESTADO` |      `CHAR(1)` |          No | Permite filtrar por estado: `A` o `I`.       |

### Resultado

Retorna un listado de acciones registradas.

### Campos retornados

| Campo                | Descripción                         |
| -------------------- | ----------------------------------- |
| `ACC_ID`             | Identificador de la acción.         |
| `ACC_CODIGO`         | Código técnico de la acción.        |
| `ACC_NOMBRE`         | Nombre visible de la acción.        |
| `ACC_DESCRIPCION`    | Descripción de la acción.           |
| `ACC_ES_SISTEMA`     | Indica si la acción es del sistema. |
| `ACC_ESTADO`         | Estado de la acción.                |
| `ACC_FECHA_CREACION` | Fecha de creación de la acción.     |

### Ejemplos de uso

Listar todas las acciones:

```sql
EXEC dbo.UspListaAcciones;
```

Listar acciones activas:

```sql
EXEC dbo.UspListaAcciones
    @ACC_ESTADO = 'A';
```

Buscar acción por código:

```sql
EXEC dbo.UspListaAcciones
    @ACC_CODIGO = 'crear';
```

---

## 9.4. UspCreaAccion

### Objetivo

Crear una nueva acción técnica reutilizable en el sistema.

Las acciones representan operaciones que pueden aplicarse sobre diferentes módulos. Ejemplos:

```text
listar
consultar
crear
actualizar
eliminar
registrar
anular
asignar
revocar
exportar
```

### Nombre

```sql
dbo.UspCreaAccion
```

### Parámetros de entrada

| Parámetro          |           Tipo | Obligatorio | Descripción                                             |
| ------------------ | -------------: | ----------: | ------------------------------------------------------- |
| `@ACC_CODIGO`      |  `VARCHAR(80)` |          Sí | Código técnico de la acción. Debe ir en minúscula.      |
| `@ACC_NOMBRE`      | `VARCHAR(120)` |          Sí | Nombre visible de la acción.                            |
| `@ACC_DESCRIPCION` | `VARCHAR(250)` |          No | Descripción funcional o técnica de la acción.           |
| `@ACC_ES_SISTEMA`  |      `CHAR(1)` |          No | Indica si la acción es del sistema. Valores: `S` o `N`. |
| `@ACC_ESTADO`      |      `CHAR(1)` |          No | Estado de la acción. Valores: `A` o `I`.                |

### Validaciones realizadas

El procedimiento valida que:

1. El código de la acción sea obligatorio.
2. El nombre de la acción sea obligatorio.
3. El código solo contenga letras minúsculas, números, guion bajo o guion medio.
4. `ACC_ES_SISTEMA` sea `S` o `N`.
5. `ACC_ESTADO` sea `A` o `I`.
6. No exista otra acción con el mismo código.

### Resultado

Retorna una respuesta estándar.

| Campo     | Descripción                                    |
| --------- | ---------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si falló. |
| `Mensaje` | Mensaje descriptivo del resultado.             |
| `ACC_ID`  | Identificador de la acción creada.             |

### Ejemplo de uso

```sql
EXEC dbo.UspCreaAccion
    @ACC_CODIGO = 'crear',
    @ACC_NOMBRE = 'Crear',
    @ACC_DESCRIPCION = 'Permite crear registros',
    @ACC_ES_SISTEMA = 'S',
    @ACC_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                       | ACC_ID
------- | ----------------------------- | ------
1       | Acción creada correctamente.   | 1
```

---

## 9.5. UspListaPermisos

### Objetivo

Listar los permisos técnicos registrados en el sistema.

Permite consultar permisos filtrando por nombre lógico, módulo, acción y estado.

El nombre lógico del permiso se obtiene desde la vista `VW_RBAC_PERMISOS` y corresponde a:

```text
modulo.accion
```

### Nombre

```sql
dbo.UspListaPermisos
```

### Parámetros de entrada

| Parámetro     |           Tipo | Obligatorio | Descripción                                    |
| ------------- | -------------: | ----------: | ---------------------------------------------- |
| `@PER_NOMBRE` | `VARCHAR(180)` |          No | Permite filtrar por nombre lógico del permiso. |
| `@MOD_CODIGO` |  `VARCHAR(80)` |          No | Permite filtrar permisos por código de módulo. |
| `@ACC_CODIGO` |  `VARCHAR(80)` |          No | Permite filtrar permisos por código de acción. |
| `@PER_ESTADO` |      `CHAR(1)` |          No | Permite filtrar por estado: `A` o `I`.         |

### Resultado

Retorna un listado de permisos junto con el módulo y acción asociados.

### Campos retornados

| Campo                | Descripción                                              |
| -------------------- | -------------------------------------------------------- |
| `PER_ID`             | Identificador del permiso.                               |
| `PER_MOD_ID`         | Identificador del módulo asociado.                       |
| `MOD_CODIGO`         | Código técnico del módulo.                               |
| `MOD_NOMBRE`         | Nombre visible del módulo.                               |
| `PER_ACC_ID`         | Identificador de la acción asociada.                     |
| `ACC_CODIGO`         | Código técnico de la acción.                             |
| `ACC_NOMBRE`         | Nombre visible de la acción.                             |
| `PER_NOMBRE`         | Nombre lógico del permiso generado como `modulo.accion`. |
| `PER_DESCRIPCION`    | Descripción del permiso.                                 |
| `PER_ESTADO`         | Estado del permiso.                                      |
| `PER_FECHA_CREACION` | Fecha de creación del permiso.                           |

### Ejemplos de uso

Listar todos los permisos:

```sql
EXEC dbo.UspListaPermisos;
```

Listar permisos activos:

```sql
EXEC dbo.UspListaPermisos
    @PER_ESTADO = 'A';
```

Listar permisos de un módulo:

```sql
EXEC dbo.UspListaPermisos
    @MOD_CODIGO = 'usuarios';
```

Buscar un permiso específico:

```sql
EXEC dbo.UspListaPermisos
    @PER_NOMBRE = 'usuarios.crear';
```

---

## 9.6. UspCreaPermiso

### Objetivo

Crear un nuevo permiso técnico asociando un módulo existente con una acción existente.

El permiso creado no almacena físicamente un nombre como `usuarios.crear`. Ese nombre se genera desde la vista `VW_RBAC_PERMISOS`.

### Nombre

```sql
dbo.UspCreaPermiso
```

### Parámetros de entrada

| Parámetro          |           Tipo | Obligatorio | Descripción                                     |
| ------------------ | -------------: | ----------: | ----------------------------------------------- |
| `@PER_MOD_ID`      |          `INT` |          Sí | Identificador del módulo asociado al permiso.   |
| `@PER_ACC_ID`      |          `INT` |          Sí | Identificador de la acción asociada al permiso. |
| `@PER_DESCRIPCION` | `VARCHAR(250)` |          No | Descripción funcional o técnica del permiso.    |
| `@PER_ESTADO`      |      `CHAR(1)` |          No | Estado del permiso. Valores: `A` o `I`.         |

### Validaciones realizadas

El procedimiento valida que:

1. El módulo sea obligatorio.
2. La acción sea obligatoria.
3. `PER_ESTADO` sea `A` o `I`.
4. El módulo exista y esté activo.
5. La acción exista y esté activa.
6. No exista previamente un permiso con el mismo módulo y la misma acción.

### Resultado

Retorna una respuesta estándar.

| Campo        | Descripción                                    |
| ------------ | ---------------------------------------------- |
| `Exitoso`    | `1` si la operación fue exitosa, `0` si falló. |
| `Mensaje`    | Mensaje descriptivo del resultado.             |
| `PER_ID`     | Identificador del permiso creado.              |
| `PER_NOMBRE` | Nombre lógico generado desde módulo y acción.  |

### Ejemplo de uso

```sql
EXEC dbo.UspCreaPermiso
    @PER_MOD_ID = 1,
    @PER_ACC_ID = 1,
    @PER_DESCRIPCION = 'Permite crear usuarios',
    @PER_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                         | PER_ID | PER_NOMBRE
------- | ------------------------------- | ------ | ----------------
1       | Permiso creado correctamente.    | 1      | usuarios.crear
```

---

# 10. Flujo recomendado de prueba

Para validar el correcto funcionamiento de la vista y los procedures, se recomienda ejecutar el siguiente flujo.

## 10.1. Crear acciones base

```sql
EXEC dbo.UspCreaAccion
    @ACC_CODIGO = 'listar',
    @ACC_NOMBRE = 'Listar',
    @ACC_DESCRIPCION = 'Permite listar registros';

EXEC dbo.UspCreaAccion
    @ACC_CODIGO = 'consultar',
    @ACC_NOMBRE = 'Consultar',
    @ACC_DESCRIPCION = 'Permite consultar un registro específico';

EXEC dbo.UspCreaAccion
    @ACC_CODIGO = 'crear',
    @ACC_NOMBRE = 'Crear',
    @ACC_DESCRIPCION = 'Permite crear registros';

EXEC dbo.UspCreaAccion
    @ACC_CODIGO = 'actualizar',
    @ACC_NOMBRE = 'Actualizar',
    @ACC_DESCRIPCION = 'Permite actualizar registros';

EXEC dbo.UspCreaAccion
    @ACC_CODIGO = 'eliminar',
    @ACC_NOMBRE = 'Eliminar',
    @ACC_DESCRIPCION = 'Permite inactivar o eliminar lógicamente registros';
```

## 10.2. Crear módulos base

```sql
EXEC dbo.UspCreaModulo
    @MOD_CODIGO = 'usuarios',
    @MOD_NOMBRE = 'Usuarios',
    @MOD_DESCRIPCION = 'Gestión de usuarios del sistema';

EXEC dbo.UspCreaModulo
    @MOD_CODIGO = 'roles',
    @MOD_NOMBRE = 'Roles',
    @MOD_DESCRIPCION = 'Gestión de roles del sistema';

EXEC dbo.UspCreaModulo
    @MOD_CODIGO = 'productos',
    @MOD_NOMBRE = 'Productos',
    @MOD_DESCRIPCION = 'Gestión de productos';

EXEC dbo.UspCreaModulo
    @MOD_CODIGO = 'ventas',
    @MOD_NOMBRE = 'Ventas',
    @MOD_DESCRIPCION = 'Gestión de ventas';
```

## 10.3. Crear permisos

Primero consultar los IDs:

```sql
EXEC dbo.UspListaModulos;
EXEC dbo.UspListaAcciones;
```

Luego crear el permiso:

```sql
EXEC dbo.UspCreaPermiso
    @PER_MOD_ID = 1,
    @PER_ACC_ID = 3,
    @PER_DESCRIPCION = 'Permite crear usuarios';
```

## 10.4. Listar permisos

```sql
EXEC dbo.UspListaPermisos;
```

## 10.5. Consultar permisos por módulo

```sql
EXEC dbo.UspListaPermisos
    @MOD_CODIGO = 'usuarios';
```

---

# 11. Reglas de negocio aplicadas

## 11.1. Módulos como catálogo técnico

Los módulos representan áreas funcionales reales del sistema.

Ejemplos:

```text
usuarios
roles
permisos
productos
inventario
compras
ventas
kardex
```

No deberían crearse módulos arbitrarios desde un panel de usuario final, porque el backend debe conocer y validar esos módulos.

---

## 11.2. Acciones como catálogo técnico reutilizable

Las acciones representan operaciones genéricas que pueden repetirse en diferentes módulos.

Ejemplos:

```text
listar
consultar
crear
actualizar
eliminar
registrar
anular
asignar
revocar
exportar
```

Una misma acción puede usarse en varios módulos.

Ejemplo:

```text
usuarios.crear
productos.crear
roles.crear
```

---

## 11.3. Permisos como combinación válida

Un permiso es una combinación válida entre un módulo y una acción.

Ejemplo:

```text
usuarios.crear
ventas.registrar
inventario.ajustar
```

No se deben crear todas las combinaciones posibles, sino únicamente las que el backend realmente va a validar.

---

## 11.4. Nombre lógico del permiso

El nombre lógico del permiso se genera como:

```text
MOD_CODIGO + '.' + ACC_CODIGO
```

Ejemplo:

```text
usuarios.crear
```

Este valor no se almacena físicamente en `TBL_PERMISOS`.

Se obtiene desde:

```sql
VW_RBAC_PERMISOS.PER_NOMBRE
```

---

## 11.5. Evitar permisos inventados

La separación entre módulos, acciones y permisos evita que se creen permisos completamente arbitrarios.

Por ejemplo, no debería existir algo como:

```text
sistema_magico.destruir
```

a menos que exista el módulo `sistema_magico`, exista la acción `destruir` y se haya creado explícitamente la combinación como permiso.

---

# 12. Manejo de errores

Los procedures de escritura utilizan `TRY/CATCH`.

Si ocurre un error:

```text
1. Se revierte la transacción.
2. Se retorna Exitoso = 0.
3. Se retorna el mensaje del error.
4. No se deja una operación parcial.
```

Ejemplo de respuesta de error:

```text
Exitoso = 0
Mensaje = Ya existe un módulo con el mismo código.
```

---

## 12.1. Errores comunes en UspCreaModulo

```text
El código del módulo es obligatorio.
El nombre del módulo es obligatorio.
El código del módulo solo puede contener letras minúsculas, números, guion bajo o guion medio.
MOD_ES_SISTEMA debe ser S o N.
MOD_ESTADO debe ser A o I.
Ya existe un módulo con el mismo código.
```

---

## 12.2. Errores comunes en UspCreaAccion

```text
El código de la acción es obligatorio.
El nombre de la acción es obligatorio.
El código de la acción solo puede contener letras minúsculas, números, guion bajo o guion medio.
ACC_ES_SISTEMA debe ser S o N.
ACC_ESTADO debe ser A o I.
Ya existe una acción con el mismo código.
```

---

## 12.3. Errores comunes en UspCreaPermiso

```text
El módulo del permiso es obligatorio.
La acción del permiso es obligatoria.
PER_ESTADO debe ser A o I.
El módulo no existe o se encuentra inactivo.
La acción no existe o se encuentra inactiva.
Ya existe un permiso para el módulo y la acción seleccionados.
```

---

# 13. Consideraciones para Backend

Desde el backend, estos procedures pueden ser consumidos desde la capa de repositorio usando Dapper.

## 13.1. Uso esperado

```text
Repository -> Ejecuta Procedure
Application Service -> Aplica reglas adicionales si se requieren
Controller -> Expone endpoint REST o herramienta interna
```

Sin embargo, para módulos, acciones y permisos técnicos, no se recomienda crear endpoints públicos administrativos sin control estricto.

---

## 13.2. Uso recomendado

Estos SP se recomiendan principalmente para:

```text
Consola técnica RBAC
Scripts de seed
Migraciones futuras
Herramientas internas de administración
```

---

## 13.3. Endpoints sugeridos solo si se requiere panel técnico

| Método HTTP | Endpoint sugerido       | Procedure          |
| ----------- | ----------------------- | ------------------ |
| `GET`       | `/api/rbac/modules`     | `UspListaModulos`  |
| `POST`      | `/api/rbac/modules`     | `UspCreaModulo`    |
| `GET`       | `/api/rbac/actions`     | `UspListaAcciones` |
| `POST`      | `/api/rbac/actions`     | `UspCreaAccion`    |
| `GET`       | `/api/rbac/permissions` | `UspListaPermisos` |
| `POST`      | `/api/rbac/permissions` | `UspCreaPermiso`   |

Estos endpoints, si se crean, deberían estar protegidos por un rol de acceso total o por permisos muy específicos.

---

# 14. Uso desde consola técnica

La consola técnica puede usar estos procedimientos para permitir:

```text
1. Listar módulos existentes.
2. Crear módulos.
3. Listar acciones existentes.
4. Crear acciones.
5. Listar permisos existentes.
6. Crear permisos seleccionando un módulo y una acción.
```

La consola debe mostrar primero los módulos y acciones disponibles antes de crear un permiso.

Flujo esperado:

```text
Crear módulo
    ↓
Crear acción
    ↓
Seleccionar módulo + acción
    ↓
Crear permiso
    ↓
Consultar permiso como modulo.accion
```

---

# 15. Notas técnicas

* Ejecutar cada `CREATE OR ALTER PROCEDURE` en lotes separados si la herramienta usada no interpreta correctamente `GO`.
* No copiar bloques Markdown con triple backtick dentro de SSMS.
* No almacenar físicamente `PER_NOMBRE` en `TBL_PERMISOS`.
* Usar `VW_RBAC_PERMISOS` para obtener el nombre lógico del permiso.
* No crear permisos que no estén asociados a validaciones reales del backend.
* No permitir que usuarios finales creen módulos, acciones o permisos arbitrarios.
* Los códigos técnicos deben manejarse en minúscula.
* Los nombres visibles pueden usar mayúsculas, tildes y espacios.
* Los módulos y acciones pueden crearse primero por consola técnica y luego usarse en los seeds de roles.
* El siguiente paso natural es documentar y construir los SP de roles, asignación de permisos a roles y asignación de roles a usuarios.
