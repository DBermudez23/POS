# Dashboard API

## Alcance actual

El backend expone los resúmenes `basic-overview` y `financial-overview` usando únicamente stored procedures (sin SQL inline). Los scripts de los SP están en `database/procedures/` y los permisos RBAC del dashboard en `database/seeds/DashboardPermissions.sql`.

## Endpoints

| Endpoint | Permiso | Stored procedure |
|---|---|---|
| `GET /api/dashboard/basic-overview` | `dashboard.ver` | `dbo.UspDashboardResumenBasico` |
| `GET /api/dashboard/financial-overview` | `dashboard-finanzas.ver` | `dbo.UspDashboardResumenFinanciero` |

## Parámetros query permitidos

| Endpoint | Parámetros |
|---|---|
| `basic-overview` | `businessId`, `startDate`, `endDate`, `top` |
| `financial-overview` | `businessId`, `startDate`, `endDate` |

`businessId` solo se acepta para usuarios globales sin `usr_neg_id` y con permiso `dashboard-global.ver`.

## BusinessId

El negocio se resuelve en backend y se pasa como `@NEGOCIO_ID` al stored procedure.

Reglas:

- Si el token tiene `usr_neg_id > 0`, siempre se usa ese negocio y se ignora cualquier `businessId` del querystring.
- Si el token no tiene `usr_neg_id` y no llega `businessId`, la API responde error controlado indicando que debe seleccionar un negocio.
- Si el token no tiene `usr_neg_id` y llega `businessId`, el backend valida `dashboard-global.ver` antes de consultar métricas.
- `financial-overview` exige siempre `dashboard-finanzas.ver` y, para usuarios globales, también `dashboard-global.ver`.

Permisos válidos:

- `dashboard.ver`.
- `dashboard-finanzas.ver`.
- `dashboard-global.ver`.

No usar `dashboard.global.ver`, `dashboard-global-ver` ni `dashboard.finanzas.ver`.

## Validaciones

La capa Application valida:

- `businessId > 0`.
- `startDate <= endDate` cuando ambas fechas existen.
- `top` por defecto en `5` para `basic-overview`.
- `top` limitado a máximo `20` para `basic-overview`.

## Frontend Blazor

El Dashboard Blazor:

- Carga métricas automáticamente si el usuario tiene `usr_neg_id`.
- Muestra selector de negocio para usuarios globales sin `usr_neg_id`.
- No llama endpoints de métricas hasta que el usuario global seleccione negocio.
- Llama `financial-overview` solo si existe permiso `dashboard-finanzas.ver`.
- Envía `businessId` solo en el caso global con negocio seleccionado.

## Pendiente

No están implementados todavía:

- `GET /api/dashboard/operational-overview`.
- `GET /api/dashboard/profitability-overview`.
- Métricas de utilidad, margen o costo de ventas.
