/*
  dbo.UspDashboardResumenBasico
  Especificacion: docs/database/procedures/README_SP's_DASHBOARD.md (seccion 1)
  Consumidor:     DashboardRepository.GetBasicOverviewAsync (lee 8 resultsets EN ESTE ORDEN)

  Notas de reconstruccion:
  - TBL_VENTAS no tiene total: el total de una venta es SUM(VENDET_TOTAL) de su detalle activo.
  - No existe numero de factura: InvoiceNumber = 'V-' + VENTA_ID con ceros a la izquierda.
  - Las entidades C# usan int NO nulo, por eso categoria sin dato devuelve 0.
  - Solo considera registros con estado 'A'.
*/
CREATE OR ALTER PROCEDURE dbo.UspDashboardResumenBasico
    @NEGOCIO_ID   INT,
    @FECHA_INICIO DATE = NULL,
    @FECHA_FIN    DATE = NULL,
    @TOP          INT  = 5
AS
BEGIN
    SET NOCOUNT ON;

    IF @NEGOCIO_ID IS NULL OR @NEGOCIO_ID <= 0
        THROW 50001, 'El negocio es obligatorio.', 1;

    DECLARE @Hoy DATE = CAST(GETDATE() AS DATE);
    SET @FECHA_FIN    = ISNULL(@FECHA_FIN, @Hoy);
    SET @FECHA_INICIO = ISNULL(@FECHA_INICIO, DATEADD(DAY, -6, @FECHA_FIN));

    IF @FECHA_INICIO > @FECHA_FIN
        THROW 50002, 'La fecha inicial no puede ser mayor que la fecha final.', 1;
    IF DATEDIFF(DAY, @FECHA_INICIO, @FECHA_FIN) > 366
        THROW 50003, 'El rango de fechas no puede superar 366 dias.', 1;

    IF @TOP IS NULL OR @TOP <= 0 SET @TOP = 5;
    IF @TOP > 20 SET @TOP = 20;

    -- Rango a leer: el periodo pedido mas hoy y ayer (necesarios para "vs ayer")
    DECLARE @Desde DATE = CASE WHEN @FECHA_INICIO < DATEADD(DAY, -1, @Hoy) THEN @FECHA_INICIO ELSE DATEADD(DAY, -1, @Hoy) END;
    DECLARE @Hasta DATE = CASE WHEN @FECHA_FIN > @Hoy THEN @FECHA_FIN ELSE @Hoy END;

    -- Total por venta activa (una sola pasada, reutilizada por varios resultsets)
    SELECT v.VENTA_ID AS VentaId, v.VENTA_TER_ID AS TerceroId, v.VENTA_FECHA AS Fecha,
           CAST(ISNULL(SUM(d.VENDET_TOTAL), 0) AS DECIMAL(18,2)) AS Total
    INTO #Ventas
    FROM dbo.TBL_VENTAS v
    LEFT JOIN dbo.TBL_VENTA_DETALLE d ON d.VENTDE_VENTA_ID = v.VENTA_ID AND d.VENDET_ESTADO = 'A'
    WHERE v.VENTA_NEG_ID = @NEGOCIO_ID
      AND v.VENTA_ESTADO = 'A'
      AND v.VENTA_FECHA >= @Desde AND v.VENTA_FECHA < DATEADD(DAY, 1, @Hasta)
    GROUP BY v.VENTA_ID, v.VENTA_TER_ID, v.VENTA_FECHA;

    -- 1. Daily sales (hoy vs ayer)
    DECLARE @TotalHoy DECIMAL(18,2), @CantHoy INT, @TotalAyer DECIMAL(18,2);
    SELECT @TotalHoy  = ISNULL(SUM(CASE WHEN CAST(Fecha AS DATE) = @Hoy THEN Total END), 0),
           @CantHoy   = ISNULL(SUM(CASE WHEN CAST(Fecha AS DATE) = @Hoy THEN 1 END), 0),
           @TotalAyer = ISNULL(SUM(CASE WHEN CAST(Fecha AS DATE) = DATEADD(DAY, -1, @Hoy) THEN Total END), 0)
    FROM #Ventas;

    SELECT @TotalHoy AS Total,
           @CantHoy  AS SalesCount,
           CAST(CASE WHEN @TotalAyer = 0 THEN CASE WHEN @TotalHoy > 0 THEN 100 ELSE 0 END
                     ELSE (@TotalHoy - @TotalAyer) * 100.0 / @TotalAyer END AS DECIMAL(18,2)) AS PercentageChange,
           CAST('vs ayer' AS VARCHAR(30)) AS ComparisonLabel;

    -- 2. Average ticket (periodo)
    SELECT CAST(ISNULL(AVG(Total), 0) AS DECIMAL(18,2)) AS Value,
           COUNT(*) AS SalesCount
    FROM #Ventas
    WHERE Fecha >= @FECHA_INICIO AND Fecha < DATEADD(DAY, 1, @FECHA_FIN);

    -- 3. Weekly sales (un registro por dia del periodo, incluye dias sin ventas)
    ;WITH Dias AS (
        SELECT @FECHA_INICIO AS Dia
        UNION ALL
        SELECT DATEADD(DAY, 1, Dia) FROM Dias WHERE Dia < @FECHA_FIN
    )
    SELECT CASE DATEDIFF(DAY, '19000101', d.Dia) % 7  -- 1900-01-01 fue lunes
               WHEN 0 THEN N'Lun' WHEN 1 THEN N'Mar' WHEN 2 THEN N'Mi' + NCHAR(233)
               WHEN 3 THEN N'Jue' WHEN 4 THEN N'Vie' WHEN 5 THEN N'S' + NCHAR(225) + N'b'
               ELSE N'Dom' END AS Label,
           d.Dia AS [Date],
           CAST(ISNULL(SUM(v.Total), 0) AS DECIMAL(18,2)) AS Total,
           COUNT(v.VentaId) AS SalesCount,
           CAST(ISNULL(AVG(v.Total), 0) AS DECIMAL(18,2)) AS AverageTicket
    FROM Dias d
    LEFT JOIN #Ventas v ON CAST(v.Fecha AS DATE) = d.Dia
    GROUP BY d.Dia
    ORDER BY d.Dia
    OPTION (MAXRECURSION 400);

    -- 4. Sales by category (periodo)
    ;WITH Cat AS (
        SELECT ISNULL(p.PROD_CATEGORIA_ID, 0) AS CategoryId,
               ISNULL(c.PARAM_DESCRIPCION, N'Sin categor' + NCHAR(237) + N'a') AS CategoryName,
               SUM(d.VENDET_TOTAL) AS Total
        FROM #Ventas v
        JOIN dbo.TBL_VENTA_DETALLE d ON d.VENTDE_VENTA_ID = v.VentaId AND d.VENDET_ESTADO = 'A'
        JOIN dbo.TBL_PRODUCTOS p ON p.PROD_ID = d.VENTDE_PROD_ID
        LEFT JOIN dbo.TBL_PARAMETROS c ON c.PARAM_ID = p.PROD_CATEGORIA_ID
        WHERE v.Fecha >= @FECHA_INICIO AND v.Fecha < DATEADD(DAY, 1, @FECHA_FIN)
        GROUP BY ISNULL(p.PROD_CATEGORIA_ID, 0), ISNULL(c.PARAM_DESCRIPCION, N'Sin categor' + NCHAR(237) + N'a')
    )
    SELECT CategoryId, CategoryName,
           CAST(Total AS DECIMAL(18,2)) AS Total,
           CAST(Total * 100.0 / NULLIF(SUM(Total) OVER (), 0) AS DECIMAL(18,2)) AS Percentage
    FROM Cat
    ORDER BY Total DESC;

    -- 5. Recent sales (las ultimas @TOP del negocio, sin limitar al periodo)
    SELECT TOP (@TOP)
           v.VENTA_ID AS SaleId,
           'V-' + RIGHT('000000' + CAST(v.VENTA_ID AS VARCHAR(10)), 6) AS InvoiceNumber,
           ISNULL(t.TER_NOMBRE, '') AS CustomerName,
           CONVERT(VARCHAR(5), v.VENTA_FECHA, 108) AS [Time],
           ISNULL((SELECT STRING_AGG(m.PARAM_DESCRIPCION, ', ')
                   FROM (SELECT DISTINCT pg.VENPA_MEDIOPAGO_ID FROM dbo.TBL_VENTA_PAGO pg
                         WHERE pg.VENPA_VENTA_ID = v.VENTA_ID AND pg.VENPA_ESTADO = 'A') x
                   JOIN dbo.TBL_PARAMETROS m ON m.PARAM_ID = x.VENPA_MEDIOPAGO_ID), '') AS PaymentMethod,
           v.VENTA_ESTADO AS [Status],
           CAST(ISNULL((SELECT SUM(d.VENDET_TOTAL) FROM dbo.TBL_VENTA_DETALLE d
                        WHERE d.VENTDE_VENTA_ID = v.VENTA_ID AND d.VENDET_ESTADO = 'A'), 0) AS DECIMAL(18,2)) AS Total
    FROM dbo.TBL_VENTAS v
    LEFT JOIN dbo.TBL_TERCEROS t ON t.TER_ID = v.VENTA_TER_ID
    WHERE v.VENTA_NEG_ID = @NEGOCIO_ID AND v.VENTA_ESTADO = 'A'
    ORDER BY v.VENTA_FECHA DESC, v.VENTA_ID DESC;

    -- Alertas de inventario (base para los resultsets 6 y 7)
    SELECT p.PROD_ID AS ProductId, p.PROD_NOMBRE AS ProductName, ISNULL(p.PROD_EXTERNAL_ID, '') AS Sku,
           CAST(i.INV_STOCK_ACTUAL AS DECIMAL(18,2)) AS CurrentStock,
           CAST(i.INV_STOCK_MINIMO AS DECIMAL(18,2)) AS MinimumStock,
           CASE WHEN i.INV_STOCK_ACTUAL <= i.INV_STOCK_MINIMO * 0.30 THEN 'Critical' ELSE 'Warning' END AS Severity
    INTO #Alertas
    FROM dbo.TBL_INVENTARIO i
    JOIN dbo.TBL_PRODUCTOS p ON p.PROD_ID = i.INV_PROD_ID AND p.PROD_ESTADO = 'A'
    WHERE i.INV_NEGOCIO_ID = @NEGOCIO_ID AND i.INV_ESTADO = 'A'
      AND i.INV_STOCK_ACTUAL < i.INV_STOCK_MINIMO;

    -- 6. Inventory alerts (criticas primero)
    SELECT TOP (@TOP) ProductId, ProductName, Sku, CurrentStock, MinimumStock, Severity
    FROM #Alertas
    ORDER BY CASE Severity WHEN 'Critical' THEN 0 ELSE 1 END, CurrentStock * 1.0 / NULLIF(MinimumStock, 0), ProductName;

    -- 7. Stock alerts count
    SELECT COUNT(*) AS TotalAlerts,
           ISNULL(SUM(CASE WHEN Severity = 'Critical' THEN 1 ELSE 0 END), 0) AS CriticalAlerts
    FROM #Alertas;

    -- 8. Recent kardex movements
    SELECT TOP (@TOP)
           k.KRDX_ID AS KardexId,
           k.KRDX_PROD_ID AS ProductId,
           p.PROD_NOMBRE AS ProductName,
           ISNULL(m.PARAM_DESCRIPCION, '') AS MovementType,
           CAST(CASE WHEN k.KRDX_CANT_ENTRADA > 0 THEN k.KRDX_CANT_ENTRADA ELSE k.KRDX_CANT_SALIDA END AS DECIMAL(18,2)) AS Quantity,
           k.KRDX_FECHA_CREACION AS [Date]
    FROM dbo.TBL_KARDEX k
    JOIN dbo.TBL_PRODUCTOS p ON p.PROD_ID = k.KRDX_PROD_ID
    LEFT JOIN dbo.TBL_PARAMETROS m ON m.PARAM_ID = k.KRDX_TIPO_MOV_ID
    WHERE k.KRDX_NEGOCIO_ID = @NEGOCIO_ID AND k.KRDX_ESTADO = 'A'
    ORDER BY k.KRDX_FECHA_CREACION DESC, k.KRDX_ID DESC;
END
GO
