# Dashboard Stored Procedures

## Objetivo

Este documento describe los stored procedures creados para alimentar el Dashboard del sistema POS.

La finalidad de estos procedimientos es entregar datos agregados, seguros y listos para ser consumidos por el backend, evitando que el frontend tenga que calcular métricas sensibles o recibir información cruda innecesaria.

Los datos del dashboard se dividen por nivel de sensibilidad y permisos.

---

## Stored procedures incluidos

| Stored procedure | Endpoint consumidor | Permiso requerido | Sensibilidad | Estado |
|---|---|---|---|---|
| `UspDashboardResumenBasico` | `GET /api/dashboard/basic-overview` | `dashboard.ver` | Baja/media | Implementado |
| `UspDashboardResumenFinanciero` | `GET /api/dashboard/financial-overview` | `dashboard-finanzas.ver` | Alta | Implementado |
| `UspDashboardResumenOperativo` | `GET /api/dashboard/operational-overview` | `dashboard-operacion.ver` | Media | Pendiente |
| `UspDashboardResumenRentabilidad` | `GET /api/dashboard/profitability-overview` | `dashboard-utilidad.ver` | Muy alta | Pendiente |

---

## Reglas generales

### Seguridad

El frontend nunca debe recibir datos que el usuario no tiene permiso de ver.

Por esta razón, los datos del dashboard no se exponen en un único endpoint global. En su lugar, se agrupan por categoría y sensibilidad.

La autorización debe realizarse en backend mediante `RequirePermission`.

### BusinessId

Los procedimientos reciben `@NEGOCIO_ID`, pero este valor no debe venir desde el frontend.

El backend debe obtener el negocio desde los claims/token del usuario autenticado y pasar ese valor al procedimiento.

No se debe permitir consultar información de otro negocio mediante querystring, body o parámetros enviados por el cliente.

### Datos agregados

Los procedimientos retornan datos agregados y listos para pintar en gráficas o cards.

No deben retornar ventas, compras, kardex o costos crudos para que el frontend haga cálculos sensibles.

### Estados

Los procedimientos consideran únicamente registros activos:

```sql
VENTA_ESTADO = 'A'
VENDET_ESTADO = 'A'
COMPRA_ESTADO = 'A'
COMDET_ESTADO = 'A'
INV_ESTADO = 'A'
KRDX_ESTADO = 'A'
```

Según aplique a cada tabla.

---

# 1. UspDashboardResumenBasico

## Nombre

```sql
dbo.UspDashboardResumenBasico
```

## Endpoint consumidor

```http
GET /api/dashboard/basic-overview
```

## Permiso requerido

```text
dashboard.ver
```

## Objetivo

Entregar la información operativa básica del dashboard principal.

Este procedimiento alimenta las cards y gráficas visibles para usuarios con acceso general al dashboard, sin exponer información financiera sensible como utilidad, costos, margen o balance detallado.

## Sensibilidad

Baja/media.

## Parámetros

| Parámetro | Tipo | Obligatorio | Descripción |
|---|---:|---:|---|
| `@NEGOCIO_ID` | `INT` | Sí | Identificador del negocio. Debe ser obtenido desde claims/token en backend. |
| `@FECHA_INICIO` | `DATE` | No | Fecha inicial del periodo a consultar. Si es `NULL`, se toma por defecto los últimos 7 días. |
| `@FECHA_FIN` | `DATE` | No | Fecha final del periodo a consultar. Si es `NULL`, se toma la fecha actual. |
| `@TOP` | `INT` | No | Cantidad máxima de registros para listas cortas. Por defecto `5`. Máximo recomendado `20`. |

## Validaciones internas

| Validación | Comportamiento |
|---|---|
| `@NEGOCIO_ID IS NULL OR @NEGOCIO_ID <= 0` | Lanza error con `THROW`. |
| `@FECHA_INICIO > @FECHA_FIN` | Lanza error con `THROW`. |
| `@TOP IS NULL OR @TOP <= 0` | Se asigna `5`. |
| `@TOP > 20` | Se limita a `20`. |

## Resultsets

Este procedimiento retorna varios resultsets en un orden fijo. El backend debe leerlos respetando este orden.

---

## Resultset 1: Daily sales

### Propósito

Muestra el total vendido en el día actual, la cantidad de ventas y el porcentaje de cambio frente al día anterior.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `Total` | `DECIMAL(18,2)` | Total vendido hoy. |
| `SalesCount` | `INT` | Cantidad de ventas del día. |
| `PercentageChange` | `DECIMAL(18,2)` | Variación porcentual frente a ayer. |
| `ComparisonLabel` | `VARCHAR` | Texto de comparación. Ejemplo: `vs ayer`. |

### DTO sugerido

```csharp
public sealed class DailySalesDto
{
    public decimal Total { get; set; }
    public int SalesCount { get; set; }
    public decimal PercentageChange { get; set; }
    public string ComparisonLabel { get; set; } = string.Empty;
}
```

---

## Resultset 2: Average ticket

### Propósito

Calcula el ticket promedio del periodo seleccionado.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `Value` | `DECIMAL(18,2)` | Ticket promedio. |
| `SalesCount` | `INT` | Cantidad de ventas consideradas. |

### DTO sugerido

```csharp
public sealed class AverageTicketDto
{
    public decimal Value { get; set; }
    public int SalesCount { get; set; }
}
```

---

## Resultset 3: Weekly sales

### Propósito

Muestra ventas agrupadas por día dentro del periodo consultado.

Este resultset alimenta la gráfica de barras de ventas semanales.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `Label` | `VARCHAR` | Día abreviado. Ejemplo: `Lun`, `Mar`, `Mié`. |
| `Date` | `DATE` | Fecha del día. |
| `Total` | `DECIMAL(18,2)` | Total vendido en el día. |
| `SalesCount` | `INT` | Cantidad de ventas del día. |
| `AverageTicket` | `DECIMAL(18,2)` | Ticket promedio del día. |

### DTO sugerido

```csharp
public sealed class WeeklySalesItemDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public int SalesCount { get; set; }
    public decimal AverageTicket { get; set; }
}
```

---

## Resultset 4: Sales by category

### Propósito

Muestra la distribución de ventas por categoría de producto.

Este resultset alimenta gráficas tipo dona, pie o barras por categoría.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `CategoryId` | `INT` | Identificador de la categoría. |
| `CategoryName` | `VARCHAR` | Nombre de la categoría. |
| `Total` | `DECIMAL(18,2)` | Total vendido en la categoría. |
| `Percentage` | `DECIMAL(18,2)` | Participación porcentual de la categoría. |

### DTO sugerido

```csharp
public sealed class SalesByCategoryItemDto
{
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Percentage { get; set; }
}
```

---

## Resultset 5: Recent sales

### Propósito

Lista las ventas recientes del negocio.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `SaleId` | `INT` | Identificador de la venta. |
| `InvoiceNumber` | `VARCHAR` | Número visual de factura o venta. |
| `CustomerName` | `VARCHAR` | Nombre del tercero/cliente. |
| `Time` | `VARCHAR(5)` | Hora de la venta en formato `HH:mm`. |
| `PaymentMethod` | `VARCHAR` | Medio o medios de pago asociados. |
| `Status` | `CHAR(1)` | Estado de la venta. |
| `Total` | `DECIMAL(18,2)` | Total de la venta. |

### DTO sugerido

```csharp
public sealed class RecentSaleItemDto
{
    public int SaleId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
```

---

## Resultset 6: Inventory alerts

### Propósito

Lista productos cuyo stock actual está por debajo del stock mínimo.

Este resultset alimenta el panel de alertas de inventario.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `ProductId` | `INT` | Identificador del producto. |
| `ProductName` | `VARCHAR` | Nombre del producto. |
| `Sku` | `VARCHAR` | Código externo o identificador visual del producto. |
| `CurrentStock` | `DECIMAL` / `INT` | Stock actual. |
| `MinimumStock` | `DECIMAL` / `INT` | Stock mínimo configurado. |
| `Severity` | `VARCHAR` | Severidad calculada: `Critical`, `Warning`, `Normal`. |

### Reglas de severidad

| Condición | Severidad |
|---|---|
| `STOCK_ACTUAL <= STOCK_MINIMO * 0.30` | `Critical` |
| `STOCK_ACTUAL < STOCK_MINIMO` | `Warning` |
| Resto de casos | `Normal` |

### DTO sugerido

```csharp
public sealed class InventoryAlertItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinimumStock { get; set; }
    public string Severity { get; set; } = string.Empty;
}
```

---

## Resultset 7: Stock alerts count

### Propósito

Muestra el total de alertas de inventario y cuántas son críticas.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `TotalAlerts` | `INT` | Cantidad total de productos bajo stock mínimo. |
| `CriticalAlerts` | `INT` | Cantidad de productos en severidad crítica. |

### DTO sugerido

```csharp
public sealed class StockAlertsCountDto
{
    public int TotalAlerts { get; set; }
    public int CriticalAlerts { get; set; }
}
```

---

## Resultset 8: Recent kardex movements

### Propósito

Lista los movimientos recientes de kardex.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `KardexId` | `INT` | Identificador del movimiento kardex. |
| `ProductId` | `INT` | Identificador del producto. |
| `ProductName` | `VARCHAR` | Nombre del producto. |
| `MovementType` | `VARCHAR` | Tipo de movimiento. |
| `Quantity` | `DECIMAL` / `INT` | Cantidad entrada o salida. |
| `Date` | `DATETIME` | Fecha del movimiento. |

### DTO sugerido

```csharp
public sealed class RecentKardexMovementItemDto
{
    public int KardexId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime Date { get; set; }
}
```

---

## Ejemplo de ejecución

```sql
EXEC dbo.UspDashboardResumenBasico
    @NEGOCIO_ID = 5,
    @TOP = 5;
GO
```

Con rango de fechas:

```sql
EXEC dbo.UspDashboardResumenBasico
    @NEGOCIO_ID = 5,
    @FECHA_INICIO = '2026-06-01',
    @FECHA_FIN = '2026-06-30',
    @TOP = 5;
GO
```

---

# 2. UspDashboardResumenFinanciero

## Nombre

```sql
dbo.UspDashboardResumenFinanciero
```

## Endpoint consumidor

```http
GET /api/dashboard/financial-overview
```

## Permiso requerido

```text
dashboard-finanzas.ver
```

## Objetivo

Entregar información financiera agregada para el dashboard.

Este procedimiento alimenta gráficas y cards sensibles como ventas vs compras, balance operativo, compras del mes, ventas del mes, proyección de cierre y ventas por método de pago.

No calcula utilidad, margen bruto ni costo de productos vendidos. Esa información queda reservada para una fase posterior.

## Sensibilidad

Alta.

## Parámetros

| Parámetro | Tipo | Obligatorio | Descripción |
|---|---:|---:|---|
| `@NEGOCIO_ID` | `INT` | Sí | Identificador del negocio. Debe ser obtenido desde claims/token en backend. |
| `@FECHA_INICIO` | `DATE` | No | Fecha inicial del periodo a consultar. Si es `NULL`, toma el primer día del mes actual. |
| `@FECHA_FIN` | `DATE` | No | Fecha final del periodo a consultar. Si es `NULL`, toma la fecha actual. |

## Validaciones internas

| Validación | Comportamiento |
|---|---|
| `@NEGOCIO_ID IS NULL OR @NEGOCIO_ID <= 0` | Lanza error con `THROW`. |
| `@FECHA_INICIO > @FECHA_FIN` | Lanza error con `THROW`. |

---

## Resultsets

Este procedimiento retorna varios resultsets en un orden fijo. El backend debe leerlos respetando este orden.

---

## Resultset 1: Monthly sales / period sales

### Propósito

Muestra el total vendido en el periodo consultado y la variación frente al periodo anterior equivalente.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `Total` | `DECIMAL(18,2)` | Total vendido en el periodo. |
| `PercentageChange` | `DECIMAL(18,2)` | Variación porcentual frente al periodo anterior. |
| `ComparisonLabel` | `VARCHAR` | Texto de comparación. Ejemplo: `vs periodo anterior`. |

### DTO sugerido

```csharp
public sealed class MonthlySalesDto
{
    public decimal Total { get; set; }
    public decimal PercentageChange { get; set; }
    public string ComparisonLabel { get; set; } = string.Empty;
}
```

---

## Resultset 2: Monthly purchases / period purchases

### Propósito

Muestra el total comprado en el periodo consultado y la variación frente al periodo anterior equivalente.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `Total` | `DECIMAL(18,2)` | Total comprado en el periodo. |
| `PercentageChange` | `DECIMAL(18,2)` | Variación porcentual frente al periodo anterior. |
| `ComparisonLabel` | `VARCHAR` | Texto de comparación. Ejemplo: `vs periodo anterior`. |

### DTO sugerido

```csharp
public sealed class MonthlyPurchasesDto
{
    public decimal Total { get; set; }
    public decimal PercentageChange { get; set; }
    public string ComparisonLabel { get; set; } = string.Empty;
}
```

---

## Resultset 3: Sales vs purchases

### Propósito

Muestra ventas y compras agrupadas por semana dentro del periodo consultado.

Este resultset alimenta la gráfica de barras de ventas vs compras.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `Label` | `VARCHAR` | Etiqueta del periodo. Ejemplo: `Semana 1`. |
| `PeriodStart` | `DATE` | Fecha inicial del periodo. |
| `PeriodEnd` | `DATE` | Fecha final del periodo. |
| `SalesTotal` | `DECIMAL(18,2)` | Total vendido en el periodo. |
| `PurchaseTotal` | `DECIMAL(18,2)` | Total comprado en el periodo. |
| `Balance` | `DECIMAL(18,2)` | Diferencia entre ventas y compras. |

### DTO sugerido

```csharp
public sealed class SalesVsPurchasesItemDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal SalesTotal { get; set; }
    public decimal PurchaseTotal { get; set; }
    public decimal Balance { get; set; }
}
```

---

## Resultset 4: Operational balance

### Propósito

Muestra un balance operativo bruto del periodo.

Este valor se calcula como:

```text
ventas - compras
```

No representa utilidad neta, ya que no considera gastos operativos, nómina, arriendo, impuestos, descuentos, devoluciones ni otros costos.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `SalesTotal` | `DECIMAL(18,2)` | Total vendido en el periodo. |
| `PurchaseTotal` | `DECIMAL(18,2)` | Total comprado en el periodo. |
| `Balance` | `DECIMAL(18,2)` | Balance operativo bruto. |

### DTO sugerido

```csharp
public sealed class OperationalBalanceDto
{
    public decimal SalesTotal { get; set; }
    public decimal PurchaseTotal { get; set; }
    public decimal Balance { get; set; }
}
```

---

## Resultset 5: Monthly projection

### Propósito

Calcula una proyección simple de cierre de mes basada en el promedio diario de ventas del mes actual.

La fórmula utilizada es:

```text
promedio diario = ventas actuales del mes / días transcurridos
proyección = promedio diario * días del mes
```

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `CurrentMonthSales` | `DECIMAL(18,2)` | Ventas acumuladas del mes actual. |
| `ElapsedDays` | `INT` | Días transcurridos del mes. |
| `DaysInMonth` | `INT` | Total de días del mes. |
| `DailyAverage` | `DECIMAL(18,2)` | Promedio diario de ventas. |
| `ProjectedMonthSales` | `DECIMAL(18,2)` | Proyección simple de ventas al cierre del mes. |

### DTO sugerido

```csharp
public sealed class MonthlyProjectionDto
{
    public decimal CurrentMonthSales { get; set; }
    public int ElapsedDays { get; set; }
    public int DaysInMonth { get; set; }
    public decimal DailyAverage { get; set; }
    public decimal ProjectedMonthSales { get; set; }
}
```

---

## Resultset 6: Sales by payment method

### Propósito

Muestra la distribución de ventas por método de pago.

Este resultset puede alimentar una gráfica de dona, barras o tabla de resumen para caja.

### Columnas

| Columna | Tipo esperado | Descripción |
|---|---:|---|
| `PaymentMethodId` | `INT` | Identificador del método de pago. |
| `PaymentMethod` | `VARCHAR` | Nombre del método de pago. |
| `Total` | `DECIMAL(18,2)` | Total recibido por este método. |
| `Percentage` | `DECIMAL(18,2)` | Participación porcentual del método de pago. |

### DTO sugerido

```csharp
public sealed class SalesByPaymentMethodItemDto
{
    public int? PaymentMethodId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal Percentage { get; set; }
}
```

---

## Ejemplo de ejecución

```sql
EXEC dbo.UspDashboardResumenFinanciero
    @NEGOCIO_ID = 5;
GO
```

Con rango de fechas:

```sql
EXEC dbo.UspDashboardResumenFinanciero
    @NEGOCIO_ID = 5,
    @FECHA_INICIO = '2026-06-01',
    @FECHA_FIN = '2026-06-30';
GO
```

---

# Stored procedures previstos para fase posterior

## UspDashboardResumenOperativo

### Endpoint previsto

```http
GET /api/dashboard/operational-overview
```

### Permiso previsto

```text
dashboard-operacion.ver
```

### Sensibilidad

Media.

### Estado

Pendiente de implementación.

### Objetivo previsto

Entregar información operativa más detallada sobre compras, inventario, rotación de productos y movimientos de kardex.

### Información prevista

- Compras por categoría.
- Movimientos de inventario por tipo.
- Productos con mayor rotación.
- Productos con baja rotación.
- Alertas críticas de inventario.
- Entradas vs salidas de inventario.
- Movimientos de kardex agrupados.

### Motivo de postergación

Parte de esta información requiere más validación sobre reglas de inventario, rotación, Kardex y comportamiento operativo esperado.

---

## UspDashboardResumenRentabilidad

### Endpoint previsto

```http
GET /api/dashboard/profitability-overview
```

### Permiso previsto

```text
dashboard-utilidad.ver
```

### Sensibilidad

Muy alta.

### Estado

Pendiente de implementación.

### Objetivo previsto

Entregar información relacionada con utilidad, margen bruto, costos estimados y rentabilidad por categoría/producto.

### Información prevista

- Margen bruto.
- Costo estimado de productos vendidos.
- Porcentaje de margen bruto.
- Margen por categoría.
- Productos más rentables.
- Productos con menor margen.
- Costo de productos vendidos.

### Motivo de postergación

No se implementa en esta fase porque requiere validar con precisión:

- Costo ponderado.
- Kardex.
- Costo real de venta.
- Anulaciones.
- Devoluciones.
- Descuentos.
- IVA.
- Consistencia histórica de inventario.

Mostrar utilidad o margen con datos incompletos puede generar indicadores incorrectos.

---

# Integración backend esperada

## Repositorio

El backend debe consumir estos stored procedures desde un repositorio de Dashboard siguiendo el patrón actual del proyecto.

Ejemplo conceptual:

```csharp
public interface IDashboardRepository
{
    Task<DashboardBasicOverviewDto> GetBasicOverviewAsync(
        int businessId,
        DateTime? startDate,
        DateTime? endDate,
        int top);

    Task<DashboardFinancialOverviewDto> GetFinancialOverviewAsync(
        int businessId,
        DateTime? startDate,
        DateTime? endDate);
}
```

## Application

La capa Application debe:

- Obtener el `BusinessId` desde el usuario autenticado.
- No aceptar `BusinessId` desde frontend.
- Validar parámetros de fecha.
- Llamar al repositorio.
- Retornar `Response<T>` según el patrón actual.
- No mezclar datos financieros en el overview básico.

## Controller

El controller debe exponer:

```http
GET /api/dashboard/basic-overview
GET /api/dashboard/financial-overview
```

Y protegerlos con permisos:

```csharp
[RequirePermission("dashboard.ver")]
[RequirePermission("dashboard-finanzas.ver")]
```

---

# Consideraciones para frontend

El frontend debe:

- Mantener el diseño actual del dashboard.
- Reemplazar datos mock por datos reales.
- Consumir `basic-overview` para la vista principal.
- Consumir `financial-overview` solo si el usuario tiene permiso financiero.
- Mostrar loading state.
- Mostrar empty state.
- Mostrar error state.
- Formatear moneda en COP.
- No calcular utilidad, costos ni márgenes.
- No recibir ni procesar datos sensibles si no tiene permiso.

---

# Consideraciones de seguridad

## Correcto

```text
Backend valida permiso → Backend consulta SP autorizado → Frontend pinta datos permitidos
```

## Incorrecto

```text
Backend manda todo → Frontend oculta secciones sensibles
```

El segundo enfoque no es seguro porque los datos pueden verse en la pestaña Network del navegador.

---

# Resumen

Los SPs implementados en esta fase son:

```text
UspDashboardResumenBasico
UspDashboardResumenFinanciero
```

Estos permiten conectar el dashboard con datos reales para:

- Ventas del día.
- Ticket promedio.
- Ventas semanales.
- Ventas por categoría.
- Ventas recientes.
- Productos bajo stock.
- Alertas de inventario.
- Kardex reciente.
- Ventas del periodo.
- Compras del periodo.
- Ventas vs compras.
- Balance operativo bruto.
- Proyección simple de cierre de mes.
- Ventas por método de pago.

Los SPs pendientes son:

```text
UspDashboardResumenOperativo
UspDashboardResumenRentabilidad
```

Estos quedan previstos para una fase posterior por su mayor complejidad operativa y sensibilidad financiera.
---

# Scripts SQL

Los scripts ejecutables (`CREATE OR ALTER`, repetibles) están en el repo:

| Script | Contenido |
|---|---|
| `database/procedures/UspDashboardResumenBasico.sql` | SP del resumen básico (8 resultsets) |
| `database/procedures/UspDashboardResumenFinanciero.sql` | SP del resumen financiero (6 resultsets) |
| `database/seeds/DashboardPermissions.sql` | Módulos, acción `ver`, permisos y asignación a roles |

Supuestos de la implementación (el esquema no tiene estos datos de forma directa):

- Total de venta/compra = suma del detalle activo (`VENDET_TOTAL` / `COMDET_TOTAL`).
- `InvoiceNumber` = `V-` + `VENTA_ID` con ceros a la izquierda (no existe número de factura).
- Categoría o método de pago sin dato devuelven id `0` (las entidades C# usan `int` no nulo).
- Ventas por método de pago = valor recibido − valor de cambio.
- Rango máximo de fechas: 366 días (error 50003).
- Códigos de error: 50001 negocio inválido, 50002 fechas invertidas, 50003 rango excesivo.
