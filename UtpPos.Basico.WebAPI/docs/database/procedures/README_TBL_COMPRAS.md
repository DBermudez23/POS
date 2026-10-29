# README - Procedures de Compras

## 1. Objetivo

Este documento describe los tipos de tabla y procedimientos almacenados creados para la gestión del módulo de compras dentro de la base de datos del Sistema de Ventas.

El módulo de compras permite registrar compras realizadas a proveedores, almacenar el encabezado de la compra, sus productos asociados, sus pagos y generar automáticamente los movimientos de inventario mediante Kardex.

A diferencia de entidades simples como productos, usuarios o terceros, una compra no se registra como una sola fila. Una compra está compuesta por:

```text
Compra
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
TBL_COMPRAS
```

### Tablas dependientes

```sql
TBL_COMPRA_DETALLE
TBL_COMPRA_PAGO
TBL_INVENTARIO
TBL_KARDEX
```

---

## 3. Estructura general del módulo

### 3.1. Encabezado de compra

La tabla `TBL_COMPRAS` almacena la información principal de la compra.

```sql
COMPRA_ID       INT IDENTITY(1,1) PRIMARY KEY
COMPRA_NEG_ID   INT NOT NULL
COMPRA_TER_ID   INT NOT NULL
COMPRA_USR_ID   INT NOT NULL
COMPRA_ESTADO   CHAR(1) NOT NULL
COMPRA_FECHA    DATETIME NOT NULL
```

---

### 3.2. Detalle de compra

La tabla `TBL_COMPRA_DETALLE` almacena los productos incluidos en la compra.

```sql
COMDET_ID              INT IDENTITY(1,1) PRIMARY KEY
COMDET_COMPRA_ID       INT NOT NULL
COMDET_PROD_ID         INT NOT NULL
COMDET_CANTIDAD        DECIMAL(18,4) NOT NULL
COMDET_VALOR_UNIDAD    DECIMAL(18,2) NOT NULL
COMDET_SUBTOTAL        DECIMAL(18,2) NOT NULL
COMDET_VALOR_IVA       DECIMAL(18,2) NOT NULL
COMDET_TOTAL           DECIMAL(18,2) NOT NULL
COMDET_PORCENTAJE_IVA  DECIMAL(5,2) NOT NULL
COMDET_ESTADO          CHAR(1) NOT NULL
```

---

### 3.3. Pagos de compra

La tabla `TBL_COMPRA_PAGO` almacena los pagos asociados a una compra.

```sql
COMPA_ID             INT IDENTITY(1,1) PRIMARY KEY
COMPA_COMPRA_ID      INT NOT NULL
COMPA_MEDIOPAGO_ID   INT NOT NULL
COMPA_VALOR_RECIBE   DECIMAL(18,2) NOT NULL
COMPA_VALOR_CAMBIO   DECIMAL(18,2) NOT NULL
COMPA_ESTADO         CHAR(1) NOT NULL
```

---

## 4. Relaciones principales

```sql
TBL_COMPRAS.COMPRA_NEG_ID -> TBL_NEGOCIO.NEG_ID
TBL_COMPRAS.COMPRA_TER_ID -> TBL_TERCEROS.TER_ID
TBL_COMPRAS.COMPRA_USR_ID -> TBL_USUARIOS.USR_ID

TBL_COMPRA_DETALLE.COMDET_COMPRA_ID -> TBL_COMPRAS.COMPRA_ID
TBL_COMPRA_DETALLE.COMDET_PROD_ID -> TBL_PRODUCTOS.PROD_ID

TBL_COMPRA_PAGO.COMPA_COMPRA_ID -> TBL_COMPRAS.COMPRA_ID
TBL_COMPRA_PAGO.COMPA_MEDIOPAGO_ID -> TBL_PARAMETROS.PARAM_ID
```

Además, el módulo de compras interactúa directamente con:

```sql
TBL_INVENTARIO
TBL_KARDEX
```

Cuando se registra una compra, se generan entradas de inventario mediante Kardex.

---

# 5. Types creados

Para registrar compras con múltiples productos y múltiples pagos, se crearon dos tipos de tabla personalizados.

---

## 5.1. CompraDetalleType

### Objetivo

Permite enviar al procedimiento de registro de compra varios productos en una sola llamada.

### Definición

```sql
CREATE TYPE dbo.CompraDetalleType AS TABLE
(
    COMDET_PROD_ID INT NOT NULL,
    COMDET_CANTIDAD DECIMAL(18,4) NOT NULL,
    COMDET_VALOR_UNIDAD DECIMAL(18,2) NOT NULL,
    COMDET_PORCENTAJE_IVA DECIMAL(5,2) NOT NULL
);
```

### Campos

| Campo                   | Descripción                             |
| ----------------------- | --------------------------------------- |
| `COMDET_PROD_ID`        | Producto comprado.                      |
| `COMDET_CANTIDAD`       | Cantidad comprada.                      |
| `COMDET_VALOR_UNIDAD`   | Valor unitario de compra.               |
| `COMDET_PORCENTAJE_IVA` | Porcentaje de IVA aplicado al producto. |

---

## 5.2. CompraPagoType

### Objetivo

Permite enviar al procedimiento de registro de compra uno o varios pagos asociados a la compra.

### Definición

```sql
CREATE TYPE dbo.CompraPagoType AS TABLE
(
    COMPA_MEDIOPAGO_ID INT NOT NULL,
    COMPA_VALOR_RECIBE DECIMAL(18,2) NOT NULL,
    COMPA_VALOR_CAMBIO DECIMAL(18,2) NOT NULL
);
```

### Campos

| Campo                | Descripción                          |
| -------------------- | ------------------------------------ |
| `COMPA_MEDIOPAGO_ID` | Medio de pago parametrizado.         |
| `COMPA_VALOR_RECIBE` | Valor recibido en ese medio de pago. |
| `COMPA_VALOR_CAMBIO` | Valor devuelto como cambio.          |

---

# 6. Procedures creados

Para el módulo de compras se crearon los siguientes procedimientos almacenados:

```text
UspListaCompras
UspConsultaCompra
UspRegistraCompra
UspAnulaCompra
```

Este módulo no maneja un CRUD clásico completo, porque una compra afecta inventario, Kardex, detalle y pagos.

---

# 7. Consideraciones generales

* Una compra se registra de forma transaccional.
* Una compra debe tener al menos un producto en el detalle.
* Una compra puede tener uno o varios pagos.
* El encabezado de la compra se guarda en `TBL_COMPRAS`.
* Los productos se guardan en `TBL_COMPRA_DETALLE`.
* Los pagos se guardan en `TBL_COMPRA_PAGO`.
* Cada producto comprado genera una entrada en Kardex.
* Cada entrada en Kardex actualiza el inventario.
* Si no existe ficha de inventario para un producto comprado, se crea automáticamente.
* No se permite comprar productos inactivos.
* No se permite registrar compras para negocios, terceros o usuarios inactivos.
* No se eliminan compras físicamente.
* La anulación de una compra se maneja mediante estado `I`.
* La anulación genera movimientos de salida en Kardex para reversar el inventario.
* Los procedures de escritura usan transacciones.
* Si ocurre un error durante el registro, se revierte toda la operación.

---

# 8. Detalle de procedures

---

## 8.1. UspListaCompras

### Objetivo

Listar las compras registradas en el sistema, permitiendo aplicar filtros por negocio, tercero, usuario, estado y rango de fechas.

### Nombre

```sql
dbo.UspListaCompras
```

### Parámetros de entrada

| Parámetro        |       Tipo | Obligatorio | Descripción                           |
| ---------------- | ---------: | ----------: | ------------------------------------- |
| `@COMPRA_NEG_ID` |      `INT` |          No | Filtra compras por negocio.           |
| `@COMPRA_TER_ID` |      `INT` |          No | Filtra compras por tercero/proveedor. |
| `@COMPRA_USR_ID` |      `INT` |          No | Filtra compras por usuario.           |
| `@COMPRA_ESTADO` |  `CHAR(1)` |          No | Filtra por estado: `A` o `I`.         |
| `@FECHA_INICIAL` | `DATETIME` |          No | Fecha inicial del rango de búsqueda.  |
| `@FECHA_FINAL`   | `DATETIME` |          No | Fecha final del rango de búsqueda.    |

### Resultado

Retorna un listado resumido de compras, incluyendo totales calculados desde el detalle y pagos.

### Campos retornados

| Campo                  | Descripción                           |
| ---------------------- | ------------------------------------- |
| `COMPRA_ID`            | Identificador de la compra.           |
| `COMPRA_NEG_ID`        | Identificador del negocio.            |
| `NEG_NOMBRE`           | Nombre del negocio.                   |
| `COMPRA_TER_ID`        | Identificador del tercero/proveedor.  |
| `TER_NOMBRE`           | Nombre del tercero/proveedor.         |
| `TER_NUMERO_DOCUMENTO` | Documento del tercero/proveedor.      |
| `COMPRA_USR_ID`        | Usuario que registró la compra.       |
| `USUARIO_COMPRA`       | Nombre del usuario.                   |
| `COMPRA_ESTADO`        | Estado de la compra.                  |
| `COMPRA_FECHA`         | Fecha de la compra.                   |
| `COMPRA_SUBTOTAL`      | Subtotal calculado desde el detalle.  |
| `COMPRA_VALOR_IVA`     | IVA total calculado desde el detalle. |
| `COMPRA_TOTAL`         | Total de la compra.                   |
| `COMPRA_TOTAL_PAGADO`  | Total pagado según pagos activos.     |

### Ejemplos de uso

Listar todas las compras:

```sql
EXEC dbo.UspListaCompras;
```

Listar compras activas:

```sql
EXEC dbo.UspListaCompras
    @COMPRA_ESTADO = 'A';
```

Listar compras por negocio:

```sql
EXEC dbo.UspListaCompras
    @COMPRA_NEG_ID = 1;
```

Listar compras por rango de fechas:

```sql
EXEC dbo.UspListaCompras
    @FECHA_INICIAL = '2026-06-01',
    @FECHA_FINAL = '2026-06-30';
```

---

## 8.2. UspConsultaCompra

### Objetivo

Consultar una compra específica con su encabezado, detalle de productos y pagos asociados.

### Nombre

```sql
dbo.UspConsultaCompra
```

### Parámetros de entrada

| Parámetro    |  Tipo | Obligatorio | Descripción                             |
| ------------ | ----: | ----------: | --------------------------------------- |
| `@COMPRA_ID` | `INT` |          Sí | Identificador de la compra a consultar. |

### Resultado

Este procedure retorna tres conjuntos de resultados:

```text
1. Encabezado de la compra.
2. Detalle de productos.
3. Pagos asociados.
```

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaCompra
    @COMPRA_ID = 1;
```

---

## 8.3. UspRegistraCompra

### Objetivo

Registrar una compra completa en una sola operación transaccional.

Este procedure crea el encabezado de la compra, registra sus productos, registra sus pagos, crea inventario si no existe, registra movimientos de entrada en Kardex y actualiza el inventario.

### Nombre

```sql
dbo.UspRegistraCompra
```

### Parámetros de entrada

| Parámetro                  |                             Tipo | Obligatorio | Descripción                                           |
| -------------------------- | -------------------------------: | ----------: | ----------------------------------------------------- |
| `@COMPRA_NEG_ID`           |                            `INT` |          Sí | Negocio donde se registra la compra.                  |
| `@COMPRA_TER_ID`           |                            `INT` |          Sí | Tercero/proveedor asociado a la compra.               |
| `@COMPRA_USR_ID`           |                            `INT` |          Sí | Usuario que registra la compra.                       |
| `@KRDX_TIPO_MOV_ID_COMPRA` |                            `INT` |          Sí | Tipo de movimiento de Kardex para entrada por compra. |
| `@DETALLES`                | `dbo.CompraDetalleType READONLY` |          Sí | Tabla con los productos comprados.                    |
| `@PAGOS`                   |    `dbo.CompraPagoType READONLY` |          Sí | Tabla con los pagos de la compra.                     |

### Validaciones realizadas

El procedure valida que:

1. El negocio sea obligatorio.
2. El tercero/proveedor sea obligatorio.
3. El usuario sea obligatorio.
4. El tipo de movimiento de Kardex sea obligatorio.
5. La compra tenga al menos un producto.
6. El negocio exista y esté activo.
7. El tercero/proveedor exista y esté activo.
8. El usuario exista y esté activo.
9. El tipo de movimiento de Kardex exista y esté activo.
10. Los productos del detalle sean válidos.
11. Las cantidades sean mayores que cero.
12. Los valores unitarios no sean negativos.
13. Los porcentajes de IVA no sean negativos.
14. Los productos existan y estén activos.
15. Los pagos tengan valores válidos.
16. Los medios de pago existan y estén activos.
17. El valor del cambio no sea mayor al valor recibido.
18. El inventario exista o pueda ser creado automáticamente.

### Operaciones realizadas

El procedure realiza las siguientes acciones:

```text
1. Crea el encabezado en TBL_COMPRAS.
2. Inserta los productos en TBL_COMPRA_DETALLE.
3. Inserta los pagos en TBL_COMPRA_PAGO.
4. Crea ficha de inventario si no existe.
5. Recorre los productos comprados.
6. Por cada producto, registra una entrada en Kardex.
7. Kardex actualiza el stock y costos de inventario.
8. Calcula subtotal, IVA, total y total pagado.
9. Retorna la respuesta estándar.
```

### Resultado

| Campo                 | Descripción                                               |
| --------------------- | --------------------------------------------------------- |
| `Exitoso`             | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje`             | Mensaje descriptivo del resultado.                        |
| `COMPRA_ID`           | Identificador de la compra creada.                        |
| `COMPRA_SUBTOTAL`     | Subtotal de la compra.                                    |
| `COMPRA_VALOR_IVA`    | Valor total de IVA.                                       |
| `COMPRA_TOTAL`        | Total de la compra.                                       |
| `COMPRA_TOTAL_PAGADO` | Total pagado.                                             |

---

## 8.4. UspAnulaCompra

### Objetivo

Anular una compra previamente registrada.

La anulación no elimina físicamente la compra. Cambia el estado de la compra, sus detalles y sus pagos a `I`, y genera movimientos de salida en Kardex para reversar el inventario ingresado por la compra.

### Nombre

```sql
dbo.UspAnulaCompra
```

### Parámetros de entrada

| Parámetro                     |  Tipo | Obligatorio | Descripción                                           |
| ----------------------------- | ----: | ----------: | ----------------------------------------------------- |
| `@COMPRA_ID`                  | `INT` |          Sí | Identificador de la compra a anular.                  |
| `@COMPRA_USR_ID`              | `INT` |          Sí | Usuario que realiza la anulación.                     |
| `@KRDX_TIPO_MOV_ID_ANULACION` | `INT` |          Sí | Tipo de movimiento de Kardex para reversar la compra. |

### Validaciones realizadas

El procedure valida que:

1. La compra sea obligatoria.
2. El usuario que anula sea obligatorio.
3. El tipo de movimiento de Kardex para anulación sea obligatorio.
4. El usuario exista y esté activo.
5. El tipo de movimiento exista y esté activo.
6. La compra exista y esté activa.
7. Exista stock suficiente para reversar la compra.

### Operaciones realizadas

```text
1. Consulta la compra activa.
2. Recorre el detalle activo de la compra.
3. Por cada producto, registra una salida en Kardex.
4. Kardex descuenta el inventario.
5. Cambia COMPRA_ESTADO a I.
6. Cambia COMDET_ESTADO a I.
7. Cambia COMPA_ESTADO a I.
8. Retorna la respuesta estándar.
```

### Resultado

| Campo       | Descripción                                               |
| ----------- | --------------------------------------------------------- |
| `Exitoso`   | `1` si la operación fue exitosa, `0` si ocurrió un error. |
| `Mensaje`   | Mensaje descriptivo del resultado.                        |
| `COMPRA_ID` | Identificador de la compra anulada.                       |

---

# 9. Uso de los Types

## 9.1. Declarar detalle

```sql
DECLARE @Detalles dbo.CompraDetalleType;
```

## 9.2. Agregar productos al detalle

```sql
INSERT INTO @Detalles (
    COMDET_PROD_ID,
    COMDET_CANTIDAD,
    COMDET_VALOR_UNIDAD,
    COMDET_PORCENTAJE_IVA
)
VALUES
    (1, 10, 25000, 19),
    (2, 5, 12000, 19);
```

---

## 9.3. Declarar pagos

```sql
DECLARE @Pagos dbo.CompraPagoType;
```

## 9.4. Agregar pagos

```sql
INSERT INTO @Pagos (
    COMPA_MEDIOPAGO_ID,
    COMPA_VALOR_RECIBE,
    COMPA_VALOR_CAMBIO
)
VALUES
    (1, 357500, 0);
```

---

## 9.5. Registrar compra

```sql
EXEC dbo.UspRegistraCompra
    @COMPRA_NEG_ID = 1,
    @COMPRA_TER_ID = 1,
    @COMPRA_USR_ID = 1,
    @KRDX_TIPO_MOV_ID_COMPRA = 1,
    @DETALLES = @Detalles,
    @PAGOS = @Pagos;
```

---

# 10. Flujo recomendado de prueba

## 10.1. Crear variables de tabla

```sql
DECLARE @Detalles dbo.CompraDetalleType;
DECLARE @Pagos dbo.CompraPagoType;
```

---

## 10.2. Insertar detalle de compra

```sql
INSERT INTO @Detalles (
    COMDET_PROD_ID,
    COMDET_CANTIDAD,
    COMDET_VALOR_UNIDAD,
    COMDET_PORCENTAJE_IVA
)
VALUES
    (1, 10, 25000, 19);
```

---

## 10.3. Insertar pago

```sql
INSERT INTO @Pagos (
    COMPA_MEDIOPAGO_ID,
    COMPA_VALOR_RECIBE,
    COMPA_VALOR_CAMBIO
)
VALUES
    (1, 297500, 0);
```

---

## 10.4. Registrar compra

```sql
EXEC dbo.UspRegistraCompra
    @COMPRA_NEG_ID = 1,
    @COMPRA_TER_ID = 1,
    @COMPRA_USR_ID = 1,
    @KRDX_TIPO_MOV_ID_COMPRA = 1,
    @DETALLES = @Detalles,
    @PAGOS = @Pagos;
```

---

## 10.5. Consultar compra

```sql
EXEC dbo.UspConsultaCompra
    @COMPRA_ID = 1;
```

---

## 10.6. Listar Kardex del producto comprado

```sql
EXEC dbo.UspListaKardex
    @KRDX_NEGOCIO_ID = 1,
    @KRDX_PROD_ID = 1;
```

---

## 10.7. Consultar inventario actualizado

```sql
EXEC dbo.UspListaInventario
    @INV_NEGOCIO_ID = 1,
    @INV_PROD_ID = 1;
```

---

## 10.8. Anular compra

```sql
EXEC dbo.UspAnulaCompra
    @COMPRA_ID = 1,
    @COMPRA_USR_ID = 1,
    @KRDX_TIPO_MOV_ID_ANULACION = 2;
```

---

# 11. Reglas de negocio aplicadas

## 11.1. Compra como operación transaccional

Una compra debe registrarse completa o no registrarse.

Esto significa que no debe quedar:

```text
Compra sin detalle.
Detalle sin pagos.
Compra sin Kardex.
Kardex sin inventario actualizado.
Inventario actualizado sin Kardex.
```

Por eso `UspRegistraCompra` usa transacciones.

---

## 11.2. Detalle obligatorio

Una compra debe tener al menos un producto.

```text
Compra válida = encabezado + al menos un producto
```

---

## 11.3. Pagos asociados

Los pagos se registran en `TBL_COMPRA_PAGO`.

El valor efectivo aplicado del pago se calcula como:

```text
Valor aplicado = COMPA_VALOR_RECIBE - COMPA_VALOR_CAMBIO
```

---

## 11.4. Inventario automático

Si un producto comprado no tiene ficha de inventario en el negocio, el procedure crea una ficha inicial con valores en cero.

Luego Kardex registra la entrada y actualiza el stock.

---

## 11.5. Kardex como fuente histórica

Cada producto comprado genera un movimiento de entrada en Kardex.

Esto permite conservar trazabilidad del inventario.

---

## 11.6. Anulación mediante reverso

La anulación de una compra no borra registros.

La anulación genera movimientos de salida en Kardex para reversar las cantidades que habían ingresado por la compra.

---

## 11.7. Estados

Los estados usados son:

```text
A = Activo
I = Inactivo / Anulado
```

La anulación cambia el estado de:

```text
TBL_COMPRAS
TBL_COMPRA_DETALLE
TBL_COMPRA_PAGO
```

---

# 12. Manejo de errores

Los procedures de escritura usan `TRY/CATCH`.

Si ocurre un error:

```text
1. Se revierte la transacción.
2. No queda la compra parcial.
3. No queda detalle parcial.
4. No quedan pagos parciales.
5. No queda inventario descuadrado.
6. Se retorna Exitoso = 0.
```

Ejemplo de error:

```text
Exitoso = 0
Mensaje = Uno o más productos del detalle no existen o se encuentran inactivos.
```

---

# 13. Consideración importante sobre Kardex

`UspRegistraCompra` depende de `UspRegistraMovimientoKardex`.

Por eso, `UspRegistraMovimientoKardex` debe comportarse correctamente cuando sea llamado desde otro procedure transaccional.

La recomendación técnica es que, si `UspRegistraMovimientoKardex` falla, debe propagar el error usando `THROW`, para que `UspRegistraCompra` también falle y haga `ROLLBACK`.

Esto evita que una compra se registre parcialmente si Kardex no pudo registrar el movimiento.

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

| Método HTTP | Endpoint sugerido          | Procedure           |
| ----------- | -------------------------- | ------------------- |
| `GET`       | `/api/compras`             | `UspListaCompras`   |
| `GET`       | `/api/compras/{id}`        | `UspConsultaCompra` |
| `POST`      | `/api/compras`             | `UspRegistraCompra` |
| `POST`      | `/api/compras/{id}/anular` | `UspAnulaCompra`    |

No se recomienda usar `PUT`, `PATCH` o `DELETE` directo para compras, porque una compra afecta Kardex e inventario.

---

## 14.3. Ejemplo conceptual desde Backend

Desde .NET/Dapper se debe construir un `DataTable` para enviar los detalles:

```csharp
var detalles = new DataTable();
detalles.Columns.Add("COMDET_PROD_ID", typeof(int));
detalles.Columns.Add("COMDET_CANTIDAD", typeof(decimal));
detalles.Columns.Add("COMDET_VALOR_UNIDAD", typeof(decimal));
detalles.Columns.Add("COMDET_PORCENTAJE_IVA", typeof(decimal));

detalles.Rows.Add(1, 10, 25000, 19);
```

Y otro `DataTable` para pagos:

```csharp
var pagos = new DataTable();
pagos.Columns.Add("COMPA_MEDIOPAGO_ID", typeof(int));
pagos.Columns.Add("COMPA_VALOR_RECIBE", typeof(decimal));
pagos.Columns.Add("COMPA_VALOR_CAMBIO", typeof(decimal));

pagos.Rows.Add(1, 297500, 0);
```

Luego se envían al procedure como parámetros tipo tabla.

```csharp
parameters.Add(
    "@DETALLES",
    detalles.AsTableValuedParameter("dbo.CompraDetalleType")
);

parameters.Add(
    "@PAGOS",
    pagos.AsTableValuedParameter("dbo.CompraPagoType")
);
```

---

# 15. Notas técnicas

* Ejecutar primero la creación de los `TYPE`.
* Los `TYPE` no se pueden alterar directamente con `ALTER TYPE`.
* Si se requiere cambiar un `TYPE`, normalmente se debe eliminar y crear de nuevo.
* No se puede eliminar un `TYPE` si algún procedure lo está usando.
* Ejecutar luego los procedures del módulo de compras.
* Crear primero negocios, terceros, usuarios, productos, medios de pago y tipos de movimiento Kardex.
* Confirmar que `UspRegistraMovimientoKardex` esté corregido para propagar errores cuando sea llamado desde compras.
* No consumir directamente las tablas de compras desde Backend si ya existen procedures definidos.
* No modificar compras directamente si ya generaron Kardex; usar anulación o procesos compensatorios.
