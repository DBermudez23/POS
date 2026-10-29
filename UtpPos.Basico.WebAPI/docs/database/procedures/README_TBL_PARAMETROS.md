# README - Procedures de Parámetros

## 1. Objetivo

Este documento describe los procedimientos almacenados creados para la gestión de la tabla `TBL_PARAMETROS` dentro de la base de datos del Sistema de Ventas.

La tabla `TBL_PARAMETROS` permite almacenar valores parametrizables del sistema, asociados a una clase definida en `TBL_CLASE`. Estos parámetros son utilizados para representar información configurable como tipos de documento, categorías de producto, medios de pago, estados, ciudades, departamentos, tipos de movimiento de inventario, entre otros.

Los procedimientos almacenados definidos permiten listar, consultar, crear, actualizar e inactivar lógicamente registros de parámetros.

---

## 2. Tabla relacionada

### Tabla principal

```sql
TBL_PARAMETROS
```

### Estructura base

```sql
PARAM_ID              INT IDENTITY(1,1) PRIMARY KEY
PARAM_CLASE_ID        INT NOT NULL
PARAM_DESCRIPCION     VARCHAR(100) NOT NULL
PARAM_ESTADO          CHAR(1) DEFAULT 'A' NOT NULL
PARAM_FECHA_CREACION  DATETIME NOT NULL
```

### Relación principal

```sql
TBL_PARAMETROS.PARAM_CLASE_ID -> TBL_CLASE.CLA_ID
```

Cada parámetro debe pertenecer obligatoriamente a una clase existente.

---

## 3. Convenciones utilizadas

Los procedures siguen la siguiente convención de nombres:

```text
UspLista[Entidad]
UspConsulta[Entidad]
UspCrea[Entidad]
UspActualiza[Entidad]
UspElimina[Entidad]
```

Para la entidad `Parametros`, se crearon los siguientes procedimientos:

```text
UspListaParametros
UspConsultaParametro
UspCreaParametro
UspActualizaParametro
UspEliminaParametro
```

---

## 4. Consideraciones generales

* No se realiza eliminación física de registros.
* La eliminación se maneja mediante borrado lógico usando el campo `PARAM_ESTADO`.
* Los estados válidos son:

  * `A`: Activo.
  * `I`: Inactivo.
* El procedimiento de creación exige todos los campos obligatorios para registrar un nuevo parámetro.
* El procedimiento de actualización permite edición parcial.
* Si un campo no se envía en la actualización, conserva su valor actual.
* Los procedimientos de escritura usan bloques `TRY/CATCH`.
* Los procedimientos de escritura usan transacciones para garantizar atomicidad.
* La descripción del parámetro se limpia usando `LTRIM` y `RTRIM`.
* Se valida que no existan parámetros duplicados dentro de la misma clase.
* Se valida que la clase asociada exista y esté activa antes de crear o actualizar un parámetro.
* `UspCreaParametro` y `UspActualizaParametro` reemplazan el comportamiento anterior de `UspAlmacenaParametro`.

---

# 5. Procedures creados

---

## 5.1. UspListaParametros

### Objetivo

Listar los parámetros registrados en el sistema, permitiendo aplicar filtros opcionales por clase, estado o descripción.

### Nombre

```sql
dbo.UspListaParametros
```

### Parámetros de entrada

| Parámetro            |           Tipo | Obligatorio | Descripción                                                           |
| -------------------- | -------------: | ----------: | --------------------------------------------------------------------- |
| `@PARAM_CLASE_ID`    |          `INT` |          No | Permite filtrar los parámetros por una clase específica.              |
| `@PARAM_ESTADO`      |      `CHAR(1)` |          No | Permite filtrar por estado: `A` o `I`.                                |
| `@PARAM_DESCRIPCION` | `VARCHAR(100)` |          No | Permite buscar parámetros por coincidencia parcial en la descripción. |

### Resultado

Retorna un listado de parámetros junto con la descripción de la clase asociada.

### Campos retornados

| Campo                  | Descripción                         |
| ---------------------- | ----------------------------------- |
| `PARAM_ID`             | Identificador del parámetro.        |
| `PARAM_CLASE_ID`       | Identificador de la clase asociada. |
| `CLASE_DESCRIPCION`    | Descripción de la clase asociada.   |
| `PARAM_DESCRIPCION`    | Descripción del parámetro.          |
| `PARAM_ESTADO`         | Estado del parámetro.               |
| `PARAM_FECHA_CREACION` | Fecha de creación del parámetro.    |

### Ejemplos de uso

Listar todos los parámetros:

```sql
EXEC dbo.UspListaParametros;
```

Listar parámetros de una clase específica:

```sql
EXEC dbo.UspListaParametros
    @PARAM_CLASE_ID = 1;
```

Listar solo parámetros activos:

```sql
EXEC dbo.UspListaParametros
    @PARAM_ESTADO = 'A';
```

Buscar parámetros por descripción:

```sql
EXEC dbo.UspListaParametros
    @PARAM_DESCRIPCION = 'Cédula';
```

---

## 5.2. UspConsultaParametro

### Objetivo

Consultar la información de un parámetro específico mediante su identificador.

### Nombre

```sql
dbo.UspConsultaParametro
```

### Parámetros de entrada

| Parámetro   |  Tipo | Obligatorio | Descripción                              |
| ----------- | ----: | ----------: | ---------------------------------------- |
| `@PARAM_ID` | `INT` |          Sí | Identificador del parámetro a consultar. |

### Resultado

Retorna la información completa del parámetro consultado, incluyendo la descripción de su clase asociada.

### Campos retornados

| Campo                  | Descripción                         |
| ---------------------- | ----------------------------------- |
| `PARAM_ID`             | Identificador del parámetro.        |
| `PARAM_CLASE_ID`       | Identificador de la clase asociada. |
| `CLASE_DESCRIPCION`    | Descripción de la clase asociada.   |
| `PARAM_DESCRIPCION`    | Descripción del parámetro.          |
| `PARAM_ESTADO`         | Estado del parámetro.               |
| `PARAM_FECHA_CREACION` | Fecha de creación del parámetro.    |

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaParametro
    @PARAM_ID = 1;
```

---

## 5.3. UspCreaParametro

### Objetivo

Crear un nuevo parámetro asociado a una clase existente.

Este procedimiento se utiliza únicamente para inserción. No actualiza registros existentes.

### Nombre

```sql
dbo.UspCreaParametro
```

### Parámetros de entrada

| Parámetro            |           Tipo | Obligatorio | Descripción                                                |
| -------------------- | -------------: | ----------: | ---------------------------------------------------------- |
| `@PARAM_CLASE_ID`    |          `INT` |          Sí | Identificador de la clase a la que pertenece el parámetro. |
| `@PARAM_DESCRIPCION` | `VARCHAR(100)` |          Sí | Descripción del parámetro.                                 |
| `@PARAM_ESTADO`      |      `CHAR(1)` |          No | Estado del parámetro. Por defecto: `A`.                    |

### Validaciones realizadas

El procedimiento valida que:

1. La clase del parámetro sea obligatoria.
2. La descripción del parámetro no esté vacía.
3. El estado sea válido: `A` o `I`.
4. La clase asociada exista y esté activa en `TBL_CLASE`.
5. No exista otro parámetro con la misma descripción dentro de la misma clase.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo      | Descripción                                               |
| ---------- | --------------------------------------------------------- |
| `Exitoso`  | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje`  | Mensaje descriptivo del resultado.                        |
| `PARAM_ID` | Identificador del parámetro creado.                       |

### Ejemplo de creación

```sql
EXEC dbo.UspCreaParametro
    @PARAM_CLASE_ID = 1,
    @PARAM_DESCRIPCION = 'Cédula ciudadanía',
    @PARAM_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                         | PARAM_ID
--------|---------------------------------|---------
1       | Parámetro creado correctamente. | 1
```

### Ejemplo de respuesta con error

```text
Exitoso | Mensaje                                                                    | PARAM_ID
--------|----------------------------------------------------------------------------|---------
0       | Ya existe un parámetro con la misma descripción para la clase seleccionada. | NULL
```

---

## 5.4. UspActualizaParametro

### Objetivo

Actualizar un parámetro existente.

Este procedimiento permite edición parcial. Si un campo no se envía, conserva el valor actual almacenado en la base de datos.

### Nombre

```sql
dbo.UspActualizaParametro
```

### Parámetros de entrada

| Parámetro            |           Tipo | Obligatorio | Descripción                               |
| -------------------- | -------------: | ----------: | ----------------------------------------- |
| `@PARAM_ID`          |          `INT` |          Sí | Identificador del parámetro a actualizar. |
| `@PARAM_CLASE_ID`    |          `INT` |          No | Nueva clase asociada al parámetro.        |
| `@PARAM_DESCRIPCION` | `VARCHAR(100)` |          No | Nueva descripción del parámetro.          |
| `@PARAM_ESTADO`      |      `CHAR(1)` |          No | Nuevo estado del parámetro: `A` o `I`.    |

### Validaciones realizadas

El procedimiento valida que:

1. El identificador del parámetro sea obligatorio.
2. El parámetro exista.
3. Si se envía una nueva clase, esta exista y esté activa.
4. Si se envía una nueva descripción, no quede vacía.
5. Si se envía un nuevo estado, sea válido: `A` o `I`.
6. No exista otro parámetro con la misma descripción dentro de la misma clase final.

### Comportamiento de actualización parcial

Si un parámetro llega como `NULL`, no se modifica el valor actual.

Ejemplo: si solo se envía `@PARAM_DESCRIPCION`, únicamente se actualiza la descripción.

```sql
EXEC dbo.UspActualizaParametro
    @PARAM_ID = 1,
    @PARAM_DESCRIPCION = 'Cédula de ciudadanía';
```

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo      | Descripción                                               |
| ---------- | --------------------------------------------------------- |
| `Exitoso`  | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje`  | Mensaje descriptivo del resultado.                        |
| `PARAM_ID` | Identificador del parámetro actualizado.                  |

### Ejemplo de actualización parcial

```sql
EXEC dbo.UspActualizaParametro
    @PARAM_ID = 1,
    @PARAM_DESCRIPCION = 'Cédula de ciudadanía';
```

### Ejemplo de actualización de varios campos

```sql
EXEC dbo.UspActualizaParametro
    @PARAM_ID = 1,
    @PARAM_CLASE_ID = 1,
    @PARAM_DESCRIPCION = 'Número de Identificación Tributaria',
    @PARAM_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                              | PARAM_ID
--------|--------------------------------------|---------
1       | Parámetro actualizado correctamente. | 1
```

### Ejemplo de respuesta con error

```text
Exitoso | Mensaje                                                   | PARAM_ID
--------|-----------------------------------------------------------|---------
0       | La clase asociada al parámetro no existe o se encuentra inactiva. | 1
```

---

## 5.5. UspEliminaParametro

### Objetivo

Inactivar un parámetro mediante borrado lógico.

Este procedimiento no elimina físicamente el registro de la tabla. En su lugar, actualiza el campo `PARAM_ESTADO` a `I`.

### Nombre

```sql
dbo.UspEliminaParametro
```

### Parámetros de entrada

| Parámetro   |  Tipo | Obligatorio | Descripción                              |
| ----------- | ----: | ----------: | ---------------------------------------- |
| `@PARAM_ID` | `INT` |          Sí | Identificador del parámetro a inactivar. |

### Validaciones realizadas

El procedimiento valida que el parámetro exista antes de inactivarlo.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo      | Descripción                                               |
| ---------- | --------------------------------------------------------- |
| `Exitoso`  | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje`  | Mensaje descriptivo del resultado.                        |
| `PARAM_ID` | Identificador del parámetro inactivado.                   |

### Ejemplo de uso

```sql
EXEC dbo.UspEliminaParametro
    @PARAM_ID = 1;
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                              | PARAM_ID
--------|--------------------------------------|---------
1       | Parámetro inactivado correctamente. | 1
```

---

# 6. Flujo recomendado de prueba

Para validar el correcto funcionamiento de los procedimientos, se recomienda ejecutar el siguiente flujo:

## 6.1. Crear un parámetro

```sql
EXEC dbo.UspCreaParametro
    @PARAM_CLASE_ID = 1,
    @PARAM_DESCRIPCION = 'NIT',
    @PARAM_ESTADO = 'A';
```

## 6.2. Listar parámetros

```sql
EXEC dbo.UspListaParametros;
```

## 6.3. Consultar un parámetro específico

```sql
EXEC dbo.UspConsultaParametro
    @PARAM_ID = 1;
```

## 6.4. Actualizar parcialmente un parámetro

```sql
EXEC dbo.UspActualizaParametro
    @PARAM_ID = 1,
    @PARAM_DESCRIPCION = 'Número de Identificación Tributaria';
```

## 6.5. Actualizar varios campos de un parámetro

```sql
EXEC dbo.UspActualizaParametro
    @PARAM_ID = 1,
    @PARAM_CLASE_ID = 1,
    @PARAM_DESCRIPCION = 'NIT',
    @PARAM_ESTADO = 'A';
```

## 6.6. Inactivar un parámetro

```sql
EXEC dbo.UspEliminaParametro
    @PARAM_ID = 1;
```

## 6.7. Listar solo parámetros activos

```sql
EXEC dbo.UspListaParametros
    @PARAM_ESTADO = 'A';
```

---

# 7. Reglas de negocio aplicadas

## 7.1. Relación con clases

Todo parámetro debe estar asociado a una clase existente y activa en `TBL_CLASE`.

Ejemplo:

```text
Clase: Tipo Documento
Parámetros:
- Cédula de ciudadanía
- NIT
- Pasaporte
```

```text
Clase: Medio de Pago
Parámetros:
- Efectivo
- Tarjeta débito
- Transferencia
```

```text
Clase: Tipo Movimiento Inventario
Parámetros:
- Entrada por compra
- Salida por venta
- Ajuste positivo
- Ajuste negativo
```

---

## 7.2. Borrado lógico

Los parámetros no se eliminan físicamente de la base de datos.

Cuando se ejecuta `UspEliminaParametro`, el registro queda con estado `I`.

Esto permite conservar trazabilidad y evitar afectar registros históricos que ya estén relacionados con otras tablas.

---

## 7.3. Prevención de duplicados

No se permite crear dos parámetros con la misma descripción dentro de una misma clase.

Ejemplo no permitido:

```text
Clase: Tipo Documento
Parámetro: Cédula de ciudadanía
Parámetro: Cédula de ciudadanía
```

Sin embargo, una misma descripción podría existir en clases diferentes si el negocio lo permite.

---

## 7.4. Separación entre creación y actualización

La creación y la actualización se manejan con procedures separados:

```text
UspCreaParametro
UspActualizaParametro
```

Esto evita ambigüedad entre insertar un nuevo registro y modificar uno existente.

`UspCreaParametro` exige los datos necesarios para crear un parámetro nuevo.

`UspActualizaParametro` permite modificar parcialmente un parámetro existente, conservando los valores actuales de los campos que no se envían.

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
Mensaje = La clase asociada al parámetro no existe o se encuentra inactiva.
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

| Método HTTP | Endpoint sugerido      | Procedure               |
| ----------- | ---------------------- | ----------------------- |
| `GET`       | `/api/parametros`      | `UspListaParametros`    |
| `GET`       | `/api/parametros/{id}` | `UspConsultaParametro`  |
| `POST`      | `/api/parametros`      | `UspCreaParametro`      |
| `PATCH`     | `/api/parametros/{id}` | `UspActualizaParametro` |
| `DELETE`    | `/api/parametros/{id}` | `UspEliminaParametro`   |

> Si el Backend decide trabajar con actualización completa mediante `PUT`, debe enviar todos los campos. Sin embargo, este procedure está diseñado para actualización parcial, por eso encaja mejor con `PATCH`.

---

# 10. Notas técnicas

* Ejecutar cada `CREATE OR ALTER PROCEDURE` por separado en DBeaver si la herramienta presenta problemas con separadores de lote.
* Evitar el uso de `GO` en DBeaver si genera errores de sintaxis.
* Mantener los nombres de procedures bajo la convención definida.
* Mantener el borrado lógico como estándar del sistema.
* No consumir directamente la tabla `TBL_PARAMETROS` desde el Backend si ya existen procedures definidos para su operación.
* Crear primero las clases mediante `UspCreaClase` antes de crear parámetros asociados.
* `UspCreaParametro` y `UspActualizaParametro` reemplazan el comportamiento anterior de `UspAlmacenaParametro`.
