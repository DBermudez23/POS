# README - Procedures de Kardex

## 1. Objetivo

Este documento describe los procedimientos almacenados creados para la gestión de movimientos de inventario mediante la tabla `TBL_KARDEX` dentro de la base de datos del Sistema de Ventas.

La tabla `TBL_KARDEX` representa el histórico de movimientos de inventario. Cada registro indica una entrada o salida de producto, el saldo anterior, el saldo actual, el costo unitario, el costo ponderado anterior, el costo ponderado actualizado y el valor total del movimiento.

A diferencia de otras entidades del sistema, Kardex no se maneja como un CRUD tradicional. Los movimientos de Kardex deben conservarse como histórico y trazabilidad operativa.

Los procedimientos almacenados definidos permiten listar, consultar y registrar movimientos de inventario.

---

## 2. Tabla relacionada

### Tabla principal

```sql
TBL_KARDEX
```

### Estructura base

```sql
KRDX_ID                    INT IDENTITY(1,1) PRIMARY KEY
KRDX_NEGOCIO_ID            INT NOT NULL
KRDX_INVENTARIO_ID         INT NOT NULL
KRDX_PROD_ID               INT NOT NULL
KRDX_CREADO_POR_USUARIO_ID INT NOT NULL
KRDX_TIPO_MOV_ID           INT NOT NULL
KRDX_CANT_ENTRADA          DECIMAL(18,4) NOT NULL
KRDX_CANT_SALIDA           DECIMAL(18,4) NOT NULL
KRDX_SALDO_ANTERIOR        DECIMAL(18,4) NOT NULL
KRDX_SALDO_ACTUAL          DECIMAL(18,4) NOT NULL
KRDX_COSTO_UNIDAD          DECIMAL(18,2) NOT NULL
KRDX_COSTO_PONDERADO_ANT   DECIMAL(18,2) NOT NULL
KRDX_COSTO_PONDERADO_ACT   DECIMAL(18,2) NOT NULL
KRDX_TOTAL                 DECIMAL(18,2) NOT NULL
KRDX_OBSERVACION           VARCHAR(150) NULL
KRDX_ESTADO                CHAR(1) NOT NULL
KRDX_FECHA_CREACION        DATETIME NOT NULL
```

---

## 3. Relaciones principales

La tabla `TBL_KARDEX` se relaciona con las siguientes tablas:

```sql
TBL_KARDEX.KRDX_NEGOCIO_ID -> TBL_NEGOCIO.NEG_ID
TBL_KARDEX.KRDX_INVENTARIO_ID -> TBL_INVENTARIO.INV_ID
TBL_KARDEX.KRDX_PROD_ID -> TBL_PRODUCTOS.PROD_ID
TBL_KARDEX.KRDX_CREADO_POR_USUARIO_ID -> TBL_USUARIOS.USR_ID
TBL_KARDEX.KRDX_TIPO_MOV_ID -> TBL_PARAMETROS.PARAM_ID
```

Estas relaciones permiten asociar cada movimiento de Kardex a un negocio, inventario, producto, usuario creador y tipo de movimiento parametrizado.

Como `TBL_PRODUCTOS.PROD_NEG_ID` es obligatorio, todo movimiento de Kardex debe operar sobre un producto que pertenezca al mismo negocio del movimiento.

---

## 4. Procedures creados

Para la entidad `Kardex`, se utilizan los siguientes procedimientos almacenados:

```text
UspListaKardex
UspConsultaKardex
UspRegistraMovimientoKardex
```

No se crea un procedure de actualización ni eliminación lógica estándar para Kardex, ya que el Kardex representa un histórico de movimientos. Si se requiere corregir una operación, se debe realizar mediante un nuevo movimiento compensatorio.

---

# 5. Consideraciones generales

* No se realiza eliminación física de registros de Kardex.

* No se actualizan movimientos históricos de Kardex.

* Cada movimiento registrado debe conservar su trazabilidad.

* Los movimientos pueden ser de entrada o salida.

* El tipo de operación se indica mediante el parámetro `@TIPO_OPERACION`.

* Los valores válidos de `@TIPO_OPERACION` son:

  * `E`: Entrada.
  * `S`: Salida.

* Para movimientos de entrada, `KRDX_CANT_ENTRADA` recibe la cantidad y `KRDX_CANT_SALIDA` queda en `0`.

* Para movimientos de salida, `KRDX_CANT_SALIDA` recibe la cantidad y `KRDX_CANT_ENTRADA` queda en `0`.

* La cantidad del movimiento debe ser mayor que cero.

* Las salidas no pueden dejar el inventario con stock negativo.

* Los movimientos de entrada requieren costo unitario.

* Los movimientos de salida usan el costo ponderado actual si no se envía costo unitario.

* Cada movimiento de Kardex actualiza la tabla `TBL_INVENTARIO`.

* Las entradas recalculan el costo ponderado del inventario.

* Las salidas conservan el costo ponderado vigente.

* Las entradas recalculan el valor base de venta del producto (`PROD_VALOR_VENTA`).

* Las salidas no recalculan el valor base de venta del producto.

* `PROD_VALOR_VENTA` no incluye IVA.

* El IVA se calcula posteriormente en el proceso de venta.

* Los procedures de escritura usan `TRY/CATCH`.

* El registro de Kardex, la actualización del inventario y el recálculo del valor base de venta ocurren dentro de la misma transacción.

* `UspRegistraMovimientoKardex` puede ejecutarse de forma independiente o ser llamado desde otros procedures transaccionales como compras, ventas, devoluciones o ajustes.

* Cuando `UspRegistraMovimientoKardex` se ejecuta de forma independiente, maneja su propia transacción.

* Cuando `UspRegistraMovimientoKardex` es llamado desde otro procedure que ya abrió una transacción, no hace `COMMIT` propio y usa `SAVE TRANSACTION`.

* Si ocurre un error al ser llamado desde otro procedure, el error se propaga con `THROW` para que el procedure padre pueda hacer `ROLLBACK` completo.

---

# 6. Detalle de procedures

---

## 6.1. UspListaKardex

### Objetivo

Listar los movimientos de Kardex registrados en el sistema, permitiendo aplicar filtros opcionales por negocio, inventario, producto, tipo de movimiento, estado y rango de fechas.

### Nombre

```sql
dbo.UspListaKardex
```

### Parámetros de entrada

| Parámetro             | Tipo       | Obligatorio | Descripción                                             |
| --------------------- | ---------- | ----------- | ------------------------------------------------------- |
| `@KRDX_NEGOCIO_ID`    | `INT`      | No          | Permite filtrar movimientos por negocio.                |
| `@KRDX_INVENTARIO_ID` | `INT`      | No          | Permite filtrar movimientos por registro de inventario. |
| `@KRDX_PROD_ID`       | `INT`      | No          | Permite filtrar movimientos por producto.               |
| `@KRDX_TIPO_MOV_ID`   | `INT`      | No          | Permite filtrar movimientos por tipo de movimiento.     |
| `@KRDX_ESTADO`        | `CHAR(1)`  | No          | Permite filtrar por estado: `A` o `I`.                  |
| `@FECHA_INICIAL`      | `DATETIME` | No          | Fecha inicial del rango de búsqueda.                    |
| `@FECHA_FINAL`        | `DATETIME` | No          | Fecha final del rango de búsqueda.                      |

### Resultado

Retorna un listado de movimientos de Kardex junto con información descriptiva del negocio, producto, usuario creador y tipo de movimiento.

### Campos retornados

| Campo                        | Descripción                             |
| ---------------------------- | --------------------------------------- |
| `KRDX_ID`                    | Identificador del movimiento de Kardex. |
| `KRDX_NEGOCIO_ID`            | Identificador del negocio.              |
| `NEG_NOMBRE`                 | Nombre del negocio.                     |
| `KRDX_INVENTARIO_ID`         | Identificador del inventario.           |
| `KRDX_PROD_ID`               | Identificador del producto.             |
| `PROD_NOMBRE`                | Nombre del producto.                    |
| `KRDX_CREADO_POR_USUARIO_ID` | Identificador del usuario creador.      |
| `CREADO_POR_USUARIO`         | Nombre del usuario creador.             |
| `KRDX_TIPO_MOV_ID`           | Identificador del tipo de movimiento.   |
| `TIPO_MOVIMIENTO`            | Descripción del tipo de movimiento.     |
| `KRDX_CANT_ENTRADA`          | Cantidad de entrada del movimiento.     |
| `KRDX_CANT_SALIDA`           | Cantidad de salida del movimiento.      |
| `KRDX_SALDO_ANTERIOR`        | Saldo antes del movimiento.             |
| `KRDX_SALDO_ACTUAL`          | Saldo después del movimiento.           |
| `KRDX_COSTO_UNIDAD`          | Costo unitario aplicado al movimiento.  |
| `KRDX_COSTO_PONDERADO_ANT`   | Costo ponderado antes del movimiento.   |
| `KRDX_COSTO_PONDERADO_ACT`   | Costo ponderado después del movimiento. |
| `KRDX_TOTAL`                 | Valor total del movimiento.             |
| `KRDX_OBSERVACION`           | Observación del movimiento.             |
| `KRDX_ESTADO`                | Estado del movimiento.                  |
| `KRDX_FECHA_CREACION`        | Fecha de creación del movimiento.       |

### Ejemplos de uso

Listar todos los movimientos:

```sql
EXEC dbo.UspListaKardex;
```

Listar movimientos por negocio:

```sql
EXEC dbo.UspListaKardex
    @KRDX_NEGOCIO_ID = 1;
```

Listar movimientos por producto:

```sql
EXEC dbo.UspListaKardex
    @KRDX_PROD_ID = 1;
```

Listar movimientos por rango de fechas:

```sql
EXEC dbo.UspListaKardex
    @FECHA_INICIAL = '2026-06-01',
    @FECHA_FINAL = '2026-06-30';
```

---

## 6.2. UspConsultaKardex

### Objetivo

Consultar la información de un movimiento de Kardex específico mediante su identificador.

### Nombre

```sql
dbo.UspConsultaKardex
```

### Parámetros de entrada

| Parámetro  | Tipo  | Obligatorio | Descripción                                         |
| ---------- | ----- | ----------- | --------------------------------------------------- |
| `@KRDX_ID` | `INT` | Sí          | Identificador del movimiento de Kardex a consultar. |

### Resultado

Retorna la información completa del movimiento consultado, incluyendo negocio, producto, usuario creador y tipo de movimiento.

### Ejemplo de uso

```sql
EXEC dbo.UspConsultaKardex
    @KRDX_ID = 1;
```

---

## 6.3. UspRegistraMovimientoKardex

### Objetivo

Registrar un movimiento de inventario en Kardex y actualizar automáticamente la ficha de inventario correspondiente.

Este procedimiento es el núcleo operativo del inventario, ya que garantiza que todo movimiento de entrada o salida deje registro histórico y actualice el stock actual del producto.

Además, cuando el movimiento es una entrada, el procedimiento recalcula el costo ponderado del inventario y actualiza el valor base de venta del producto.

El procedimiento está preparado para funcionar de dos maneras:

```text
1. Ejecución independiente.
2. Ejecución interna desde otro procedure transaccional.
```

Esto permite que sea usado directamente para ajustes manuales de inventario o internamente desde procesos como compras, ventas, devoluciones y anulaciones.

### Nombre

```sql
dbo.UspRegistraMovimientoKardex
```

### Parámetros de entrada

| Parámetro                     | Tipo            | Obligatorio      | Descripción                                           |
| ----------------------------- | --------------- | ---------------- | ----------------------------------------------------- |
| `@KRDX_NEGOCIO_ID`            | `INT`           | Sí               | Identificador del negocio donde ocurre el movimiento. |
| `@KRDX_PROD_ID`               | `INT`           | Sí               | Identificador del producto que se mueve.              |
| `@KRDX_TIPO_MOV_ID`           | `INT`           | Sí               | Identificador parametrizado del tipo de movimiento.   |
| `@TIPO_OPERACION`             | `CHAR(1)`       | Sí               | Tipo de operación: `E` para entrada, `S` para salida. |
| `@CANTIDAD`                   | `DECIMAL(18,4)` | Sí               | Cantidad del producto que entra o sale.               |
| `@COSTO_UNIDAD`               | `DECIMAL(18,2)` | Sí para entradas | Costo unitario del movimiento.                        |
| `@KRDX_OBSERVACION`           | `VARCHAR(150)`  | No               | Observación o detalle del movimiento.                 |
| `@KRDX_CREADO_POR_USUARIO_ID` | `INT`           | Sí               | Identificador del usuario que registra el movimiento. |

### Validaciones realizadas

El procedimiento valida que:

1. El negocio sea obligatorio.
2. El producto sea obligatorio.
3. El tipo de movimiento sea obligatorio.
4. El usuario creador sea obligatorio.
5. El tipo de operación sea `E` o `S`.
6. La cantidad sea mayor que cero.
7. En movimientos de entrada, el costo unitario sea obligatorio y no negativo.
8. En movimientos de salida, el costo unitario no sea negativo si se envía.
9. El negocio exista y esté activo.
10. El producto exista, esté activo y pertenezca al negocio del movimiento.
11. El tipo de movimiento exista y esté activo.
12. El usuario creador exista y esté activo.
13. Exista inventario activo para el producto y negocio.
14. En movimientos de salida, exista stock suficiente.

---

## 6.4. Comportamiento transaccional de UspRegistraMovimientoKardex

### Ejecución independiente

Cuando `UspRegistraMovimientoKardex` se ejecuta directamente, sin una transacción previa, el procedure:

```text
1. Abre su propia transacción.
2. Registra el movimiento en TBL_KARDEX.
3. Actualiza TBL_INVENTARIO.
4. Si el movimiento es entrada, recalcula PROD_VALOR_VENTA.
5. Hace COMMIT si todo sale bien.
6. Retorna un resultado estándar con Exitoso = 1.
```

Si ocurre un error:

```text
1. Hace ROLLBACK de su propia transacción.
2. Retorna Exitoso = 0.
3. Retorna el mensaje del error.
```

Ejemplo de uso independiente:

```sql
EXEC dbo.UspRegistraMovimientoKardex
    @KRDX_NEGOCIO_ID = 1,
    @KRDX_PROD_ID = 1,
    @KRDX_TIPO_MOV_ID = 1,
    @TIPO_OPERACION = 'E',
    @CANTIDAD = 10,
    @COSTO_UNIDAD = 25000,
    @KRDX_OBSERVACION = 'Entrada inicial de inventario',
    @KRDX_CREADO_POR_USUARIO_ID = 1;
```

---

### Ejecución desde otro procedure

Cuando `UspRegistraMovimientoKardex` es llamado desde otro procedure que ya abrió una transacción, por ejemplo `UspRegistraCompra`, el procedure:

```text
1. No abre una transacción independiente.
2. No hace COMMIT propio.
3. Crea un punto de retorno usando SAVE TRANSACTION.
4. Registra el movimiento en TBL_KARDEX.
5. Actualiza TBL_INVENTARIO.
6. Si el movimiento es entrada, recalcula PROD_VALOR_VENTA.
7. Deja que el procedure padre decida si hace COMMIT o ROLLBACK.
8. No retorna resultsets intermedios para no contaminar la respuesta del procedure padre.
```

Si ocurre un error dentro de Kardex mientras es llamado desde otro procedure:

```text
1. Intenta volver al punto de retorno SavePoint_Kardex.
2. Propaga el error usando THROW.
3. El procedure padre recibe el error.
4. El procedure padre debe hacer ROLLBACK completo.
```

Ejemplo conceptual:

```text
UspRegistraCompra
    BEGIN TRANSACTION
        Inserta encabezado de compra
        Inserta detalle de compra
        Inserta pagos
        Llama UspRegistraMovimientoKardex
            SAVE TRANSACTION SavePoint_Kardex
            Inserta Kardex
            Actualiza Inventario
            Recalcula valor base de venta si es entrada
        Si todo sale bien:
            COMMIT desde UspRegistraCompra
        Si Kardex falla:
            THROW hacia UspRegistraCompra
            ROLLBACK completo desde UspRegistraCompra
```

---

## 6.5. Comportamiento para entradas

Cuando `@TIPO_OPERACION = 'E'`, el procedimiento:

1. Toma el stock actual del inventario como saldo anterior.
2. Suma la cantidad ingresada al stock actual.
3. Registra la cantidad en `KRDX_CANT_ENTRADA`.
4. Registra `0` en `KRDX_CANT_SALIDA`.
5. Calcula el total del movimiento.
6. Recalcula el costo ponderado.
7. Actualiza `INV_STOCK_ACTUAL`, `INV_COSTO_PONDERADO`, `INV_ULTIMO_COSTO`, `INV_FECHA_ACTUALIZACION` e `INV_MODIFICADO_POR_USUARIO_ID`.
8. Recalcula `PROD_VALOR_VENTA` usando el nuevo costo ponderado y el margen de ganancia del producto.

Ejemplo:

```text
Saldo anterior: 10
Entrada: 5
Saldo actual: 15
```

---

## 6.6. Comportamiento para salidas

Cuando `@TIPO_OPERACION = 'S'`, el procedimiento:

1. Toma el stock actual del inventario como saldo anterior.
2. Valida que exista stock suficiente.
3. Resta la cantidad de salida al stock actual.
4. Registra `0` en `KRDX_CANT_ENTRADA`.
5. Registra la cantidad en `KRDX_CANT_SALIDA`.
6. Usa el costo ponderado actual como costo unitario si no se envía uno explícito.
7. Mantiene el costo ponderado actual.
8. Actualiza `INV_STOCK_ACTUAL`, `INV_FECHA_ACTUALIZACION` e `INV_MODIFICADO_POR_USUARIO_ID`.
9. No recalcula `PROD_VALOR_VENTA`.

Ejemplo:

```text
Saldo anterior: 15
Salida: 3
Saldo actual: 12
```

---

## 6.7. Cálculo de costo ponderado en entradas

Para movimientos de entrada, el costo ponderado actualizado se calcula tomando el valor total anterior del inventario más el valor total de la nueva entrada, dividido entre el nuevo saldo actual.

```text
Total anterior = Saldo anterior * Costo ponderado anterior
Total movimiento = Cantidad entrada * Costo unidad
Costo ponderado actual = (Total anterior + Total movimiento) / Saldo actual
```

Ejemplo:

```text
Saldo anterior: 10
Costo ponderado anterior: 20.000
Entrada: 5
Costo unidad entrada: 25.000

Total anterior: 200.000
Total movimiento: 125.000
Saldo actual: 15

Costo ponderado actual: 21.666,67
```

---

## 6.8. Recalculo del valor base de venta del producto

Cuando el movimiento es una entrada, el procedimiento recalcula el valor base de venta del producto después de actualizar el costo ponderado del inventario.

El valor base de venta no incluye IVA.

El IVA se calcula después en el proceso de venta.

### Fórmula

```text
PROD_VALOR_VENTA = INV_COSTO_PONDERADO * (1 + PROD_PORCENTAJE_MARGEN_GAN / 100)
```

### Ejemplo

```text
Costo ponderado actualizado: 21.666,67
Margen de ganancia: 30%

PROD_VALOR_VENTA = 21.666,67 * 1.30
PROD_VALOR_VENTA = 28.166,67
```

### Casos donde se recalcula

```text
Entrada por compra
Entrada manual de inventario
Ajuste positivo
Devolución de venta, si se maneja como entrada
```

### Casos donde no se recalcula

```text
Salida por venta
Ajuste negativo
Devolución de compra, si se maneja como salida
Anulación de venta, si se maneja como salida
```

---

## 6.9. Resultado del procedure

Cuando `UspRegistraMovimientoKardex` se ejecuta de forma independiente y la operación es exitosa, retorna una respuesta estándar con datos relevantes del movimiento.

| Campo                      | Descripción                                    |
| -------------------------- | ---------------------------------------------- |
| `Exitoso`                  | `1` si la operación fue exitosa.               |
| `Mensaje`                  | Mensaje descriptivo del resultado.             |
| `KRDX_ID`                  | Identificador del movimiento de Kardex creado. |
| `INV_ID`                   | Identificador del inventario afectado.         |
| `SALDO_ANTERIOR`           | Saldo antes del movimiento.                    |
| `SALDO_ACTUAL`             | Saldo después del movimiento.                  |
| `COSTO_PONDERADO_ANTERIOR` | Costo ponderado antes del movimiento.          |
| `COSTO_PONDERADO_ACTUAL`   | Costo ponderado después del movimiento.        |
| `TOTAL_MOVIMIENTO`         | Total del movimiento registrado.               |

Cuando `UspRegistraMovimientoKardex` se ejecuta dentro de otro procedure transaccional, no retorna resultsets intermedios. En ese caso, el resultado final lo debe retornar el procedure padre.

### Ejemplo de respuesta exitosa en ejecución independiente

```text
Exitoso | Mensaje                                       | KRDX_ID | INV_ID | SALDO_ANTERIOR | SALDO_ACTUAL
--------|-----------------------------------------------|---------|--------|----------------|-------------
1       | Movimiento de Kardex registrado correctamente. | 1       | 1      | 10.0000        | 20.0000
```

---

# 7. Flujo recomendado de prueba

Para validar el correcto funcionamiento de los procedimientos, se recomienda ejecutar el siguiente flujo.

## 7.1. Registrar una entrada independiente

```sql
EXEC dbo.UspRegistraMovimientoKardex
    @KRDX_NEGOCIO_ID = 1,
    @KRDX_PROD_ID = 1,
    @KRDX_TIPO_MOV_ID = 1,
    @TIPO_OPERACION = 'E',
    @CANTIDAD = 10,
    @COSTO_UNIDAD = 25000,
    @KRDX_OBSERVACION = 'Entrada inicial de inventario',
    @KRDX_CREADO_POR_USUARIO_ID = 1;
```

Después de ejecutar esta entrada, se recomienda consultar:

```sql
EXEC dbo.UspListaInventario
    @INV_NEGOCIO_ID = 1,
    @INV_PROD_ID = 1;
```

Y luego consultar el producto:

```sql
EXEC dbo.UspConsultaProducto
    @PROD_ID = 1;
```

Validaciones esperadas:

```text
INV_STOCK_ACTUAL aumenta.
INV_COSTO_PONDERADO se recalcula.
INV_ULTIMO_COSTO se actualiza.
PROD_VALOR_VENTA se recalcula como valor base antes de IVA.
```

---

## 7.2. Registrar una salida independiente

```sql
EXEC dbo.UspRegistraMovimientoKardex
    @KRDX_NEGOCIO_ID = 1,
    @KRDX_PROD_ID = 1,
    @KRDX_TIPO_MOV_ID = 2,
    @TIPO_OPERACION = 'S',
    @CANTIDAD = 2,
    @KRDX_OBSERVACION = 'Salida por venta de prueba',
    @KRDX_CREADO_POR_USUARIO_ID = 1;
```

Validaciones esperadas:

```text
INV_STOCK_ACTUAL disminuye.
INV_COSTO_PONDERADO se mantiene.
PROD_VALOR_VENTA se mantiene.
```

---

## 7.3. Listar movimientos

```sql
EXEC dbo.UspListaKardex
    @KRDX_NEGOCIO_ID = 1,
    @KRDX_PROD_ID = 1;
```

---

## 7.4. Consultar un movimiento

```sql
EXEC dbo.UspConsultaKardex
    @KRDX_ID = 1;
```

---

## 7.5. Probar Kardex desde compras

Después de registrar una compra mediante `UspRegistraCompra`, se recomienda consultar Kardex para validar que se haya generado una entrada por cada producto comprado.

```sql
EXEC dbo.UspListaKardex
    @KRDX_NEGOCIO_ID = 1,
    @KRDX_PROD_ID = 1;
```

También se recomienda consultar inventario para confirmar que el stock haya sido actualizado.

```sql
EXEC dbo.UspListaInventario
    @INV_NEGOCIO_ID = 1,
    @INV_PROD_ID = 1;
```

Y consultar producto para confirmar que el valor base de venta haya sido recalculado.

```sql
EXEC dbo.UspConsultaProducto
    @PROD_ID = 1;
```

---

# 8. Reglas de negocio aplicadas

## 8.1. Kardex como histórico

Los registros de Kardex representan movimientos históricos de inventario.

Por esta razón, no se actualizan ni se eliminan directamente mediante procedures estándar.

Si se requiere corregir una operación, debe registrarse un nuevo movimiento compensatorio.

Ejemplo:

```text
Error: se registró una entrada de 10 unidades, pero eran 8.
Corrección: registrar una salida de ajuste por 2 unidades.
```

---

## 8.2. Entrada de inventario

Una entrada aumenta el stock actual del producto.

```text
Stock actual = Stock anterior + Cantidad entrada
```

Las entradas requieren costo unitario porque pueden afectar el costo ponderado del inventario.

Cuando una entrada modifica el costo ponderado, también se recalcula el valor base de venta del producto.

---

## 8.3. Salida de inventario

Una salida disminuye el stock actual del producto.

```text
Stock actual = Stock anterior - Cantidad salida
```

No se permiten salidas si no hay stock suficiente.

Las salidas no modifican el costo ponderado del inventario y no recalculan el valor base de venta del producto.

---

## 8.4. Costo ponderado

El costo ponderado se recalcula únicamente en movimientos de entrada.

Las salidas mantienen el costo ponderado anterior.

Esto permite valorar el inventario bajo un esquema promedio ponderado.

---

## 8.5. Valor base de venta del producto

El valor base de venta del producto se almacena en `TBL_PRODUCTOS.PROD_VALOR_VENTA`.

Este valor:

```text
No es digitado manualmente.
No incluye IVA.
Se calcula desde el costo ponderado del inventario.
Usa el margen de ganancia configurado en el producto.
```

Fórmula:

```text
PROD_VALOR_VENTA = INV_COSTO_PONDERADO * (1 + PROD_PORCENTAJE_MARGEN_GAN / 100)
```

---

## 8.6. Tipo de movimiento parametrizado

El campo `KRDX_TIPO_MOV_ID` apunta a `TBL_PARAMETROS`.

Esto permite definir tipos de movimiento como:

```text
Entrada por compra
Salida por venta
Ajuste positivo
Ajuste negativo
Devolución de venta
Devolución de compra
Anulación de compra
Anulación de venta
```

El parámetro `@TIPO_OPERACION` define si el movimiento suma o resta stock.

---

## 8.7. Actualización automática de inventario

Cada movimiento registrado en Kardex actualiza automáticamente la tabla `TBL_INVENTARIO`.

Se actualizan principalmente:

```text
INV_STOCK_ACTUAL
INV_COSTO_PONDERADO
INV_ULTIMO_COSTO
INV_FECHA_ACTUALIZACION
INV_MODIFICADO_POR_USUARIO_ID
```

---

## 8.8. Control transaccional

El registro de Kardex, la actualización del inventario y el eventual recálculo del valor base de venta ocurren dentro de la misma transacción.

Esto significa que:

```text
Si falla Kardex, no se actualiza inventario.
Si falla inventario, no se registra Kardex.
Si falla el recálculo del valor base de venta, se revierte el movimiento completo.
```

Además, cuando Kardex es llamado desde un procedure padre, como compras o ventas, el `COMMIT` final debe hacerlo el procedure padre.

---

## 8.9. Transacción reutilizable

`UspRegistraMovimientoKardex` está diseñado para ser reutilizado por otros módulos.

Cuando se ejecuta solo:

```text
UspRegistraMovimientoKardex
    BEGIN TRANSACTION
    INSERT TBL_KARDEX
    UPDATE TBL_INVENTARIO
    UPDATE TBL_PRODUCTOS cuando sea entrada
    COMMIT
```

Cuando es llamado desde otro procedure:

```text
UspRegistraCompra
    BEGIN TRANSACTION
    INSERT TBL_COMPRAS
    INSERT TBL_COMPRA_DETALLE
    INSERT TBL_COMPRA_PAGO
    EXEC UspRegistraMovimientoKardex
        SAVE TRANSACTION SavePoint_Kardex
        INSERT TBL_KARDEX
        UPDATE TBL_INVENTARIO
        UPDATE TBL_PRODUCTOS cuando sea entrada
    COMMIT desde UspRegistraCompra
```

Si Kardex falla dentro del procedure padre:

```text
UspRegistraMovimientoKardex
    ROLLBACK TRANSACTION SavePoint_Kardex
    THROW
```

Luego el procedure padre recibe el error y debe hacer:

```text
ROLLBACK TRANSACTION
```

---

## 8.10. Bloqueo preventivo de inventario

El procedure usa bloqueo sobre el registro de inventario al momento de registrar el movimiento.

Esto evita inconsistencias cuando dos usuarios intentan mover el mismo producto al mismo tiempo.

La consulta del inventario usa:

```sql
WITH (UPDLOCK, HOLDLOCK)
```

Esto ayuda a que dos transacciones concurrentes no calculen el mismo saldo anterior al mismo tiempo.

---

## 8.11. Validación producto-negocio

El movimiento de Kardex solo puede registrarse si el producto pertenece al mismo negocio del movimiento.

Esto evita que un producto de un negocio afecte inventario o Kardex de otro negocio.

Validación esperada:

```text
KRDX_NEGOCIO_ID = PROD_NEG_ID
```

---

# 9. Manejo de errores

Los procedures de escritura utilizan `TRY/CATCH`.

El comportamiento de error depende de cómo se ejecute `UspRegistraMovimientoKardex`.

---

## 9.1. Error en ejecución independiente

Si `UspRegistraMovimientoKardex` se ejecuta directamente y ocurre un error:

```text
1. Se revierte su propia transacción.
2. Retorna una respuesta con Exitoso = 0.
3. Retorna el mensaje del error en el campo Mensaje.
```

Ejemplo:

```text
Exitoso = 0
Mensaje = No hay stock suficiente para realizar la salida de inventario.
```

---

## 9.2. Error cuando es llamado desde otro procedure

Si `UspRegistraMovimientoKardex` es llamado desde otro procedure transaccional y ocurre un error:

```text
1. Kardex intenta volver al punto SavePoint_Kardex.
2. Kardex propaga el error usando THROW.
3. El procedure padre recibe el error.
4. El procedure padre debe hacer ROLLBACK completo.
5. El procedure padre retorna la respuesta final de error.
```

Ejemplo conceptual:

```text
UspRegistraCompra
    BEGIN TRANSACTION
    EXEC UspRegistraMovimientoKardex
        Error: No hay stock suficiente
        THROW
    CATCH de UspRegistraCompra
        ROLLBACK TRANSACTION
        SELECT 0 AS Exitoso, ERROR_MESSAGE() AS Mensaje
```

---

# 10. Consideraciones para Backend

Desde el backend, estos procedures pueden ser consumidos desde la capa de repositorio usando Dapper.

## 10.1. Uso esperado

```text
Repository -> Ejecuta Procedure
Application Service -> Aplica reglas adicionales si se requieren
Controller -> Expone endpoint REST
```

---

## 10.2. Endpoints sugeridos

| Método HTTP | Endpoint sugerido         | Procedure                      |
| ----------- | ------------------------- | ------------------------------ |
| `GET`       | `/api/kardex`             | `UspListaKardex              ` |
| `GET`       | `/api/kardex/{id}`        | `UspConsultaKardex`            |
| `POST`      | `/api/kardex/movimientos` | `UspRegistraMovimientoKardex`  |

No se sugieren endpoints `PUT`, `PATCH` o `DELETE` para Kardex, porque no debe modificarse ni eliminarse el histórico directamente.

---

## 10.3. Uso desde compras y ventas

Los procesos de compras y ventas deberán llamar internamente a `UspRegistraMovimientoKardex`.

Ejemplo de compra:

```text
Compra registrada
        ↓
Registra entrada en Kardex
        ↓
Actualiza inventario
        ↓
Recalcula valor base de venta del producto
        ↓
COMMIT desde UspRegistraCompra
```

Ejemplo de venta:

```text
Venta confirmada
        ↓
Registra salida en Kardex
        ↓
Actualiza inventario
        ↓
Mantiene costo ponderado y valor base de venta
        ↓
COMMIT desde UspRegistraVenta
```

Si Kardex falla, el error debe propagarse al procedure padre para que no quede una compra o venta parcial.

---

## 10.4. Consideración sobre valor de venta

El backend no debe calcular el valor base de venta en el frontend.

El valor de venta debe ser resultado de la regla persistida en base de datos:

```text
costo ponderado + margen de ganancia
```

El frontend puede mostrar el valor, pero no debe ser la fuente de verdad.

---

# 11. Notas técnicas

* Ejecutar cada `CREATE OR ALTER PROCEDURE` por separado en DBeaver si la herramienta presenta problemas con separadores de lote.
* Evitar el uso de `GO` en DBeaver si genera errores de sintaxis.
* No consumir directamente la tabla `TBL_KARDEX` desde el backend si ya existen procedures definidos.
* No crear procedures estándar de actualización o eliminación para Kardex.
* Usar movimientos compensatorios para corregir errores.
* Crear primero negocio, producto, usuario, inventario y tipos de movimiento antes de registrar Kardex.
* El producto debe pertenecer al mismo negocio del movimiento.
* `KRDX_CANT_ENTRADA` y `KRDX_CANT_SALIDA` no aceptan `NULL`; por eso se registra `0` cuando el campo no aplica.
* `KRDX_OBSERVACION` permite `NULL`.
* `KRDX_ID` debe ser `IDENTITY(1,1)`.
* `UspRegistraMovimientoKardex` usa `SAVE TRANSACTION SavePoint_Kardex` cuando es llamado dentro de otra transacción.
* `UspRegistraMovimientoKardex` usa `THROW` para propagar errores cuando es llamado desde otro procedure.
* Cuando `UspRegistraMovimientoKardex` es llamado desde compras o ventas, no retorna resultsets intermedios.
* El procedure padre, como `UspRegistraCompra` o `UspRegistraVenta`, es responsable de hacer `COMMIT` o `ROLLBACK` final.
* El procedure `UspRegistraMovimientoKardex` es la base para los procedures transaccionales de compras, ventas, devoluciones, anulaciones y ajustes.
* En movimientos de entrada, `UspRegistraMovimientoKardex` recalcula `PROD_VALOR_VENTA`.
* En movimientos de salida, `UspRegistraMovimientoKardex` no recalcula `PROD_VALOR_VENTA`.
