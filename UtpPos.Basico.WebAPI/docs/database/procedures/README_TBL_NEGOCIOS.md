# README - Procedures de Negocio

## 1. Objetivo

Este documento describe los procedimientos almacenados creados para la gestión de la tabla `TBL_NEGOCIO` dentro de la base de datos del Sistema de Ventas.

La tabla `TBL_NEGOCIO` representa la información principal del negocio o empresa que opera el sistema. Esta entidad sirve como punto de relación para usuarios, terceros, productos, compras, ventas, inventario y movimientos de Kardex.

Los procedimientos almacenados definidos permiten listar, consultar, crear, actualizar e inactivar lógicamente registros de negocio.

---

## 2. Tabla relacionada

### Tabla principal

```sql
TBL_NEGOCIO
```

### Estructura base

```sql
NEG_ID         INT IDENTITY(1,1) PRIMARY KEY
NEG_NIT        VARCHAR(20) NOT NULL
NEG_NOMBRE     VARCHAR(80) NOT NULL
NEG_DIRECCION  VARCHAR(100) NOT NULL
NEG_TELEFONO   VARCHAR(20) NULL
NEG_ESTADO     CHAR(1) DEFAULT 'A' NOT NULL
```

---

## 3. Procedures creados

Para la entidad `Negocio`, se crearon los siguientes procedimientos almacenados:

```text
UspListaNegocios
UspConsultaNegocio
UspCreaNegocio
UspActualizaNegocio
UspEliminaNegocio
```

---

# 4. Consideraciones generales

* No se realiza eliminación física de registros.
* La eliminación se maneja mediante borrado lógico usando el campo `NEG_ESTADO`.
* Los estados válidos son:

  * `A`: Activo.
  * `I`: Inactivo.
* El procedimiento de creación exige todos los campos obligatorios.
* El procedimiento de actualización permite edición parcial.
* Si un campo no se envía en la actualización, conserva su valor actual.
* Los procedimientos de escritura usan `TRY/CATCH`.
* Los procedimientos de escritura usan transacciones para garantizar atomicidad.
* Se valida que no existan negocios duplicados con el mismo NIT.

---

# 5. Detalle de procedures

---

## 5.1. UspListaNegocios

### Objetivo

Listar los negocios registrados en el sistema, permitiendo aplicar filtros opcionales por NIT, nombre o estado.

### Nombre

```sql
dbo.UspListaNegocios
```

### Parámetros de entrada

| Parámetro     |          Tipo | Obligatorio | Descripción                                                  |
| ------------- | ------------: | ----------: | ------------------------------------------------------------ |
| `@NEG_NIT`    | `VARCHAR(20)` |          No | Permite buscar negocios por coincidencia parcial del NIT.    |
| `@NEG_NOMBRE` | `VARCHAR(80)` |          No | Permite buscar negocios por coincidencia parcial del nombre. |
| `@NEG_ESTADO` |     `CHAR(1)` |          No | Permite filtrar por estado: `A` o `I`.                       |

### Resultado

Retorna un listado de negocios registrados.

### Campos retornados

| Campo           | Descripción                        |
| --------------- | ---------------------------------- |
| `NEG_ID`        | Identificador del negocio.         |
| `NEG_NIT`       | NIT del negocio.                   |
| `NEG_NOMBRE`    | Nombre o razón social del negocio. |
| `NEG_DIRECCION` | Dirección del negocio.             |
| `NEG_TELEFONO`  | Teléfono del negocio.              |
| `NEG_ESTADO`    | Estado del negocio.                |

### Ejemplos de uso

Listar todos los negocios:

```sql
EXEC dbo.UspListaNegocios;
```

Listar negocios activos:

```sql
EXEC dbo.UspListaNegocios
    @NEG_ESTADO = 'A';
```

Buscar por nombre:

```sql
EXEC dbo.UspListaNegocios
    @NEG_NOMBRE = 'UTP POS';
```

Buscar por NIT:

```sql
EXEC dbo.UspListaNegocios
    @NEG_NIT = '900';
```

---

## 5.2. UspConsultaNegocio

### Objetivo

Consultar la información de un negocio específico mediante su identificador.

### Nombre

```sql
dbo.UspConsultaNegocio
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                            |
| --------- | ----: | ----------: | -------------------------------------- |
| `@NEG_ID` | `INT` |          Sí | Identificador del negocio a consultar. |

### Resultado

Retorna la información completa del negocio consultado.

### Campos retornados

| Campo           | Descripción                        |
| --------------- | ---------------------------------- |
| `NEG_ID`        | Identificador del negocio.         |
| `NEG_NIT`       | NIT del negocio.                   |
| `NEG_NOMBRE`    | Nombre o razón social del negocio. |
| `NEG_DIRECCION` | Dirección del negocio.             |
| `NEG_TELEFONO`  | Teléfono del negocio.              |
| `NEG_ESTADO`    | Estado del negocio.                |

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaNegocio
    @NEG_ID = 1;
```

---

## 5.3. UspCreaNegocio

### Objetivo

Crear un nuevo registro de negocio.

Este procedimiento se utiliza únicamente para inserción. Todos los campos obligatorios deben ser enviados.

### Nombre

```sql
dbo.UspCreaNegocio
```

### Parámetros de entrada

| Parámetro        |           Tipo | Obligatorio | Descripción                           |
| ---------------- | -------------: | ----------: | ------------------------------------- |
| `@NEG_NIT`       |  `VARCHAR(20)` |          Sí | NIT del negocio.                      |
| `@NEG_NOMBRE`    |  `VARCHAR(80)` |          Sí | Nombre o razón social del negocio.    |
| `@NEG_DIRECCION` | `VARCHAR(100)` |          Sí | Dirección del negocio.                |
| `@NEG_TELEFONO`  |  `VARCHAR(20)` |          No | Teléfono del negocio.                 |
| `@NEG_ESTADO`    |      `CHAR(1)` |          No | Estado del negocio. Por defecto: `A`. |

### Validaciones realizadas

El procedimiento valida que:

1. El NIT del negocio sea obligatorio.
2. El nombre del negocio sea obligatorio.
3. La dirección del negocio sea obligatoria.
4. El estado sea válido: `A` o `I`.
5. No exista otro negocio registrado con el mismo NIT.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `NEG_ID`  | Identificador del negocio creado.                         |

### Ejemplo de uso

```sql
EXEC dbo.UspCreaNegocio
    @NEG_NIT = '900123456',
    @NEG_NOMBRE = 'UTP POS Demo',
    @NEG_DIRECCION = 'Calle 10 # 20-30',
    @NEG_TELEFONO = '3001234567',
    @NEG_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                       | NEG_ID
--------|-------------------------------|-------
1       | Negocio creado correctamente. | 1
```

---

## 5.4. UspActualizaNegocio

### Objetivo

Actualizar la información de un negocio existente.

Este procedimiento permite edición parcial. Si un campo no se envía, conserva el valor actual almacenado en la base de datos.

### Nombre

```sql
dbo.UspActualizaNegocio
```

### Parámetros de entrada

| Parámetro           |           Tipo | Obligatorio | Descripción                                                          |
| ------------------- | -------------: | ----------: | -------------------------------------------------------------------- |
| `@NEG_ID`           |          `INT` |          Sí | Identificador del negocio a actualizar.                              |
| `@NEG_NIT`          |  `VARCHAR(20)` |          No | Nuevo NIT del negocio.                                               |
| `@NEG_NOMBRE`       |  `VARCHAR(80)` |          No | Nuevo nombre del negocio.                                            |
| `@NEG_DIRECCION`    | `VARCHAR(100)` |          No | Nueva dirección del negocio.                                         |
| `@NEG_TELEFONO`     |  `VARCHAR(20)` |          No | Nuevo teléfono del negocio.                                          |
| `@NEG_ESTADO`       |      `CHAR(1)` |          No | Nuevo estado del negocio: `A` o `I`.                                 |
| `@LIMPIAR_TELEFONO` |          `BIT` |          No | Si es `1`, limpia el teléfono y lo deja en `NULL`. Por defecto: `0`. |

### Validaciones realizadas

El procedimiento valida que:

1. El identificador del negocio sea obligatorio.
2. El negocio exista.
3. Si se envía un estado, sea válido: `A` o `I`.
4. Si se envía un nuevo NIT, no exista otro negocio con el mismo NIT.

### Comportamiento de actualización parcial

Si un parámetro llega como `NULL`, no se modifica el valor actual.

Ejemplo: si solo se envía `@NEG_NOMBRE`, únicamente se actualiza el nombre.

```sql
EXEC dbo.UspActualizaNegocio
    @NEG_ID = 1,
    @NEG_NOMBRE = 'UTP POS Demo Actualizado';
```

### Limpieza de teléfono

Como `NULL` significa "no actualizar este campo", se usa el parámetro `@LIMPIAR_TELEFONO` para indicar explícitamente que se desea borrar el teléfono.

```sql
EXEC dbo.UspActualizaNegocio
    @NEG_ID = 1,
    @LIMPIAR_TELEFONO = 1;
```

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `NEG_ID`  | Identificador del negocio actualizado.                    |

### Ejemplo de actualización parcial

```sql
EXEC dbo.UspActualizaNegocio
    @NEG_ID = 1,
    @NEG_NOMBRE = 'UTP POS Demo Actualizado';
```

### Ejemplo de actualización de varios campos

```sql
EXEC dbo.UspActualizaNegocio
    @NEG_ID = 1,
    @NEG_DIRECCION = 'Carrera 15 # 25-40',
    @NEG_TELEFONO = '3111234567',
    @NEG_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                            | NEG_ID
--------|------------------------------------|-------
1       | Negocio actualizado correctamente. | 1
```

---

## 5.5. UspEliminaNegocio

### Objetivo

Inactivar un negocio mediante borrado lógico.

Este procedimiento no elimina físicamente el registro de la tabla. En su lugar, actualiza el campo `NEG_ESTADO` a `I`.

### Nombre

```sql
dbo.UspEliminaNegocio
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                            |
| --------- | ----: | ----------: | -------------------------------------- |
| `@NEG_ID` | `INT` |          Sí | Identificador del negocio a inactivar. |

### Validaciones realizadas

El procedimiento valida que el negocio exista antes de inactivarlo.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `NEG_ID`  | Identificador del negocio inactivado.                     |

### Ejemplo de uso

```sql
EXEC dbo.UspEliminaNegocio
    @NEG_ID = 1;
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                            | NEG_ID
--------|------------------------------------|-------
1       | Negocio inactivado correctamente.  | 1
```

---

# 6. Flujo recomendado de prueba

Para validar el correcto funcionamiento de los procedimientos, se recomienda ejecutar el siguiente flujo.

## 6.1. Crear un negocio

```sql
EXEC dbo.UspCreaNegocio
    @NEG_NIT = '900123456',
    @NEG_NOMBRE = 'UTP POS Demo',
    @NEG_DIRECCION = 'Calle 10 # 20-30',
    @NEG_TELEFONO = '3001234567',
    @NEG_ESTADO = 'A';
```

## 6.2. Listar negocios

```sql
EXEC dbo.UspListaNegocios;
```

## 6.3. Consultar un negocio específico

```sql
EXEC dbo.UspConsultaNegocio
    @NEG_ID = 1;
```

## 6.4. Actualizar parcialmente un negocio

```sql
EXEC dbo.UspActualizaNegocio
    @NEG_ID = 1,
    @NEG_NOMBRE = 'UTP POS Demo Actualizado';
```

## 6.5. Actualizar varios campos del negocio

```sql
EXEC dbo.UspActualizaNegocio
    @NEG_ID = 1,
    @NEG_DIRECCION = 'Carrera 15 # 25-40',
    @NEG_TELEFONO = '3111234567';
```

## 6.6. Inactivar un negocio

```sql
EXEC dbo.UspEliminaNegocio
    @NEG_ID = 1;
```

## 6.7. Listar solo negocios activos

```sql
EXEC dbo.UspListaNegocios
    @NEG_ESTADO = 'A';
```

---

# 7. Reglas de negocio aplicadas

## 7.1. Identificación única por NIT

No se permite crear dos negocios con el mismo NIT.

Ejemplo no permitido:

```text
Negocio 1:
NIT: 900123456

Negocio 2:
NIT: 900123456
```

---

## 7.2. Borrado lógico

Los negocios no se eliminan físicamente de la base de datos.

Cuando se ejecuta `UspEliminaNegocio`, el registro queda con estado `I`.

Esto permite conservar trazabilidad y evitar afectaciones a usuarios, productos, terceros, compras, ventas, inventario o Kardex relacionados con el negocio.

---

## 7.3. Actualización parcial

El procedimiento `UspActualizaNegocio` permite actualizar solo los campos enviados.

Ejemplo:

```sql
EXEC dbo.UspActualizaNegocio
    @NEG_ID = 1,
    @NEG_NOMBRE = 'Nuevo nombre';
```

En este caso, se conserva el NIT, dirección, teléfono y estado existentes.

---

# 8. Manejo de errores

Los procedures de escritura utilizan `TRY/CATCH`.

Si ocurre un error:

1. Se revierte la transacción con `ROLLBACK`.
2. Se retorna una respuesta con `Exitoso = 0`.
3. Se muestra el mensaje del error en el campo `Mensaje`.

Ejemplo:

```text
Exitoso = 0
Mensaje = Ya existe un negocio registrado con el mismo NIT.
```

---

# 9. Consideraciones para Backend

Desde el Backend, estos procedures pueden ser consumidos desde la capa de repositorio usando Dapper.

## 9.1. Uso esperado

```text
Repository -> Ejecuta Procedure
Application Service -> Aplica reglas adicionales si se requieren
Controller -> Expone endpoint REST
```

## 9.2. Endpoints sugeridos

| Método HTTP | Endpoint sugerido    | Procedure             |
| ----------- | -------------------- | --------------------- |
| `GET`       | `/api/negocios`      | `UspListaNegocios`    |
| `GET`       | `/api/negocios/{id}` | `UspConsultaNegocio`  |
| `POST`      | `/api/negocios`      | `UspCreaNegocio`      |
| `PATCH`     | `/api/negocios/{id}` | `UspActualizaNegocio` |
| `DELETE`    | `/api/negocios/{id}` | `UspEliminaNegocio`   |

> Si el Backend decide trabajar con actualización completa mediante `PUT`, debe enviar todos los campos. Sin embargo, este procedure está diseñado para actualización parcial, por eso encaja mejor con `PATCH`.

---

# 10. Notas técnicas

* Ejecutar cada `CREATE OR ALTER PROCEDURE` por separado si la herramienta presenta problemas con separadores de lote.
* Evitar el uso de `GO` en DBeaver si genera errores de sintaxis.
* Mantener los nombres de procedures bajo la convención definida.
* Mantener el borrado lógico como estándar del sistema.
* No consumir directamente la tabla `TBL_NEGOCIO` desde el Backend si ya existen procedures definidos para su operación.
* `UspCreaNegocio` y `UspActualizaNegocio` están separados para evitar ambigüedad entre creación, actualización completa y actualización parcial.
