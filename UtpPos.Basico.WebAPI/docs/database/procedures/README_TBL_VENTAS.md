# README - Procedures de Ventas

## 1. Objetivo

Este documento describe los tipos de tabla y procedimientos almacenados creados para la gestión del módulo de ventas dentro de la base de datos del Sistema de Ventas.

El módulo de ventas permite registrar ventas realizadas a clientes, almacenar el encabezado de la venta, sus productos asociados, sus pagos y generar automáticamente los movimientos de salida de inventario mediante Kardex.

Una venta no se registra como una sola fila. Una venta está compuesta por:

```text
Venta
 ├── Encabezado
 ├── Detalle de productos
 ├── Pagos
 └── Movimientos de Kardex
```

Por esta razón, este módulo utiliza `User-Defined Table Types` para enviar múltiples productos y múltiples pagos a un único procedimiento almacenado.

---

## 2. Tablas relacionadas

### Tabla principal

```sql
TBL_VENTAS
```

### Tablas dependientes

```sql
TBL_VENTA_DETALLE
TBL_VENTA_PAGO
TBL_INVENTARIO
TBL_KARDEX
```

---

## 3. Estructura general del módulo

### 3.1. Encabezado de venta

La tabla `TBL_VENTAS` almacena la información principal de la venta.

```sql
VENTA_ID       INT IDENTITY(1,1) PRIMARY KEY
VENTA_NEG_ID   INT NOT NULL
VENTA_TER_ID   INT NOT NULL
VENTA_USR_ID   INT NOT NULL
VENTA_ESTADO   CHAR(1) NOT NULL
VENTA_FECHA    DATETIME NOT NULL
```

---

### 3.2. Detalle de venta

La tabla `TBL_VENTA_DETALLE` almacena los productos incluidos en la venta.

```sql
VENTDE_ID              INT IDENTITY(1,1) PRIMARY KEY
VENTDE_VENTA_ID        INT NOT NULL
VENTDE_PROD_ID         INT NOT NULL
VENDET_CANTIDAD        DECIMAL(18,4) NOT NULL
VENDET_VALOR_UNIDAD    DECIMAL(18,2) NOT NULL
VENDET_SUBTOTAL        DECIMAL(18,2) NOT NULL
VENDET_VALOR_IVA       DECIMAL(18,2) NOT NULL
VENDET_TOTAL           DECIMAL(18,2) NOT NULL
VENDET_PORCENTAJE_IVA  DECIMAL(5,2) NOT NULL
VENDET_COSTO_UNIDAD    DECIMAL(18,2) NOT NULL
VENDET_COSTO_TOTAL     DECIMAL(18,2) NOT NULL
VENDET_ESTADO          CHAR(1) NOT NULL
```

---

### 3.3. Pagos de venta

La tabla `TBL_VENTA_PAGO` almacena los pagos asociados a una venta.

```sql
VENPA_ID             INT IDENTITY(1,1) PRIMARY KEY
VENPA_VENTA_ID       INT NOT NULL
VENPA_MEDIOPAGO_ID   INT NOT NULL
VENPA_VALOR_RECIBE   DECIMAL(18,2) NOT NULL
VENPA_VALOR_CAMBIO   DECIMAL(18,2) NOT NULL
VENPA_ESTADO         CHAR(1) NOT NULL
```

---

## 4. Relaciones principales

```sql
TBL_VENTAS.VENTA_NEG_ID -> TBL_NEGOCIO.NEG_ID
TBL_VENTAS.VENTA_TER_ID -> TBL_TERCEROS.TER_ID
TBL_VENTAS.VENTA_USR_ID -> TBL_USUARIOS.USR_ID

TBL_VENTA_DETALLE.VENTDE_VENTA_ID -> TBL_VENTAS.VENTA_ID
TBL_VENTA_DETALLE.VENTDE_PROD_ID -> TBL_PRODUCTOS.PROD_ID

TBL_VENTA_PAGO.VENPA_VENTA_ID -> TBL_VENTAS.VENTA_ID
TBL_VENTA_PAGO.VENPA_MEDIOPAGO_ID -> TBL_PARAMETROS.PARAM_ID
```

Además, el módulo de ventas interactúa directamente con:

```sql
TBL_INVENTARIO
TBL_KARDEX
```

Cuando se registra una venta, se generan salidas de inventario mediante Kardex.

---

# 5. Types creados

Para registrar ventas con múltiples productos y múltiples pagos, se crearon dos tipos de tabla personalizados.

---

## 5.1. VentaDetalleType

### Objetivo

Permite enviar al procedimiento de registro de venta varios productos en una sola llamada.

### Definición

```sql
CREATE TYPE dbo.VentaDetalleType AS TABLE
(
    VENTDE_PROD_ID INT NOT NULL,
    VENDET_CANTIDAD DECIMAL(18,4) NOT NULL,
    VENDET_VALOR_UNIDAD DECIMAL(18,2) NOT NULL,
    VENDET_PORCENTAJE_IVA DECIMAL(5,2) NOT NULL
);
```

### Campos

| Campo                   | Descripción                             |
| ----------------------- | --------------------------------------- |
| `VENTDE_PROD_ID`        | Producto vendido.                       |
| `VENDET_CANTIDAD`       | Cantidad vendida.                       |
| `VENDET_VALOR_UNIDAD`   | Valor unitario de venta.                |
| `VENDET_PORCENTAJE_IVA` | Porcentaje de IVA aplicado al producto. |

---

## 5.2. VentaPagoType

### Objetivo

Permite enviar al procedimiento de registro de venta uno o varios pagos asociados a la venta.

### Definición

```sql
CREATE TYPE dbo.VentaPagoType AS TABLE
(
    VENPA_MEDIOPAGO_ID INT NOT NULL,
    VENPA_VALOR_RECIBE DECIMAL(18,2) NOT NULL,
    VENPA_VALOR_CAMBIO DECIMAL(18,2) NOT NULL
);
```

### Campos

| Campo                | Descripción                          |
| -------------------- | ------------------------------------ |
| `VENPA_MEDIOPAGO_ID` | Medio de pago parametrizado.         |
| `VENPA_VALOR_RECIBE` | Valor recibido en ese medio de pago. |
| `VENPA_VALOR_CAMBIO` | Valor devuelto como cambio.          |

---

# 6. Procedures creados

Para el módulo de ventas se crearon los siguientes procedimientos almacenados:

```text
UspListaVentas
UspConsultaVenta
UspRegistraVenta
UspAnulaVenta
```

Este módulo no maneja un CRUD clásico completo, porque una venta afecta detalle, pagos, Kardex e inventario.

---

# 7. Consideraciones generales

* Una venta se registra de forma transaccional.
* Una venta debe tener al menos un producto en el detalle.
* Una venta debe tener al menos un pago.
* El encabezado de la venta se guarda en `TBL_VENTAS`.
* Los productos se guardan en `TBL_VENTA_DETALLE`.
* Los pagos se guardan en `TBL_VENTA_PAGO`.
* Cada producto vendido genera una salida en Kardex.
* Cada salida en Kardex actualiza el inventario.
* No se permite vender productos inactivos.
* No se permite vender productos sin inventario activo.
* No se permite vender más unidades que el stock disponible.
* No se permite registrar ventas para negocios, terceros o usuarios inactivos.
* El total pagado debe coincidir con el total calculado de la venta.
* No se eliminan ventas físicamente.
* La anulación de una venta se maneja mediante estado `I`.
* La anulación genera movimientos de entrada en Kardex para reversar el inventario descontado por la venta.
* Los procedures de escritura usan transacciones.
* Si ocurre un error durante el registro, se revierte toda la operación.
* `UspRegistraVenta` depende de `UspRegistraMovimientoKardex`.
* `UspRegistraMovimientoKardex` debe estar corregido para propagar errores con `THROW` cuando sea llamado desde ventas.

---

# 8. Detalle de procedures

---

## 8.1. UspListaVentas

### Objetivo

Listar las ventas registradas en el sistema, permitiendo aplicar filtros por negocio, tercero, usuario, estado y rango de fechas.

### Nombre

```sql
dbo.UspListaVentas
```

### Parámetros de entrada

| Parámetro        |       Tipo | Obligatorio | Descripción                          |
| ---------------- | ---------: | ----------: | ------------------------------------ |
| `@VENTA_NEG_ID`  |      `INT` |          No | Filtra ventas por negocio.           |
| `@VENTA_TER_ID`  |      `INT` |          No | Filtra ventas por tercero/cliente.   |
| `@VENTA_USR_ID`  |      `INT` |          No | Filtra ventas por usuario.           |
| `@VENTA_ESTADO`  |  `CHAR(1)` |          No | Filtra por estado: `A` o `I`.        |
| `@FECHA_INICIAL` | `DATETIME` |          No | Fecha inicial del rango de búsqueda. |
| `@FECHA_FINAL`   | `DATETIME` |          No | Fecha final del rango de búsqueda.   |

### Resultado

Retorna un listado resumido de ventas, incluyendo totales calculados desde el detalle y pagos.

### Campos retornados

| Campo                  | Descripción                                    |
| ---------------------- | ---------------------------------------------- |
| `VENTA_ID`             | Identificador de la venta.                     |
| `VENTA_NEG_ID`         | Identificador del negocio.                     |
| `NEG_NOMBRE`           | Nombre del negocio.                            |
| `VENTA_TER_ID`         | Identificador del tercero/cliente.             |
| `TER_NOMBRE`           | Nombre del tercero/cliente.                    |
| `TER_NUMERO_DOCUMENTO` | Documento del tercero/cliente.                 |
| `VENTA_USR_ID`         | Usuario que registró la venta.                 |
| `USUARIO_VENTA`        | Nombre del usuario.                            |
| `VENTA_ESTADO`         | Estado de la venta.                            |
| `VENTA_FECHA`          | Fecha de la venta.                             |
| `VENTA_SUBTOTAL`       | Subtotal calculado desde el detalle.           |
| `VENTA_VALOR_IVA`      | IVA total calculado desde el detalle.          |
| `VENTA_TOTAL`          | Total de la venta.                             |
| `VENTA_COSTO_TOTAL`    | Costo total de los productos vendidos.         |
| `VENTA_UTILIDAD_BRUTA` | Diferencia entre total de venta y costo total. |
| `VENTA_TOTAL_PAGADO`   | Total pagado según pagos activos.              |

### Ejemplos de uso

Listar todas las ventas:

```sql
EXEC dbo.UspListaVentas;
```

Listar ventas activas:

```sql
EXEC dbo.UspListaVentas
    @VENTA_ESTADO = 'A';
```

Listar ventas por negocio:

```sql
EXEC dbo.UspListaVentas
    @VENTA_NEG_ID = 1;
```

Listar ventas por rango de fechas:

```sql
EXEC dbo.UspListaVentas
    @FECHA_INICIAL = '2026-06-01',
    @FECHA_FINAL = '2026-06-30';
```

---

## 8.2. UspConsultaVenta

### Objetivo

Consultar una venta específica con su encabezado, detalle de productos y pagos asociados.

### Nombre

```sql
dbo.UspConsultaVenta
```

### Parámetros de entrada

| Parámetro   |  Tipo | Obligatorio | Descripción                            |
| ----------- | ----: | ----------: | -------------------------------------- |
| `@VENTA_ID` | `INT` |          Sí | Identificador de la venta a consultar. |

### Resultado

Este procedure retorna tres conjuntos de resultados:

```text
1. Encabezado de la venta.
2. Detalle de productos.
3. Pagos asociados.
```

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaVenta
    @VENTA_ID = 1;
```

---

## 8.3. UspRegistraVenta

### Objetivo

Registrar una venta completa en una sola operación transaccional.

Este procedure crea el encabezado de la venta, registra sus productos, registra sus pagos, registra movimientos de salida en Kardex y actualiza el inventario.

### Nombre

```sql
dbo.UspRegistraVenta
```

### Parámetros de entrada

| Parámetro                 |                            Tipo | Obligatorio | Descripción                                         |
| ------------------------- | ------------------------------: | ----------: | --------------------------------------------------- |
| `@VENTA_NEG_ID`           |                           `INT` |          Sí | Negocio donde se registra la venta.                 |
| `@VENTA_TER_ID`           |                           `INT` |          Sí | Tercero/cliente asociado a la venta.                |
| `@VENTA_USR_ID`           |                           `INT` |          Sí | Usuario que registra la venta.                      |
| `@KRDX_TIPO_MOV_ID_VENTA` |                           `INT` |          Sí | Tipo de movimiento de Kardex para salida por venta. |
| `@DETALLES`               | `dbo.VentaDetalleType READONLY` |          Sí | Tabla con los productos vendidos.                   |
| `@PAGOS`                  |    `dbo.VentaPagoType READONLY` |          Sí | Tabla con los pagos de la venta.                    |

### Validaciones realizadas

El procedure valida que:

1. El negocio sea obligatorio.
2. El tercero/cliente sea obligatorio.
3. El usuario sea obligatorio.
4. El tipo de movimiento de Kardex sea obligatorio.
5. La venta tenga al menos un producto.
6. La venta tenga al menos un pago.
7. El negocio exista y esté activo.
8. El tercero/cliente exista y esté activo.
9. El usuario exista y esté activo.
10. El tipo de movimiento de Kardex exista y esté activo.
11. Los productos del detalle sean válidos.
12. Las cantidades sean mayores que cero.
13. Los valores unitarios no sean negativos.
14. Los porcentajes de IVA no sean negativos.
15. Los productos existan y estén activos.
16. Si un producto pertenece a un negocio específico, debe coincidir con el negocio de la venta.
17. Los productos tengan inventario activo para el negocio.
18. Exista stock suficiente para cada producto vendido.
19. Los pagos tengan valores válidos.
20. Los medios de pago existan y estén activos.
21. El valor del cambio no sea mayor al valor recibido.
22. El total pagado coincida con el total calculado de la venta.

### Operaciones realizadas

El procedure realiza las siguientes acciones:

```text
1. Calcula subtotal, IVA, total y total pagado.
2. Valida que el total pagado coincida con el total de la venta.
3. Crea el encabezado en TBL_VENTAS.
4. Inserta los productos en TBL_VENTA_DETALLE.
5. Guarda costo unitario y costo total de cada producto vendido.
6. Inserta los pagos en TBL_VENTA_PAGO.
7. Agrupa los productos vendidos.
8. Por cada producto, registra una salida en Kardex.
9. Kardex actualiza el stock del inventario.
10. Retorna la respuesta estándar.
```

### Resultado

| Campo                | Descripción                                               |
| -------------------- | --------------------------------------------------------- |
| `Exitoso`            | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje`            | Mensaje descriptivo del resultado.                        |
| `VENTA_ID`           | Identificador de la venta creada.                         |
| `VENTA_SUBTOTAL`     | Subtotal de la venta.                                     |
| `VENTA_VALOR_IVA`    | Valor total de IVA.                                       |
| `VENTA_TOTAL`        | Total de la venta.                                        |
| `VENTA_TOTAL_PAGADO` | Total pagado.                                             |

---

## 8.4. UspAnulaVenta

### Objetivo

Anular una venta previamente registrada.

La anulación no elimina físicamente la venta. Cambia el estado de la venta, sus detalles y sus pagos a `I`, y genera movimientos de entrada en Kardex para reversar el inventario descontado por la venta.

### Nombre

```sql
dbo.UspAnulaVenta
```

### Parámetros de entrada

| Parámetro                     |  Tipo | Obligatorio | Descripción                                          |
| ----------------------------- | ----: | ----------: | ---------------------------------------------------- |
| `@VENTA_ID`                   | `INT` |          Sí | Identificador de la venta a anular.                  |
| `@VENTA_USR_ID`               | `INT` |          Sí | Usuario que realiza la anulación.                    |
| `@KRDX_TIPO_MOV_ID_ANULACION` | `INT` |          Sí | Tipo de movimiento de Kardex para reversar la venta. |

### Validaciones realizadas

El procedure valida que:

1. La venta sea obligatoria.
2. El usuario que anula sea obligatorio.
3. El tipo de movimiento de Kardex para anulación sea obligatorio.
4. El usuario exista y esté activo.
5. El tipo de movimiento exista y esté activo.
6. La venta exista y esté activa.
7. La venta tenga detalle activo para anular.

### Operaciones realizadas

```text
1. Consulta la venta activa.
2. Recorre el detalle activo de la venta.
3. Agrupa los productos vendidos.
4. Por cada producto, registra una entrada en Kardex.
5. Kardex suma nuevamente el inventario.
6. Cambia VENTA_ESTADO a I.
7. Cambia VENDET_ESTADO a I.
8. Cambia VENPA_ESTADO a I.
9. Retorna la respuesta estándar.
```

### Resultado

| Campo      | Descripción                                               |
| ---------- | --------------------------------------------------------- |
| `Exitoso`  | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje`  | Mensaje descriptivo del resultado.                        |
| `VENTA_ID` | Identificador de la venta anulada.                        |

---

# 9. Uso de los Types

## 9.1. Declarar detalle

```sql
DECLARE @Detalles dbo.VentaDetalleType;
```

## 9.2. Agregar productos al detalle

```sql
INSERT INTO @Detalles (
    VENTDE_PROD_ID,
    VENDET_CANTIDAD,
    VENDET_VALOR_UNIDAD,
    VENDET_PORCENTAJE_IVA
)
VALUES
    (1, 2, 45000, 19),
    (2, 1, 80000, 19);
```

---

## 9.3. Declarar pagos

```sql
DECLARE @Pagos dbo.VentaPagoType;
```

## 9.4. Agregar pagos

```sql
INSERT INTO @Pagos (
    VENPA_MEDIOPAGO_ID,
    VENPA_VALOR_RECIBE,
    VENPA_VALOR_CAMBIO
)
VALUES
    (1, 197540, 0);
```

---

## 9.5. Registrar venta

```sql
EXEC dbo.UspRegistraVenta
    @VENTA_NEG_ID = 1,
    @VENTA_TER_ID = 1,
    @VENTA_USR_ID = 1,
    @KRDX_TIPO_MOV_ID_VENTA = 2,
    @DETALLES = @Detalles,
    @PAGOS = @Pagos;
```

---

# 10. Flujo recomendado de prueba

## 10.1. Crear variables de tabla

```sql
DECLARE @Detalles dbo.VentaDetalleType;
DECLARE @Pagos dbo.VentaPagoType;
```

---

## 10.2. Insertar detalle de venta

```sql
INSERT INTO @Detalles (
    VENTDE_PROD_ID,
    VENDET_CANTIDAD,
    VENDET_VALOR_UNIDAD,
    VENDET_PORCENTAJE_IVA
)
VALUES
    (1, 2, 45000, 19);
```

---

## 10.3. Insertar pago

```sql
INSERT INTO @Pagos (
    VENPA_MEDIOPAGO_ID,
    VENPA_VALOR_RECIBE,
    VENPA_VALOR_CAMBIO
)
VALUES
    (1, 107100, 0);
```

---

## 10.4. Registrar venta

```sql
EXEC dbo.UspRegistraVenta
    @VENTA_NEG_ID = 1,
    @VENTA_TER_ID = 1,
    @VENTA_USR_ID = 1,
    @KRDX_TIPO_MOV_ID_VENTA = 2,
    @DETALLES = @Detalles,
    @PAGOS = @Pagos;
```

---

## 10.5. Consultar venta

```sql
EXEC dbo.UspConsultaVenta
    @VENTA_ID = 1;
```

---

## 10.6. Listar ventas

```sql
EXEC dbo.UspListaVentas;
```

---

## 10.7. Consultar Kardex del producto vendido

```sql
EXEC dbo.UspListaKardex
    @KRDX_NEGOCIO_ID = 1,
    @KRDX_PROD_ID = 1;
```

---

## 10.8. Consultar inventario actualizado

```sql
EXEC dbo.UspListaInventario
    @INV_NEGOCIO_ID = 1,
    @INV_PROD_ID = 1;
```

---

## 10.9. Anular venta

```sql
EXEC dbo.UspAnulaVenta
    @VENTA_ID = 1,
    @VENTA_USR_ID = 1,
    @KRDX_TIPO_MOV_ID_ANULACION = 1;
```

---

# 11. Reglas de negocio aplicadas

## 11.1. Venta como operación transaccional

Una venta debe registrarse completa o no registrarse.

Esto significa que no debe quedar:

```text
Venta sin detalle.
Detalle sin pagos.
Venta sin Kardex.
Kardex sin inventario actualizado.
Inventario actualizado sin Kardex.
```

Por eso `UspRegistraVenta` usa transacciones.

---

## 11.2. Detalle obligatorio

Una venta debe tener al menos un producto.

```text
Venta válida = encabezado + al menos un producto
```

---

## 11.3. Pago obligatorio

Una venta debe tener al menos un pago.

El valor efectivo aplicado del pago se calcula como:

```text
Valor aplicado = VENPA_VALOR_RECIBE - VENPA_VALOR_CAMBIO
```

---

## 11.4. Validación de pago total

El total pagado debe coincidir con el total de la venta.

```text
Total venta = SUM(VENDET_TOTAL)
Total pagado = SUM(VENPA_VALOR_RECIBE - VENPA_VALOR_CAMBIO)
```

Si los valores no coinciden, la venta no se registra.

---

## 11.5. Inventario obligatorio

Para poder vender un producto, debe existir inventario activo para el producto dentro del negocio.

```text
INV_ESTADO = 'A'
```

---

## 11.6. Stock suficiente

No se permite registrar una venta si la cantidad solicitada supera el stock disponible.

```text
INV_STOCK_ACTUAL >= Cantidad vendida
```

Esta validación se realiza antes de insertar la venta y también queda reforzada por Kardex.

---

## 11.7. Kardex como fuente histórica

Cada producto vendido genera un movimiento de salida en Kardex.

Esto permite conservar trazabilidad del inventario.

---

## 11.8. Costo de venta

Al registrar el detalle de venta, se guarda el costo unitario y costo total del producto vendido.

El costo usado corresponde al costo ponderado actual del inventario en el momento de la venta.

```text
VENDET_COSTO_UNIDAD = INV_COSTO_PONDERADO
VENDET_COSTO_TOTAL = VENDET_CANTIDAD * INV_COSTO_PONDERADO
```

Esto permite calcular utilidad bruta posteriormente.

---

## 11.9. Utilidad bruta

La utilidad bruta se calcula como:

```text
Utilidad bruta = Total venta - Costo total
```

En el listado y consulta de ventas se retorna:

```text
VENTA_TOTAL
VENTA_COSTO_TOTAL
VENTA_UTILIDAD_BRUTA
```

---

## 11.10. Anulación mediante reverso

La anulación de una venta no borra registros.

La anulación genera movimientos de entrada en Kardex para reversar las cantidades descontadas por la venta.

---

## 11.11. Estados

Los estados usados son:

```text
A = Activo
I = Inactivo / Anulado
```

La anulación cambia el estado de:

```text
TBL_VENTAS
TBL_VENTA_DETALLE
TBL_VENTA_PAGO
```

---

# 12. Manejo de errores

Los procedures de escritura usan `TRY/CATCH`.

Si ocurre un error:

```text
1. Se revierte la transacción.
2. No queda la venta parcial.
3. No queda detalle parcial.
4. No quedan pagos parciales.
5. No queda inventario descuadrado.
6. Se retorna Exitoso = 0.
```

Ejemplo de error:

```text
Exitoso = 0
Mensaje = No hay stock suficiente para uno o más productos de la venta.
```

---

# 13. Consideración importante sobre Kardex

`UspRegistraVenta` depende de `UspRegistraMovimientoKardex`.

Por eso, `UspRegistraMovimientoKardex` debe comportarse correctamente cuando sea llamado desde otro procedure transaccional.

La versión recomendada de Kardex debe:

```text
1. Usar su propia transacción cuando se ejecuta directamente.
2. Usar SAVE TRANSACTION cuando es llamado desde otro procedure.
3. No hacer COMMIT propio cuando viene desde ventas.
4. Propagar errores con THROW cuando falla dentro de ventas.
```

Esto evita que una venta se registre parcialmente si Kardex no pudo registrar el movimiento.

---

# 14. Consideraciones para Backend

Desde el Backend, este módulo puede ser consumido desde la capa de repositorio usando Dapper.

## 14.1. Uso esperado

```text
Repository -> Ejecuta Procedure
Application Service -> Aplica reglas adicionales
Controller -> Expone endpoint REST
```

---

## 14.2. Endpoints sugeridos

| Método HTTP | Endpoint sugerido         | Procedure          |
| ----------- | ------------------------- | ------------------ |
| `GET`       | `/api/ventas`             | `UspListaVentas`   |
| `GET`       | `/api/ventas/{id}`        | `UspConsultaVenta` |
| `POST`      | `/api/ventas`             | `UspRegistraVenta` |
| `POST`      | `/api/ventas/{id}/anular` | `UspAnulaVenta`    |

No se recomienda usar `PUT`, `PATCH` o `DELETE` directo para ventas, porque una venta afecta Kardex e inventario.

---

## 14.3. Ejemplo conceptual desde Backend

Desde .NET/Dapper se debe construir un `DataTable` para enviar los detalles:

```csharp
var detalles = new DataTable();
detalles.Columns.Add("VENTDE_PROD_ID", typeof(int));
detalles.Columns.Add("VENDET_CANTIDAD", typeof(decimal));
detalles.Columns.Add("VENDET_VALOR_UNIDAD", typeof(decimal));
detalles.Columns.Add("VENDET_PORCENTAJE_IVA", typeof(decimal));

detalles.Rows.Add(1, 2, 45000, 19);
```

Y otro `DataTable` para pagos:

```csharp
var pagos = new DataTable();
pagos.Columns.Add("VENPA_MEDIOPAGO_ID", typeof(int));
pagos.Columns.Add("VENPA_VALOR_RECIBE", typeof(decimal));
pagos.Columns.Add("VENPA_VALOR_CAMBIO", typeof(decimal));

pagos.Rows.Add(1, 107100, 0);
```

Luego se envían al procedure como parámetros tipo tabla.

```csharp
parameters.Add(
    "@DETALLES",
    detalles.AsTableValuedParameter("dbo.VentaDetalleType")
);

parameters.Add(
    "@PAGOS",
    pagos.AsTableValuedParameter("dbo.VentaPagoType")
);
```

---

# 15. Notas técnicas

* Ejecutar primero la creación de los `TYPE`.
* Los `TYPE` no se pueden alterar directamente con `ALTER TYPE`.
* Si se requiere cambiar un `TYPE`, normalmente se debe eliminar y crear de nuevo.
* No se puede eliminar un `TYPE` si algún procedure lo está usando.
* Ejecutar luego los procedures del módulo de ventas.
* Crear primero negocios, terceros, usuarios, productos, inventario, medios de pago y tipos de movimiento Kardex.
* Antes de probar ventas, debe existir stock disponible en inventario.
* El stock normalmente llega al sistema mediante compras o entradas de Kardex.
* Confirmar que `UspRegistraMovimientoKardex` esté corregido para propagar errores cuando sea llamado desde ventas.
* No consumir directamente las tablas de ventas desde Backend si ya existen procedures definidos.
* No modificar ventas directamente si ya generaron Kardex; usar anulación o procesos compensatorios.
* `@KRDX_TIPO_MOV_ID_VENTA` debe representar un tipo de movimiento como `Salida por venta`.
* `@KRDX_TIPO_MOV_ID_ANULACION` debe representar un tipo de movimiento como `Entrada por anulación de venta` o `Devolución de venta`.
