# README - Procedures de Inventario

## 1. Objetivo

Este documento describe los procedimientos almacenados creados para la gestión de la tabla `TBL_INVENTARIO` dentro de la base de datos del Sistema de Ventas.

La tabla `TBL_INVENTARIO` representa la ficha de inventario de un producto dentro de un negocio. Permite controlar stock actual, stock mínimo, stock máximo, costo ponderado, último costo y estado del inventario.

Los procedimientos almacenados definidos permiten listar, consultar, crear, actualizar e inactivar lógicamente registros de inventario.

> Importante: estos procedures administran la ficha o configuración del inventario. Los movimientos reales de entrada, salida, compra, venta o ajuste deben manejarse posteriormente mediante procesos transaccionales y `TBL_KARDEX`.

---

## 2. Tabla relacionada

### Tabla principal

```sql
TBL_INVENTARIO
```

### Estructura base

```sql
INV_ID                         INT IDENTITY(1,1) PRIMARY KEY
INV_NEGOCIO_ID                 INT NOT NULL
INV_PROD_ID                    INT NOT NULL
INV_STOCK_ACTUAL               DECIMAL(18,4) NOT NULL
INV_STOCK_MINIMO               DECIMAL(18,4) NOT NULL
INV_STOCK_MAX                  DECIMAL(18,4) NOT NULL
INV_COSTO_PONDERADO            DECIMAL(18,2) NOT NULL
INV_ULTIMO_COSTO               DECIMAL(18,2) NOT NULL
INV_ESTADO                     CHAR(1) DEFAULT 'A' NOT NULL
INV_FECHA_ACTUALIZACION        DATETIME NULL
INV_MODIFICADO_POR_USUARIO_ID  INT NULL
INV_CREADO_POR_USUARIO_ID      INT NOT NULL
INV_FECHA_CREADO               DATETIME NOT NULL
```

---

## 3. Relaciones principales

La tabla `TBL_INVENTARIO` se relaciona con las siguientes tablas:

```sql
TBL_INVENTARIO.INV_NEGOCIO_ID -> TBL_NEGOCIO.NEG_ID
TBL_INVENTARIO.INV_PROD_ID -> TBL_PRODUCTOS.PROD_ID
TBL_INVENTARIO.INV_CREADO_POR_USUARIO_ID -> TBL_USUARIOS.USR_ID
TBL_INVENTARIO.INV_MODIFICADO_POR_USUARIO_ID -> TBL_USUARIOS.USR_ID
```

Estas relaciones permiten asociar cada registro de inventario a un negocio, producto, usuario creador y usuario modificador.

---

## 4. Procedures creados

Para la entidad `Inventario`, se crearon los siguientes procedimientos almacenados:

```text
UspListaInventario
UspConsultaInventario
UspCreaInventario
UspActualizaInventario
UspEliminaInventario
```

---

# 5. Consideraciones generales

* No se realiza eliminación física de registros.
* La eliminación se maneja mediante borrado lógico usando el campo `INV_ESTADO`.
* Los estados válidos son:

  * `A`: Activo.
  * `I`: Inactivo.
* El procedimiento de creación exige todos los campos obligatorios para registrar una ficha de inventario.
* El procedimiento de actualización permite edición parcial.
* Si un campo no se envía en la actualización, conserva su valor actual.
* Se valida que el negocio exista y esté activo.
* Se valida que el producto exista y esté activo.
* Se valida que el usuario creador exista y esté activo.
* Se valida que el usuario modificador exista y esté activo al actualizar o inactivar.
* No se permite crear más de un registro de inventario para el mismo producto dentro del mismo negocio.
* Los valores de stock y costos no pueden ser negativos.
* El stock mínimo no puede ser mayor que el stock máximo cuando el stock máximo es mayor que cero.
* Los procedures de escritura usan `TRY/CATCH`.
* Los procedures de escritura usan transacciones para garantizar atomicidad.

---

# 6. Detalle de procedures

---

## 6.1. UspListaInventario

### Objetivo

Listar los registros de inventario del sistema, permitiendo aplicar filtros opcionales por negocio, producto, estado o nombre del producto.

### Nombre

```sql
dbo.UspListaInventario
```

### Parámetros de entrada

| Parámetro         |           Tipo | Obligatorio | Descripción                                                                 |
| ----------------- | -------------: | ----------: | --------------------------------------------------------------------------- |
| `@INV_NEGOCIO_ID` |          `INT` |          No | Permite filtrar inventario por negocio.                                     |
| `@INV_PROD_ID`    |          `INT` |          No | Permite filtrar inventario por producto.                                    |
| `@INV_ESTADO`     |      `CHAR(1)` |          No | Permite filtrar por estado: `A` o `I`.                                      |
| `@PROD_NOMBRE`    | `VARCHAR(100)` |          No | Permite buscar inventario por coincidencia parcial del nombre del producto. |

### Resultado

Retorna un listado de inventario junto con información descriptiva del negocio, producto, categoría, usuario creador y usuario modificador.

### Campos retornados

| Campo                           | Descripción                                   |
| ------------------------------- | --------------------------------------------- |
| `INV_ID`                        | Identificador del inventario.                 |
| `INV_NEGOCIO_ID`                | Identificador del negocio.                    |
| `NEG_NOMBRE`                    | Nombre del negocio.                           |
| `INV_PROD_ID`                   | Identificador del producto.                   |
| `PROD_NOMBRE`                   | Nombre del producto.                          |
| `PROD_DESCRIPCION`              | Descripción del producto.                     |
| `PROD_CATEGORIA_ID`             | Identificador de la categoría del producto.   |
| `CATEGORIA`                     | Descripción de la categoría.                  |
| `INV_STOCK_ACTUAL`              | Stock actual del producto.                    |
| `INV_STOCK_MINIMO`              | Stock mínimo configurado.                     |
| `INV_STOCK_MAX`                 | Stock máximo configurado.                     |
| `INV_COSTO_PONDERADO`           | Costo promedio ponderado del producto.        |
| `INV_ULTIMO_COSTO`              | Último costo registrado del producto.         |
| `INV_ESTADO`                    | Estado del inventario.                        |
| `INV_FECHA_ACTUALIZACION`       | Fecha de última actualización del inventario. |
| `INV_MODIFICADO_POR_USUARIO_ID` | Identificador del usuario modificador.        |
| `MODIFICADO_POR_USUARIO`        | Nombre del usuario modificador.               |
| `INV_CREADO_POR_USUARIO_ID`     | Identificador del usuario creador.            |
| `CREADO_POR_USUARIO`            | Nombre del usuario creador.                   |
| `INV_FECHA_CREADO`              | Fecha de creación del inventario.             |

### Ejemplos de uso

Listar todo el inventario:

```sql
EXEC dbo.UspListaInventario;
```

Listar inventario activo:

```sql
EXEC dbo.UspListaInventario
    @INV_ESTADO = 'A';
```

Listar inventario de un negocio:

```sql
EXEC dbo.UspListaInventario
    @INV_NEGOCIO_ID = 1;
```

Buscar por nombre de producto:

```sql
EXEC dbo.UspListaInventario
    @PROD_NOMBRE = 'Camiseta';
```

---

## 6.2. UspConsultaInventario

### Objetivo

Consultar la información de un registro de inventario específico mediante su identificador.

### Nombre

```sql
dbo.UspConsultaInventario
```

### Parámetros de entrada

| Parámetro |  Tipo | Obligatorio | Descripción                                           |
| --------- | ----: | ----------: | ----------------------------------------------------- |
| `@INV_ID` | `INT` |          Sí | Identificador del registro de inventario a consultar. |

### Resultado

Retorna la información completa del inventario consultado, incluyendo negocio, producto, categoría, usuario creador y usuario modificador.

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaInventario
    @INV_ID = 1;
```

---

## 6.3. UspCreaInventario

### Objetivo

Crear una nueva ficha de inventario para un producto dentro de un negocio.

Este procedimiento se utiliza únicamente para inserción. No actualiza registros existentes.

### Nombre

```sql
dbo.UspCreaInventario
```

### Parámetros de entrada

| Parámetro                    |            Tipo | Obligatorio | Descripción                                         |
| ---------------------------- | --------------: | ----------: | --------------------------------------------------- |
| `@INV_NEGOCIO_ID`            |           `INT` |          Sí | Identificador del negocio.                          |
| `@INV_PROD_ID`               |           `INT` |          Sí | Identificador del producto.                         |
| `@INV_STOCK_ACTUAL`          | `DECIMAL(18,4)` |          No | Stock actual inicial. Por defecto: `0`.             |
| `@INV_STOCK_MINIMO`          | `DECIMAL(18,4)` |          No | Stock mínimo permitido. Por defecto: `0`.           |
| `@INV_STOCK_MAX`             | `DECIMAL(18,4)` |          No | Stock máximo permitido. Por defecto: `0`.           |
| `@INV_COSTO_PONDERADO`       | `DECIMAL(18,2)` |          No | Costo promedio ponderado inicial. Por defecto: `0`. |
| `@INV_ULTIMO_COSTO`          | `DECIMAL(18,2)` |          No | Último costo inicial. Por defecto: `0`.             |
| `@INV_CREADO_POR_USUARIO_ID` |           `INT` |          Sí | Identificador del usuario que crea el inventario.   |
| `@INV_ESTADO`                |       `CHAR(1)` |          No | Estado del inventario. Por defecto: `A`.            |

### Validaciones realizadas

El procedimiento valida que:

1. El negocio sea obligatorio.
2. El producto sea obligatorio.
3. El usuario creador sea obligatorio.
4. El estado sea válido: `A` o `I`.
5. El stock actual no sea negativo.
6. El stock mínimo no sea negativo.
7. El stock máximo no sea negativo.
8. El stock mínimo no sea mayor que el stock máximo cuando el stock máximo sea mayor que cero.
9. El costo ponderado no sea negativo.
10. El último costo no sea negativo.
11. El negocio exista y esté activo.
12. El producto exista y esté activo.
13. El usuario creador exista y esté activo.
14. No exista otro inventario para el mismo producto y negocio.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `INV_ID`  | Identificador del inventario creado.                      |

### Ejemplo de creación

```sql
EXEC dbo.UspCreaInventario
    @INV_NEGOCIO_ID = 1,
    @INV_PROD_ID = 1,
    @INV_STOCK_ACTUAL = 0,
    @INV_STOCK_MINIMO = 5,
    @INV_STOCK_MAX = 100,
    @INV_COSTO_PONDERADO = 0,
    @INV_ULTIMO_COSTO = 0,
    @INV_CREADO_POR_USUARIO_ID = 1,
    @INV_ESTADO = 'A';
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                         | INV_ID
--------|---------------------------------|-------
1       | Inventario creado correctamente. | 1
```

---

## 6.4. UspActualizaInventario

### Objetivo

Actualizar una ficha de inventario existente.

Este procedimiento permite edición parcial. Si un campo no se envía, conserva el valor actual almacenado en la base de datos.

### Nombre

```sql
dbo.UspActualizaInventario
```

### Parámetros de entrada

| Parámetro                        |            Tipo | Obligatorio | Descripción                                           |
| -------------------------------- | --------------: | ----------: | ----------------------------------------------------- |
| `@INV_ID`                        |           `INT` |          Sí | Identificador del inventario a actualizar.            |
| `@INV_STOCK_ACTUAL`              | `DECIMAL(18,4)` |          No | Nuevo stock actual.                                   |
| `@INV_STOCK_MINIMO`              | `DECIMAL(18,4)` |          No | Nuevo stock mínimo.                                   |
| `@INV_STOCK_MAX`                 | `DECIMAL(18,4)` |          No | Nuevo stock máximo.                                   |
| `@INV_COSTO_PONDERADO`           | `DECIMAL(18,2)` |          No | Nuevo costo promedio ponderado.                       |
| `@INV_ULTIMO_COSTO`              | `DECIMAL(18,2)` |          No | Nuevo último costo.                                   |
| `@INV_ESTADO`                    |       `CHAR(1)` |          No | Nuevo estado del inventario: `A` o `I`.               |
| `@INV_MODIFICADO_POR_USUARIO_ID` |           `INT` |          Sí | Identificador del usuario que modifica el inventario. |

### Validaciones realizadas

El procedimiento valida que:

1. El identificador del inventario sea obligatorio.
2. El usuario modificador sea obligatorio.
3. El inventario exista.
4. El usuario modificador exista y esté activo.
5. Si se envía stock actual, no sea negativo.
6. Si se envía stock mínimo, no sea negativo.
7. Si se envía stock máximo, no sea negativo.
8. El stock mínimo final no sea mayor que el stock máximo final cuando el stock máximo sea mayor que cero.
9. Si se envía costo ponderado, no sea negativo.
10. Si se envía último costo, no sea negativo.
11. Si se envía estado, sea válido: `A` o `I`.

### Comportamiento de actualización parcial

Si un parámetro llega como `NULL`, no se modifica el valor actual.

Ejemplo: si solo se envía `@INV_STOCK_MINIMO`, únicamente se actualiza el stock mínimo.

```sql
EXEC dbo.UspActualizaInventario
    @INV_ID = 1,
    @INV_STOCK_MINIMO = 10,
    @INV_MODIFICADO_POR_USUARIO_ID = 1;
```

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `INV_ID`  | Identificador del inventario actualizado.                 |

### Ejemplo de actualización parcial

```sql
EXEC dbo.UspActualizaInventario
    @INV_ID = 1,
    @INV_STOCK_MINIMO = 10,
    @INV_MODIFICADO_POR_USUARIO_ID = 1;
```

### Ejemplo de actualización de varios campos

```sql
EXEC dbo.UspActualizaInventario
    @INV_ID = 1,
    @INV_STOCK_MINIMO = 10,
    @INV_STOCK_MAX = 200,
    @INV_COSTO_PONDERADO = 25000,
    @INV_ULTIMO_COSTO = 27000,
    @INV_MODIFICADO_POR_USUARIO_ID = 1;
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                              | INV_ID
--------|--------------------------------------|-------
1       | Inventario actualizado correctamente. | 1
```

---

## 6.5. UspEliminaInventario

### Objetivo

Inactivar una ficha de inventario mediante borrado lógico.

Este procedimiento no elimina físicamente el registro de la tabla. En su lugar, actualiza el campo `INV_ESTADO` a `I`.

### Nombre

```sql
dbo.UspEliminaInventario
```

### Parámetros de entrada

| Parámetro                        |  Tipo | Obligatorio | Descripción                                            |
| -------------------------------- | ----: | ----------: | ------------------------------------------------------ |
| `@INV_ID`                        | `INT` |          Sí | Identificador del inventario a inactivar.              |
| `@INV_MODIFICADO_POR_USUARIO_ID` | `INT` |          Sí | Identificador del usuario que realiza la inactivación. |

### Validaciones realizadas

El procedimiento valida que:

1. El identificador del inventario sea obligatorio.
2. El usuario modificador sea obligatorio.
3. El inventario exista.
4. El usuario modificador exista y esté activo.

### Resultado

El procedimiento retorna una respuesta estándar con el resultado de la operación.

| Campo     | Descripción                                               |
| --------- | --------------------------------------------------------- |
| `Exitoso` | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje` | Mensaje descriptivo del resultado.                        |
| `INV_ID`  | Identificador del inventario inactivado.                  |

### Ejemplo de uso

```sql
EXEC dbo.UspEliminaInventario
    @INV_ID = 1,
    @INV_MODIFICADO_POR_USUARIO_ID = 1;
```

### Ejemplo de respuesta exitosa

```text
Exitoso | Mensaje                              | INV_ID
--------|--------------------------------------|-------
1       | Inventario inactivado correctamente. | 1
```

---

# 7. Flujo recomendado de prueba

Para validar el correcto funcionamiento de los procedimientos, se recomienda ejecutar el siguiente flujo.

## 7.1. Crear inventario

```sql
EXEC dbo.UspCreaInventario
    @INV_NEGOCIO_ID = 1,
    @INV_PROD_ID = 1,
    @INV_STOCK_ACTUAL = 0,
    @INV_STOCK_MINIMO = 5,
    @INV_STOCK_MAX = 100,
    @INV_COSTO_PONDERADO = 0,
    @INV_ULTIMO_COSTO = 0,
    @INV_CREADO_POR_USUARIO_ID = 1,
    @INV_ESTADO = 'A';
```

## 7.2. Listar inventario

```sql
EXEC dbo.UspListaInventario;
```

## 7.3. Consultar un inventario específico

```sql
EXEC dbo.UspConsultaInventario
    @INV_ID = 1;
```

## 7.4. Actualizar stock mínimo

```sql
EXEC dbo.UspActualizaInventario
    @INV_ID = 1,
    @INV_STOCK_MINIMO = 10,
    @INV_MODIFICADO_POR_USUARIO_ID = 1;
```

## 7.5. Actualizar varios campos

```sql
EXEC dbo.UspActualizaInventario
    @INV_ID = 1,
    @INV_STOCK_MINIMO = 10,
    @INV_STOCK_MAX = 200,
    @INV_COSTO_PONDERADO = 25000,
    @INV_ULTIMO_COSTO = 27000,
    @INV_MODIFICADO_POR_USUARIO_ID = 1;
```

## 7.6. Inactivar inventario

```sql
EXEC dbo.UspEliminaInventario
    @INV_ID = 1,
    @INV_MODIFICADO_POR_USUARIO_ID = 1;
```

## 7.7. Listar solo inventario activo

```sql
EXEC dbo.UspListaInventario
    @INV_ESTADO = 'A';
```

---

# 8. Reglas de negocio aplicadas

## 8.1. Inventario por negocio y producto

No se permite crear más de un registro de inventario para el mismo producto dentro del mismo negocio.

Ejemplo no permitido:

```text
Negocio: 1
Producto: Camiseta básica

Negocio: 1
Producto: Camiseta básica
```

Esto garantiza que cada producto tenga una única ficha de inventario por negocio.

---

## 8.2. Producto activo

Solo se puede crear inventario para productos activos.

```text
PROD_ESTADO = 'A'
```

Esto evita crear inventario para productos inactivos o fuera del catálogo operativo.

---

## 8.3. Negocio activo

Solo se puede crear inventario para negocios activos.

```text
NEG_ESTADO = 'A'
```

Esto evita asociar inventario a negocios inactivos.

---

## 8.4. Usuario creador y modificador

El inventario debe registrar el usuario que lo creó y el usuario que lo modificó por última vez.

* `INV_CREADO_POR_USUARIO_ID`: usuario que crea la ficha de inventario.
* `INV_MODIFICADO_POR_USUARIO_ID`: usuario que modifica o inactiva la ficha de inventario.

---

## 8.5. Stock actual

El stock actual representa la cantidad disponible del producto en inventario.

```text
INV_STOCK_ACTUAL >= 0
```

Este valor no puede ser negativo.

---

## 8.6. Stock mínimo y máximo

El stock mínimo representa el punto de alerta para reposición.

El stock máximo representa el límite recomendado de existencia.

```text
INV_STOCK_MINIMO >= 0
INV_STOCK_MAX >= 0
```

Cuando `INV_STOCK_MAX` es mayor que cero, el stock mínimo no puede ser mayor que el stock máximo.

---

## 8.7. Costos de inventario

El inventario maneja dos costos:

```text
INV_COSTO_PONDERADO
INV_ULTIMO_COSTO
```

Ambos valores no pueden ser negativos.

El costo ponderado debe actualizarse de forma controlada en procesos posteriores de compra, entrada o ajuste de inventario.

---

## 8.8. Movimientos reales de inventario

Aunque `UspActualizaInventario` permite modificar stock y costos, en un sistema productivo los cambios reales de stock deberían realizarse mediante movimientos controlados, compras, ventas, ajustes y registros en `TBL_KARDEX`.

Este CRUD debe entenderse como administración de ficha de inventario, no como reemplazo del histórico de movimientos.

---

## 8.9. Borrado lógico

Los registros de inventario no se eliminan físicamente de la base de datos.

Cuando se ejecuta `UspEliminaInventario`, el registro queda con estado `I`.

Esto permite conservar trazabilidad y evitar inconsistencias con productos, compras, ventas o Kardex.

---

## 8.10. Separación entre creación y actualización

La creación y la actualización se manejan con procedures separados:

```text
UspCreaInventario
UspActualizaInventario
```

Esto evita ambigüedad entre insertar un nuevo registro y modificar uno existente.

`UspCreaInventario` exige los datos necesarios para crear una ficha de inventario nueva.

`UspActualizaInventario` permite modificar parcialmente un inventario existente, conservando los valores actuales de los campos que no se envían.

---

# 9. Manejo de errores

Los procedures de escritura utilizan `TRY/CATCH`.

Si ocurre un error:

1. Se revierte la transacción con `ROLLBACK`.
2. Se retorna una respuesta con `Exitoso = 0`.
3. Se muestra el mensaje del error en el campo `Mensaje`.

Ejemplo:

```text
Exitoso = 0
Mensaje = Ya existe un registro de inventario para el producto y negocio seleccionados.
```

---

# 10. Consideraciones para Backend

Desde el Backend, estos procedures pueden ser consumidos desde la capa de repositorio usando Dapper.

## 10.1. Uso esperado

```text
Repository -> Ejecuta Procedure
Application Service -> Aplica reglas adicionales si se requieren
Controller -> Expone endpoint REST
```

## 10.2. Endpoints sugeridos

| Método HTTP | Endpoint sugerido      | Procedure                |
| ----------- | ---------------------- | ------------------------ |
| `GET`       | `/api/inventario`      | `UspListaInventario`     |
| `GET`       | `/api/inventario/{id}` | `UspConsultaInventario`  |
| `POST`      | `/api/inventario`      | `UspCreaInventario`      |
| `PATCH`     | `/api/inventario/{id}` | `UspActualizaInventario` |
| `DELETE`    | `/api/inventario/{id}` | `UspEliminaInventario`   |

> Si el Backend decide trabajar con actualización completa mediante `PUT`, debe enviar todos los campos. Sin embargo, este procedure está diseñado para actualización parcial, por eso encaja mejor con `PATCH`.

---

# 11. Notas técnicas

* Ejecutar cada `CREATE OR ALTER PROCEDURE` por separado en DBeaver si la herramienta presenta problemas con separadores de lote.
* Evitar el uso de `GO` en DBeaver si genera errores de sintaxis.
* Mantener los nombres de procedures bajo la convención definida.
* Mantener el borrado lógico como estándar del sistema.
* No consumir directamente la tabla `TBL_INVENTARIO` desde el Backend si ya existen procedures definidos para su operación.
* Crear primero negocios, productos y usuarios antes de crear inventario.
* `TBL_INVENTARIO` debe tener una restricción única por negocio y producto.
* Se recomienda que los movimientos reales de inventario se realicen posteriormente mediante procedures transaccionales relacionados con compras, ventas, ajustes y `TBL_KARDEX`.
