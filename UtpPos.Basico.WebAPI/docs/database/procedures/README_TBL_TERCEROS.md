# README - Procedures de Terceros

## 1. Objetivo

Este documento describe los procedimientos almacenados creados para la gestión de terceros dentro de la base de datos del Sistema de Ventas.

La entidad de terceros se compone principalmente de dos tablas:

```text
TBL_TERCEROS
TBL_NEGOCIO_TERCEROS
```

`TBL_TERCEROS` almacena la información global del tercero.

`TBL_NEGOCIO_TERCEROS` almacena la relación entre un tercero y uno o varios negocios.

Este diseño permite que un tercero exista una sola vez en el sistema y pueda ser asociado a diferentes negocios sin duplicar su información.

---

## 2. Contexto del cambio estructural

Anteriormente, `TBL_TERCEROS` tenía una relación directa con negocio mediante el campo:

```sql
TER_NEG_ID
```

Ese modelo obligaba a duplicar terceros cuando el mismo cliente o proveedor pertenecía a varios negocios.

El nuevo modelo elimina esa dependencia directa.

Ahora:

```text
TBL_TERCEROS = catálogo maestro global de terceros
TBL_NEGOCIO_TERCEROS = tabla pivote entre negocios y terceros
```

Con este cambio:

```text
Un tercero existe una sola vez.
Un tercero puede pertenecer a varios negocios.
Un negocio puede tener varios terceros asociados.
Compras y ventas deben validar la relación negocio-tercero antes de operar.
```

---

## 3. Corrección de nombre de campo

El campo de número de documento debe llamarse:

```sql
TER_NUMERO_DOCUMENTO
```

No debe usarse:

```sql
TER_NUM_DOCUMENTO
```

Todos los procedures, DTOs, repositorios, filtros y consultas deben usar `TER_NUMERO_DOCUMENTO`.

---

## 4. Tablas relacionadas

## 4.1. Tabla principal: `TBL_TERCEROS`

```sql
TBL_TERCEROS
```

### Estructura base esperada

```sql
TER_ID                         INT IDENTITY(1,1) PRIMARY KEY
TER_TIPO_DOC_ID                INT NOT NULL
TER_NUMERO_DOCUMENTO           VARCHAR(20) NOT NULL
TER_NOMBRE                     VARCHAR(60) NOT NULL
TER_CIUDAD_ID                  INT NOT NULL
TER_DEPTO_ID                   INT NOT NULL
TER_DIRECCION                  VARCHAR(100) NULL
TER_TELEFONO                   VARCHAR(20) NOT NULL
TER_CREADO_POR_USUARIO_ID      INT NOT NULL
TER_ESTADO                     CHAR(1) DEFAULT 'A' NOT NULL
TER_FECHA_CREADO               DATETIME NOT NULL
```

### Campo eliminado del modelo

```sql
TER_NEG_ID
```

`TER_NEG_ID` ya no debe existir en `TBL_TERCEROS`.

La relación con negocios se maneja exclusivamente mediante `TBL_NEGOCIO_TERCEROS`.

---

## 4.2. Tabla pivote: `TBL_NEGOCIO_TERCEROS`

```sql
TBL_NEGOCIO_TERCEROS
```

### Estructura base esperada

```sql
NEGTER_ID                         INT IDENTITY(1,1) PRIMARY KEY
NEGTER_NEG_ID                     INT NOT NULL
NEGTER_TER_ID                     INT NOT NULL
NEGTER_CREADO_POR_USUARIO_ID      INT NOT NULL
NEGTER_MODIFICADO_POR_USUARIO_ID  INT NULL
NEGTER_ESTADO                     CHAR(1) DEFAULT 'A' NOT NULL
NEGTER_FECHA_CREADO               DATETIME DEFAULT GETDATE() NOT NULL
NEGTER_FECHA_ACTUALIZACION        DATETIME NULL
```

---

# 5. Relaciones principales

## 5.1. Relaciones de `TBL_TERCEROS`

```sql
TBL_TERCEROS.TER_TIPO_DOC_ID -> TBL_PARAMETROS.PARAM_ID
TBL_TERCEROS.TER_CIUDAD_ID -> TBL_PARAMETROS.PARAM_ID
TBL_TERCEROS.TER_DEPTO_ID -> TBL_PARAMETROS.PARAM_ID
TBL_TERCEROS.TER_CREADO_POR_USUARIO_ID -> TBL_USUARIOS.USR_ID
```

`TBL_TERCEROS` ya no debe tener relación directa con `TBL_NEGOCIO`.

---

## 5.2. Relaciones de `TBL_NEGOCIO_TERCEROS`

```sql
TBL_NEGOCIO_TERCEROS.NEGTER_NEG_ID -> TBL_NEGOCIO.NEG_ID
TBL_NEGOCIO_TERCEROS.NEGTER_TER_ID -> TBL_TERCEROS.TER_ID
TBL_NEGOCIO_TERCEROS.NEGTER_CREADO_POR_USUARIO_ID -> TBL_USUARIOS.USR_ID
TBL_NEGOCIO_TERCEROS.NEGTER_MODIFICADO_POR_USUARIO_ID -> TBL_USUARIOS.USR_ID
```

---

# 6. Constraints e índices recomendados

## 6.1. Constraint única global de terceros

Como el tercero ahora es global, no debe duplicarse por negocio.

La identificación única del tercero debe ser:

```sql
TER_TIPO_DOC_ID
TER_NUMERO_DOCUMENTO
```

Constraint recomendada:

```sql
UQ_TBL_TERCEROS_DOCUMENTO_GLOBAL
UNIQUE (TER_TIPO_DOC_ID, TER_NUMERO_DOCUMENTO)
```

Antes de crearla, se debe validar que no existan duplicados.

Consulta de validación:

```sql
SELECT
    TER_TIPO_DOC_ID,
    TER_NUMERO_DOCUMENTO,
    COUNT(*) AS Cantidad
FROM dbo.TBL_TERCEROS
GROUP BY
    TER_TIPO_DOC_ID,
    TER_NUMERO_DOCUMENTO
HAVING COUNT(*) > 1;
```

---

## 6.2. Constraint única de relación negocio-tercero

En `TBL_NEGOCIO_TERCEROS`, no debe duplicarse la misma relación.

Constraint recomendada:

```sql
UQ_TBL_NEGOCIO_TERCEROS_NEGOCIO_TERCERO
UNIQUE (NEGTER_NEG_ID, NEGTER_TER_ID)
```

Esto evita registrar dos veces el mismo tercero para el mismo negocio.

---

## 6.3. Índices recomendados

Para `TBL_TERCEROS`:

```sql
IX_TBL_TERCEROS_TIPO_DOC_DOCUMENTO
IX_TBL_TERCEROS_ESTADO
IX_TBL_TERCEROS_NOMBRE
```

Para `TBL_NEGOCIO_TERCEROS`:

```sql
IX_TBL_NEGOCIO_TERCEROS_NEG_ID
IX_TBL_NEGOCIO_TERCEROS_TER_ID
IX_TBL_NEGOCIO_TERCEROS_ESTADO
```

---

# 7. Procedures involucrados

## 7.1. Procedures de `TBL_TERCEROS`

```text
UspListaTerceros
UspConsultaTercero
UspCreaTercero
UspActualizaTercero
UspEliminaTercero
```

Estos procedures administran la información global del tercero.

Ya no deben insertar, actualizar, consultar ni filtrar directamente por `TER_NEG_ID`.

---

## 7.2. Procedures de `TBL_NEGOCIO_TERCEROS`

```text
UspAsociaTerceroNegocio
UspInactivaTerceroNegocio
UspValidaTerceroNegocio
UspListaTercerosPorNegocio
```

Estos procedures administran la relación entre negocios y terceros.

---

# 8. Consideraciones generales

* No se realiza eliminación física de terceros.

* No se realiza eliminación física de relaciones negocio-tercero.

* La eliminación se maneja mediante borrado lógico usando campos de estado.

* Estados válidos:

  * `A`: Activo.
  * `I`: Inactivo.

* `TBL_TERCEROS` contiene datos globales del tercero.

* `TBL_NEGOCIO_TERCEROS` define en qué negocios puede operar el tercero.

* Un tercero puede estar asociado a varios negocios.

* Un tercero no debe duplicarse por negocio.

* `TER_NEG_ID` pertenece al modelo anterior y debe eliminarse de tabla, SPs, DTOs, repositorios y frontend.

* `TER_NUMERO_DOCUMENTO` es el nombre correcto del campo de documento.

* Los procedures de escritura usan `TRY/CATCH`.

* Los procedures de escritura usan transacciones para garantizar atomicidad.

* Compras y ventas deben validar que el tercero esté asociado activamente al negocio de la operación.

---

# 9. Detalle de procedures de terceros

---

## 9.1. UspListaTerceros

### Objetivo

Listar los terceros registrados en el sistema como catálogo global.

Permite aplicar filtros opcionales por tipo de documento, estado, número de documento, nombre o teléfono.

### Nombre

```sql
dbo.UspListaTerceros
```

### Parámetros de entrada

| Parámetro               | Tipo          | Obligatorio | Descripción                                                               |
| ----------------------- | ------------- | ----------- | ------------------------------------------------------------------------- |
| `@TER_TIPO_DOC_ID`      | `INT`         | No          | Permite filtrar terceros por tipo de documento.                           |
| `@TER_ESTADO`           | `CHAR(1)`     | No          | Permite filtrar por estado global del tercero: `A` o `I`.                 |
| `@TER_NUMERO_DOCUMENTO` | `VARCHAR(20)` | No          | Permite buscar terceros por coincidencia parcial del número de documento. |
| `@TER_NOMBRE`           | `VARCHAR(60)` | No          | Permite buscar terceros por coincidencia parcial del nombre.              |
| `@TER_TELEFONO`         | `VARCHAR(20)` | No          | Permite buscar terceros por coincidencia parcial del teléfono.            |

### Cambio respecto al modelo anterior

El parámetro siguiente debe eliminarse:

```sql
@TER_NEG_ID
```

`UspListaTerceros` ya no debe filtrar directamente por negocio.

Si se necesita listar terceros de un negocio específico, se debe usar:

```sql
UspListaTercerosPorNegocio
```

### Resultado

Retorna terceros globales junto con información descriptiva de tipo de documento, departamento, ciudad y usuario creador.

### Campos retornados

| Campo                       | Descripción                          |
| --------------------------- | ------------------------------------ |
| `TER_ID`                    | Identificador del tercero.           |
| `TER_TIPO_DOC_ID`           | Identificador del tipo de documento. |
| `TIPO_DOCUMENTO`            | Descripción del tipo de documento.   |
| `TER_NUMERO_DOCUMENTO`      | Número de documento del tercero.     |
| `TER_NOMBRE`                | Nombre o razón social del tercero.   |
| `TER_DEPTO_ID`              | Identificador del departamento.      |
| `DEPARTAMENTO`              | Descripción del departamento.        |
| `TER_CIUDAD_ID`             | Identificador de la ciudad.          |
| `CIUDAD`                    | Descripción de la ciudad.            |
| `TER_DIRECCION`             | Dirección del tercero.               |
| `TER_TELEFONO`              | Teléfono del tercero.                |
| `TER_CREADO_POR_USUARIO_ID` | Identificador del usuario creador.   |
| `CREADO_POR_USUARIO`        | Nombre del usuario creador.          |
| `TER_ESTADO`                | Estado global del tercero.           |
| `TER_FECHA_CREADO`          | Fecha de creación del tercero.       |

### Ejemplos de uso

Listar todos los terceros:

```sql
EXEC dbo.UspListaTerceros;
```

Listar terceros activos:

```sql
EXEC dbo.UspListaTerceros
    @TER_ESTADO = 'A';
```

Buscar por número de documento:

```sql
EXEC dbo.UspListaTerceros
    @TER_NUMERO_DOCUMENTO = '900';
```

Buscar por nombre:

```sql
EXEC dbo.UspListaTerceros
    @TER_NOMBRE = 'Proveedor';
```

---

## 9.2. UspConsultaTercero

### Objetivo

Consultar la información global de un tercero mediante su identificador.

### Nombre

```sql
dbo.UspConsultaTercero
```

### Parámetros de entrada

| Parámetro | Tipo  | Obligatorio | Descripción                            |
| --------- | ----- | ----------- | -------------------------------------- |
| `@TER_ID` | `INT` | Sí          | Identificador del tercero a consultar. |

### Cambio respecto al modelo anterior

Este procedure ya no debe retornar:

```sql
TER_NEG_ID
NEG_NOMBRE
```

La relación con negocios se debe consultar desde `TBL_NEGOCIO_TERCEROS`.

Si se necesita validar el tercero dentro de un negocio, se debe usar:

```sql
UspValidaTerceroNegocio
```

o consultar mediante:

```sql
UspListaTercerosPorNegocio
```

### Campos retornados

| Campo                       | Descripción                          |
| --------------------------- | ------------------------------------ |
| `TER_ID`                    | Identificador del tercero.           |
| `TER_TIPO_DOC_ID`           | Identificador del tipo de documento. |
| `TIPO_DOCUMENTO`            | Descripción del tipo de documento.   |
| `TER_NUMERO_DOCUMENTO`      | Número de documento del tercero.     |
| `TER_NOMBRE`                | Nombre o razón social del tercero.   |
| `TER_DEPTO_ID`              | Identificador del departamento.      |
| `DEPARTAMENTO`              | Descripción del departamento.        |
| `TER_CIUDAD_ID`             | Identificador de la ciudad.          |
| `CIUDAD`                    | Descripción de la ciudad.            |
| `TER_DIRECCION`             | Dirección del tercero.               |
| `TER_TELEFONO`              | Teléfono del tercero.                |
| `TER_CREADO_POR_USUARIO_ID` | Identificador del usuario creador.   |
| `CREADO_POR_USUARIO`        | Nombre del usuario creador.          |
| `TER_ESTADO`                | Estado global del tercero.           |
| `TER_FECHA_CREADO`          | Fecha de creación del tercero.       |

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaTercero
    @TER_ID = 1;
```

---

## 9.3. UspCreaTercero

### Objetivo

Crear un nuevo tercero global en `TBL_TERCEROS`.

Este procedimiento no debe asociar el tercero directamente mediante `TER_NEG_ID`, porque ese campo ya no pertenece al modelo.

### Nombre

```sql
dbo.UspCreaTercero
```

### Parámetros de entrada

| Parámetro                    | Tipo           | Obligatorio | Descripción                                    |
| ---------------------------- | -------------- | ----------- | ---------------------------------------------- |
| `@TER_TIPO_DOC_ID`           | `INT`          | Sí          | Identificador del tipo de documento.           |
| `@TER_NUMERO_DOCUMENTO`      | `VARCHAR(20)`  | Sí          | Número de documento del tercero.               |
| `@TER_NOMBRE`                | `VARCHAR(60)`  | Sí          | Nombre o razón social del tercero.             |
| `@TER_CIUDAD_ID`             | `INT`          | Sí          | Identificador de la ciudad.                    |
| `@TER_DEPTO_ID`              | `INT`          | Sí          | Identificador del departamento.                |
| `@TER_DIRECCION`             | `VARCHAR(100)` | No          | Dirección del tercero.                         |
| `@TER_TELEFONO`              | `VARCHAR(20)`  | Sí          | Teléfono del tercero.                          |
| `@TER_CREADO_POR_USUARIO_ID` | `INT`          | Sí          | Identificador del usuario que crea el tercero. |
| `@TER_ESTADO`                | `CHAR(1)`      | No          | Estado global del tercero. Por defecto: `A`.   |

### Parámetro eliminado

Debe eliminarse:

```sql
@TER_NEG_ID
```

### Validaciones realizadas

El procedimiento valida que:

1. El tipo de documento sea obligatorio.
2. El número de documento sea obligatorio.
3. El nombre del tercero sea obligatorio.
4. La ciudad sea obligatoria.
5. El departamento sea obligatorio.
6. El teléfono sea obligatorio.
7. El usuario creador sea obligatorio.
8. El estado sea válido: `A` o `I`.
9. El tipo de documento exista y esté activo.
10. El departamento exista y esté activo.
11. La ciudad exista y esté activa.
12. El usuario creador exista y esté activo.
13. No exista otro tercero con el mismo tipo de documento y número de documento.

### Comportamiento esperado

`UspCreaTercero` debe insertar únicamente en:

```sql
TBL_TERCEROS
```

No debe insertar en:

```sql
TBL_NEGOCIO_TERCEROS
```

La asociación con negocio debe hacerse posteriormente mediante:

```sql
UspAsociaTerceroNegocio
```

o desde un procedimiento orquestador si se decide crear uno.

### Resultado

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `TER_ID`  | Identificador del tercero creado.                         |

### Ejemplo de creación

```sql
EXEC dbo.UspCreaTercero
    @TER_TIPO_DOC_ID = 1,
    @TER_NUMERO_DOCUMENTO = '900123456',
    @TER_NOMBRE = 'Proveedor Prueba S.A.S',
    @TER_CIUDAD_ID = 5,
    @TER_DEPTO_ID = 4,
    @TER_DIRECCION = 'Calle 12 # 8-30',
    @TER_TELEFONO = '3001234567',
    @TER_CREADO_POR_USUARIO_ID = 1,
    @TER_ESTADO = 'A';
```

---

## 9.4. UspActualizaTercero

### Objetivo

Actualizar la información global de un tercero existente.

Este procedimiento permite edición parcial.

### Nombre

```sql
dbo.UspActualizaTercero
```

### Parámetros de entrada

| Parámetro               | Tipo           | Obligatorio | Descripción                                                           |
| ----------------------- | -------------- | ----------- | --------------------------------------------------------------------- |
| `@TER_ID`               | `INT`          | Sí          | Identificador del tercero a actualizar.                               |
| `@TER_TIPO_DOC_ID`      | `INT`          | No          | Nuevo tipo de documento.                                              |
| `@TER_NUMERO_DOCUMENTO` | `VARCHAR(20)`  | No          | Nuevo número de documento.                                            |
| `@TER_NOMBRE`           | `VARCHAR(60)`  | No          | Nuevo nombre o razón social.                                          |
| `@TER_CIUDAD_ID`        | `INT`          | No          | Nueva ciudad.                                                         |
| `@TER_DEPTO_ID`         | `INT`          | No          | Nuevo departamento.                                                   |
| `@TER_DIRECCION`        | `VARCHAR(100)` | No          | Nueva dirección.                                                      |
| `@TER_TELEFONO`         | `VARCHAR(20)`  | No          | Nuevo teléfono.                                                       |
| `@TER_ESTADO`           | `CHAR(1)`      | No          | Nuevo estado global del tercero: `A` o `I`.                           |
| `@LIMPIAR_DIRECCION`    | `BIT`          | No          | Si es `1`, limpia la dirección y la deja en `NULL`. Por defecto: `0`. |

### Parámetro eliminado

Debe eliminarse:

```sql
@TER_NEG_ID
```

### Cambio respecto al modelo anterior

`UspActualizaTercero` ya no debe actualizar negocio.

La asociación con negocios se administra exclusivamente mediante `TBL_NEGOCIO_TERCEROS`.

Para asociar o reactivar un tercero en un negocio:

```sql
UspAsociaTerceroNegocio
```

Para retirar un tercero de un negocio:

```sql
UspInactivaTerceroNegocio
```

### Validaciones realizadas

El procedimiento valida que:

1. El identificador del tercero sea obligatorio.
2. El tercero exista.
3. Si se envía un nuevo tipo de documento, exista y esté activo.
4. Si se envía una nueva ciudad, exista y esté activa.
5. Si se envía un nuevo departamento, exista y esté activo.
6. Si se envía un nuevo estado, sea válido: `A` o `I`.
7. No exista otro tercero con la misma combinación final de tipo de documento y número de documento.

### Ejemplo de actualización parcial

```sql
EXEC dbo.UspActualizaTercero
    @TER_ID = 1,
    @TER_NOMBRE = 'Proveedor Prueba Actualizado S.A.S';
```

### Ejemplo de actualización de varios campos

```sql
EXEC dbo.UspActualizaTercero
    @TER_ID = 1,
    @TER_CIUDAD_ID = 5,
    @TER_DEPTO_ID = 4,
    @TER_DIRECCION = 'Carrera 10 # 20-30',
    @TER_TELEFONO = '3111234567',
    @TER_ESTADO = 'A';
```

### Limpiar dirección

```sql
EXEC dbo.UspActualizaTercero
    @TER_ID = 1,
    @LIMPIAR_DIRECCION = 1;
```

---

## 9.5. UspEliminaTercero

### Objetivo

Inactivar globalmente un tercero mediante borrado lógico.

Este procedimiento no elimina físicamente el registro.

Actualiza:

```sql
TER_ESTADO = 'I'
```

### Nombre

```sql
dbo.UspEliminaTercero
```

### Parámetros de entrada

| Parámetro | Tipo  | Obligatorio | Descripción                            |
| --------- | ----- | ----------- | -------------------------------------- |
| `@TER_ID` | `INT` | Sí          | Identificador del tercero a inactivar. |

### Consideración importante

Inactivar un tercero global afecta todos los negocios donde esté asociado.

Para retirar un tercero de un solo negocio, no se debe usar `UspEliminaTercero`.

Se debe usar:

```sql
UspInactivaTerceroNegocio
```

### Validaciones realizadas

El procedimiento valida que el tercero exista antes de inactivarlo.

### Resultado

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `TER_ID`  | Identificador del tercero inactivado.                     |

### Ejemplo de uso

```sql
EXEC dbo.UspEliminaTercero
    @TER_ID = 1;
```

---

# 10. Detalle de procedures de relación negocio-tercero

---

## 10.1. UspAsociaTerceroNegocio

### Objetivo

Asociar un tercero existente a un negocio.

Si la relación ya existe activa, retorna exitoso sin duplicar.

Si la relación existe inactiva, la reactiva.

### Nombre

```sql
dbo.UspAsociaTerceroNegocio
```

### Parámetros de entrada

| Parámetro                       | Tipo  | Obligatorio | Descripción                                     |
| ------------------------------- | ----- | ----------- | ----------------------------------------------- |
| `@NEGTER_NEG_ID`                | `INT` | Sí          | Identificador del negocio.                      |
| `@NEGTER_TER_ID`                | `INT` | Sí          | Identificador del tercero.                      |
| `@NEGTER_CREADO_POR_USUARIO_ID` | `INT` | Sí          | Identificador del usuario que crea la relación. |

### Validaciones realizadas

El procedimiento valida que:

1. El negocio sea obligatorio.
2. El tercero sea obligatorio.
3. El usuario creador sea obligatorio.
4. El negocio exista y esté activo.
5. El tercero exista y esté activo.
6. El usuario creador exista y esté activo.

### Resultado

| Campo       | Descripción                                               |
| ----------- | --------------------------------------------------------- |
| `Exitoso`   | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje`   | Mensaje descriptivo del resultado.                        |
| `NEGTER_ID` | Identificador de la relación negocio-tercero.             |

### Ejemplo

```sql
EXEC dbo.UspAsociaTerceroNegocio
    @NEGTER_NEG_ID = 1,
    @NEGTER_TER_ID = 10,
    @NEGTER_CREADO_POR_USUARIO_ID = 1;
```

---

## 10.2. UspInactivaTerceroNegocio

### Objetivo

Inactivar la relación entre un tercero y un negocio.

No inactiva el tercero global.

### Nombre

```sql
dbo.UspInactivaTerceroNegocio
```

### Parámetros de entrada

| Parámetro                           | Tipo  | Obligatorio | Descripción                                         |
| ----------------------------------- | ----- | ----------- | --------------------------------------------------- |
| `@NEGTER_NEG_ID`                    | `INT` | Sí          | Identificador del negocio.                          |
| `@NEGTER_TER_ID`                    | `INT` | Sí          | Identificador del tercero.                          |
| `@NEGTER_MODIFICADO_POR_USUARIO_ID` | `INT` | Sí          | Identificador del usuario que inactiva la relación. |

### Resultado

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `TER_ID`  | Identificador del tercero retirado del negocio.           |

### Ejemplo

```sql
EXEC dbo.UspInactivaTerceroNegocio
    @NEGTER_NEG_ID = 1,
    @NEGTER_TER_ID = 10,
    @NEGTER_MODIFICADO_POR_USUARIO_ID = 1;
```

---

## 10.3. UspValidaTerceroNegocio

### Objetivo

Validar si existe una relación activa entre un tercero y un negocio.

Este procedimiento se usa como apoyo para compras, ventas y validaciones desde backend.

### Nombre

```sql
dbo.UspValidaTerceroNegocio
```

### Parámetros de entrada

| Parámetro | Tipo  | Obligatorio | Descripción                |
| --------- | ----- | ----------- | -------------------------- |
| `@NEG_ID` | `INT` | Sí          | Identificador del negocio. |
| `@TER_ID` | `INT` | Sí          | Identificador del tercero. |

### Resultado

| Campo                  | Descripción                                                      |
| ---------------------- | ---------------------------------------------------------------- |
| `ExisteRelacionActiva` | `1` si existe relación activa; `0` si no existe o está inactiva. |

### Criterios para retornar `1`

```text
El negocio existe y está activo.
El tercero existe y está activo.
La relación negocio-tercero existe.
La relación negocio-tercero está activa.
```

### Ejemplo

```sql
EXEC dbo.UspValidaTerceroNegocio
    @NEG_ID = 1,
    @TER_ID = 10;
```

---

## 10.4. UspListaTercerosPorNegocio

### Objetivo

Listar terceros asociados activamente a un negocio.

Este es el procedimiento recomendado para búsquedas de terceros en compras y ventas.

### Nombre

```sql
dbo.UspListaTercerosPorNegocio
```

### Parámetros de entrada

| Parámetro               | Tipo          | Obligatorio | Descripción                                                        |
| ----------------------- | ------------- | ----------- | ------------------------------------------------------------------ |
| `@NEG_ID`               | `INT`         | Sí          | Identificador del negocio.                                         |
| `@TER_ESTADO`           | `CHAR(1)`     | No          | Permite filtrar por estado global del tercero: `A` o `I`.          |
| `@TER_NUMERO_DOCUMENTO` | `VARCHAR(20)` | No          | Permite buscar por coincidencia parcial del número de documento.   |
| `@TER_NOMBRE`           | `VARCHAR(60)` | No          | Permite buscar por coincidencia parcial del nombre o razón social. |
| `@TER_TELEFONO`         | `VARCHAR(20)` | No          | Permite buscar por coincidencia parcial del teléfono.              |

### Resultado

Retorna terceros asociados activamente al negocio indicado.

### Campos retornados

| Campo                       | Descripción                                    |
| --------------------------- | ---------------------------------------------- |
| `TER_ID`                    | Identificador del tercero.                     |
| `TER_TIPO_DOC_ID`           | Identificador del tipo de documento.           |
| `TIPO_DOCUMENTO`            | Descripción del tipo de documento.             |
| `TER_NUMERO_DOCUMENTO`      | Número de documento del tercero.               |
| `TER_NOMBRE`                | Nombre o razón social del tercero.             |
| `TER_DEPTO_ID`              | Identificador del departamento.                |
| `DEPARTAMENTO`              | Descripción del departamento.                  |
| `TER_CIUDAD_ID`             | Identificador de la ciudad.                    |
| `CIUDAD`                    | Descripción de la ciudad.                      |
| `TER_DIRECCION`             | Dirección del tercero.                         |
| `TER_TELEFONO`              | Teléfono del tercero.                          |
| `TER_CREADO_POR_USUARIO_ID` | Identificador del usuario creador del tercero. |
| `CREADO_POR_USUARIO`        | Nombre del usuario creador del tercero.        |
| `TER_ESTADO`                | Estado global del tercero.                     |
| `TER_FECHA_CREADO`          | Fecha de creación del tercero.                 |
| `NEGTER_ID`                 | Identificador de la relación negocio-tercero.  |
| `NEGTER_NEG_ID`             | Identificador del negocio asociado.            |
| `NEGTER_ESTADO`             | Estado de la relación negocio-tercero.         |

### Ejemplo

```sql
EXEC dbo.UspListaTercerosPorNegocio
    @NEG_ID = 1,
    @TER_ESTADO = 'A';
```

---

# 11. Flujo recomendado para crear o asociar terceros

## 11.1. Crear tercero global y asociarlo a negocio

```text
1. Buscar tercero por TER_TIPO_DOC_ID + TER_NUMERO_DOCUMENTO.
2. Si no existe, ejecutar UspCreaTercero.
3. Obtener TER_ID.
4. Ejecutar UspAsociaTerceroNegocio.
```

Ejemplo:

```sql
EXEC dbo.UspCreaTercero
    @TER_TIPO_DOC_ID = 1,
    @TER_NUMERO_DOCUMENTO = '900123456',
    @TER_NOMBRE = 'Proveedor Prueba S.A.S',
    @TER_CIUDAD_ID = 5,
    @TER_DEPTO_ID = 4,
    @TER_DIRECCION = 'Calle 12 # 8-30',
    @TER_TELEFONO = '3001234567',
    @TER_CREADO_POR_USUARIO_ID = 1,
    @TER_ESTADO = 'A';

EXEC dbo.UspAsociaTerceroNegocio
    @NEGTER_NEG_ID = 1,
    @NEGTER_TER_ID = 10,
    @NEGTER_CREADO_POR_USUARIO_ID = 1;
```

---

## 11.2. Asociar tercero existente a otro negocio

```sql
EXEC dbo.UspAsociaTerceroNegocio
    @NEGTER_NEG_ID = 2,
    @NEGTER_TER_ID = 10,
    @NEGTER_CREADO_POR_USUARIO_ID = 1;
```

---

## 11.3. Retirar tercero de un negocio

```sql
EXEC dbo.UspInactivaTerceroNegocio
    @NEGTER_NEG_ID = 1,
    @NEGTER_TER_ID = 10,
    @NEGTER_MODIFICADO_POR_USUARIO_ID = 1;
```

---

# 12. Impacto en compras y ventas

## 12.1. Compras

`UspRegistraCompra` debe validar que el proveedor esté asociado activamente al negocio de la compra.

Validación esperada:

```sql
IF NOT EXISTS (
    SELECT 1
    FROM dbo.TBL_NEGOCIO_TERCEROS NT
    INNER JOIN dbo.TBL_TERCEROS T
        ON T.TER_ID = NT.NEGTER_TER_ID
       AND T.TER_ESTADO = 'A'
    WHERE NT.NEGTER_NEG_ID = @COMPRA_NEG_ID
      AND NT.NEGTER_TER_ID = @COMPRA_TER_ID
      AND NT.NEGTER_ESTADO = 'A'
)
    RAISERROR('El tercero/proveedor no está asociado al negocio de la compra o se encuentra inactivo.', 16, 1);
```

---

## 12.2. Ventas

`UspRegistraVenta` debe validar que el cliente esté asociado activamente al negocio de la venta.

Validación esperada:

```sql
IF NOT EXISTS (
    SELECT 1
    FROM dbo.TBL_NEGOCIO_TERCEROS NT
    INNER JOIN dbo.TBL_TERCEROS T
        ON T.TER_ID = NT.NEGTER_TER_ID
       AND T.TER_ESTADO = 'A'
    WHERE NT.NEGTER_NEG_ID = @VENTA_NEG_ID
      AND NT.NEGTER_TER_ID = @VENTA_TER_ID
      AND NT.NEGTER_ESTADO = 'A'
)
    RAISERROR('El tercero/cliente no está asociado al negocio de la venta o se encuentra inactivo.', 16, 1);
```

---

# 13. Flujo recomendado de prueba

## 13.1. Crear tercero global

```sql
EXEC dbo.UspCreaTercero
    @TER_TIPO_DOC_ID = 1,
    @TER_NUMERO_DOCUMENTO = '900123456',
    @TER_NOMBRE = 'Proveedor Prueba S.A.S',
    @TER_CIUDAD_ID = 5,
    @TER_DEPTO_ID = 4,
    @TER_DIRECCION = 'Calle 12 # 8-30',
    @TER_TELEFONO = '3001234567',
    @TER_CREADO_POR_USUARIO_ID = 1,
    @TER_ESTADO = 'A';
```

---

## 13.2. Asociar tercero a negocio

```sql
EXEC dbo.UspAsociaTerceroNegocio
    @NEGTER_NEG_ID = 1,
    @NEGTER_TER_ID = 1,
    @NEGTER_CREADO_POR_USUARIO_ID = 1;
```

---

## 13.3. Listar terceros por negocio

```sql
EXEC dbo.UspListaTercerosPorNegocio
    @NEG_ID = 1,
    @TER_ESTADO = 'A';
```

---

## 13.4. Validar relación activa

```sql
EXEC dbo.UspValidaTerceroNegocio
    @NEG_ID = 1,
    @TER_ID = 1;
```

Resultado esperado:

```text
ExisteRelacionActiva = 1
```

---

## 13.5. Retirar tercero de negocio

```sql
EXEC dbo.UspInactivaTerceroNegocio
    @NEGTER_NEG_ID = 1,
    @NEGTER_TER_ID = 1,
    @NEGTER_MODIFICADO_POR_USUARIO_ID = 1;
```

---

## 13.6. Validar relación inactiva

```sql
EXEC dbo.UspValidaTerceroNegocio
    @NEG_ID = 1,
    @TER_ID = 1;
```

Resultado esperado:

```text
ExisteRelacionActiva = 0
```

---

# 14. Consideraciones para Backend

Desde el backend, estos procedures pueden ser consumidos desde la capa de repositorio usando Dapper.

## 14.1. Uso esperado

```text
Repository -> Ejecuta Procedure
Application Service -> Aplica reglas adicionales si se requieren
Controller -> Expone endpoint REST
```

---

## 14.2. Endpoints sugeridos

| Método HTTP | Endpoint sugerido                            | Procedure                    |
| ----------- | -------------------------------------------- | ---------------------------- |
| `GET`       | `/api/terceros`                              | `UspListaTerceros`           |
| `GET`       | `/api/terceros/{id}`                         | `UspConsultaTercero`         |
| `POST`      | `/api/terceros`                              | `UspCreaTercero`             |
| `PATCH`     | `/api/terceros/{id}`                         | `UspActualizaTercero`        |
| `DELETE`    | `/api/terceros/{id}`                         | `UspEliminaTercero`          |
| `GET`       | `/api/negocios/{negocioId}/terceros`         | `UspListaTercerosPorNegocio` |
| `POST`      | `/api/negocios/{negocioId}/terceros/{terId}` | `UspAsociaTerceroNegocio`    |
| `DELETE`    | `/api/negocios/{negocioId}/terceros/{terId}` | `UspInactivaTerceroNegocio`  |

---

## 14.3. Uso desde compras

El backend debe buscar proveedores desde:

```sql
UspListaTercerosPorNegocio
```

Antes de registrar compra, debe existir relación activa entre:

```text
COMPRA_NEG_ID
COMPRA_TER_ID
```

---

## 14.4. Uso desde ventas

El backend debe buscar clientes desde:

```sql
UspListaTercerosPorNegocio
```

Antes de registrar venta, debe existir relación activa entre:

```text
VENTA_NEG_ID
VENTA_TER_ID
```

---

# 15. Procedures existentes que deben modificarse

## 15.1. Procedures de terceros

```text
UspListaTerceros
UspConsultaTercero
UspCreaTercero
UspActualizaTercero
UspEliminaTercero
```

Cambios obligatorios:

```text
Eliminar TER_NEG_ID de parámetros.
Eliminar TER_NEG_ID de SELECT.
Eliminar TER_NEG_ID de INSERT.
Eliminar TER_NEG_ID de UPDATE.
Eliminar joins directos entre TBL_TERCEROS y TBL_NEGOCIO.
Usar TER_NUMERO_DOCUMENTO como nombre correcto del campo.
Aplicar unicidad global por TER_TIPO_DOC_ID + TER_NUMERO_DOCUMENTO.
```

---

## 15.2. Procedures de relación negocio-tercero

```text
UspAsociaTerceroNegocio
UspInactivaTerceroNegocio
UspValidaTerceroNegocio
UspListaTercerosPorNegocio
```

Estos procedures deben usar `TBL_NEGOCIO_TERCEROS`.

---

## 15.3. Procedures de compras

```text
UspRegistraCompra
```

Cambio obligatorio:

```text
Reemplazar validación directa del tercero por validación en TBL_NEGOCIO_TERCEROS.
```

---

## 15.4. Procedures de ventas

```text
UspRegistraVenta
```

Cambio obligatorio:

```text
Reemplazar validación directa del tercero por validación en TBL_NEGOCIO_TERCEROS.
```

---

## 15.5. Procedures de búsqueda

Revisar cualquier procedure que busque terceros por negocio, documento, nombre o teléfono.

Ejemplos:

```text
UspBuscaTerceroPorDocumento
UspBuscaTerceroPorNombre
UspListaClientes
UspListaProveedores
UspListaTercerosPorTipo
```

Si la búsqueda es por negocio, debe usar `TBL_NEGOCIO_TERCEROS`.

---

# 16. Notas técnicas

* Ejecutar cada `CREATE OR ALTER PROCEDURE` por separado en DBeaver si la herramienta presenta problemas con separadores de lote.
* Evitar el uso de `GO` en DBeaver si genera errores de sintaxis.
* Mantener los nombres de procedures bajo la convención definida.
* Mantener el borrado lógico como estándar del sistema.
* No consumir directamente `TBL_TERCEROS` ni `TBL_NEGOCIO_TERCEROS` desde backend si existen procedures definidos.
* No duplicar terceros por negocio.
* No usar `TER_NUM_DOCUMENTO`.
* Usar siempre `TER_NUMERO_DOCUMENTO`.
* El campo `TER_NEG_ID` pertenece al modelo anterior.
* `TER_NEG_ID` debe eliminarse de SPs, DTOs, repositorios, frontend y documentación.
* Compras y ventas deben validar relación activa negocio-tercero antes de registrar operaciones.
* Inactivar un tercero global afecta todos los negocios.
* Inactivar una relación negocio-tercero solo afecta ese negocio.
