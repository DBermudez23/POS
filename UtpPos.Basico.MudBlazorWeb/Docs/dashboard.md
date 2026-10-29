# Dashboard Blazor

## Flujo de negocio

- Usuario con claim `usr_neg_id`: el dashboard carga automáticamente y no muestra selector de negocio.
- Usuario sin claim `usr_neg_id`: el dashboard muestra selector de negocio y no consulta métricas hasta seleccionar uno.
- El selector se muestra solo si el usuario tiene `dashboard-global.ver` y puede listar negocios.

## Endpoints consumidos

- Usuario con negocio: `GET /api/dashboard/basic-overview`.
- Usuario con negocio y permiso financiero: `GET /api/dashboard/financial-overview`.
- Usuario global con negocio seleccionado: `GET /api/dashboard/basic-overview?businessId={id}&top=5`.
- Usuario global financiero: `GET /api/dashboard/financial-overview?businessId={id}`.

## Permisos

- `dashboard.ver`: permite ver el dashboard básico.
- `dashboard-global.ver`: permite seleccionar negocio cuando el usuario no tiene `usr_neg_id`.
- `dashboard-finanzas.ver`: permite solicitar y renderizar datos financieros.

No usar `dashboard.global.ver`, `dashboard-global-ver` ni `dashboard.finanzas.ver`.

## UI conectada

- Cards principales con ventas, compras, ticket promedio y alertas.
- Gráfica de ventas semanales desde `weeklySales`.
- Gráfica ventas vs compras desde `salesVsPurchases`.
- Alertas de inventario desde `inventoryAlerts`.
- Ventas recientes desde `recentSales`.
- Ventas por categoría desde `salesByCategory`.
- Movimientos recientes desde `recentKardexMovements`.

El frontend no calcula métricas sensibles y no solicita `financial-overview` sin `dashboard-finanzas.ver`.
