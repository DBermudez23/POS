/*
  dbo.UspDashboardResumenFinanciero
  Especificacion: docs/database/procedures/README_SP's_DASHBOARD.md (seccion 2)
  Consumidor:     DashboardRepository.GetFinancialOverviewAsync (lee 6 resultsets EN ESTE ORDEN)

  Notas de reconstruccion:
  - Totales de venta/compra = suma del detalle activo (VENDET_TOTAL / COMDET_TOTAL).
  - Ventas por metodo de pago = VENPA_VALOR_RECIBE - VENPA_VALOR_CAMBIO (neto de vueltos).
  - Periodo anterior = mismo numero de dias inmediatamente antes de @FECHA_INICIO.
  - Proyeccion: siempre sobre el mes calendario actual (hasta hoy), independiente del rango.
  - No calcula utilidad ni margen (fuera de alcance, ver README).
*/
CREATE OR ALTER PROCEDURE dbo.UspDashboardResumenFinanciero
    @NEGOCIO_ID   INT,
    @FECHA_INICIO DATE = NULL,
    @FECHA_FIN    DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @NEGOCIO_ID IS NULL OR @NEGOCIO_ID <= 0
        THROW 50001, 'El negocio es obligatorio.', 1;

    DECLARE @Hoy DATE = CAST(GETDATE() AS DATE);
    SET @FECHA_FIN    = ISNULL(@FECHA_FIN, @Hoy);
    SET @FECHA_INICIO = ISNULL(@FECHA_INICIO, DATEFROMPARTS(YEAR(@FECHA_FIN), MONTH(@FECHA_FIN), 1));

    IF @FECHA_INICIO > @FECHA_FIN
        THROW 50002, 'La fecha inicial no puede ser mayor que la fecha final.', 1;
    IF DATEDIFF(DAY, @FECHA_INICIO, @FECHA_FIN) > 366
        THROW 50003, 'El rango de fechas no puede superar 366 dias.', 1;

    DECLARE @Dias      INT  = DATEDIFF(DAY, @FECHA_INICIO, @FECHA_FIN) + 1;
    DECLARE @PrevIni   DATE = DATEADD(DAY, -@Dias, @FECHA_INICIO);
    DECLARE @PrevFin   DATE = DATEADD(DAY, -1, @FECHA_INICIO);
    DECLARE @MesIni    DATE = DATEFROMPARTS(YEAR(@Hoy), MONTH(@Hoy), 1);
    DECLARE @Desde     DATE = CASE WHEN @PrevIni < @MesIni THEN @PrevIni ELSE @MesIni END;
    DECLARE @Hasta     DATE = CASE WHEN @FECHA_FIN > @Hoy THEN @FECHA_FIN ELSE @Hoy END;

    -- Ventas y compras activas con su total (una pasada; cubre periodo, periodo anterior y mes actual)
    SELECT v.VENTA_ID AS Id, v.VENTA_FECHA AS Fecha,
           CAST(ISNULL(SUM(d.VENDET_TOTAL), 0) AS DECIMAL(18,2)) AS Total
    INTO #Ventas
    FROM dbo.TBL_VENTAS v
    LEFT JOIN dbo.TBL_VENTA_DETALLE d ON d.VENTDE_VENTA_ID = v.VENTA_ID AND d.VENDET_ESTADO = 'A'
    WHERE v.VENTA_NEG_ID = @NEGOCIO_ID AND v.VENTA_ESTADO = 'A'
      AND v.VENTA_FECHA >= @Desde AND v.VENTA_FECHA < DATEADD(DAY, 1, @Hasta)
    GROUP BY v.VENTA_ID, v.VENTA_FECHA;

    SELECT c.COMPRA_ID AS Id, c.COMPRA_FECHA AS Fecha,
           CAST(ISNULL(SUM(d.COMDET_TOTAL), 0) AS DECIMAL(18,2)) AS Total
    INTO #Compras
    FROM dbo.TBL_COMPRAS c
    LEFT JOIN dbo.TBL_COMPRA_DETALLE d ON d.COMDET_COMPRA_ID = c.COMPRA_ID AND d.COMDET_ESTADO = 'A'
    WHERE c.COMPRA_NEG_ID = @NEGOCIO_ID AND c.COMPRA_ESTADO = 'A'
      AND c.COMPRA_FECHA >= @Desde AND c.COMPRA_FECHA < DATEADD(DAY, 1, @Hasta)
    GROUP BY c.COMPRA_ID, c.COMPRA_FECHA;

    DECLARE @VentasPer DECIMAL(18,2), @VentasPrev DECIMAL(18,2), @ComprasPer DECIMAL(18,2), @ComprasPrev DECIMAL(18,2);
    SELECT @VentasPer = ISNULL(SUM(CASE WHEN Fecha >= @FECHA_INICIO AND Fecha < DATEADD(DAY, 1, @FECHA_FIN) THEN Total END), 0),
           @VentasPrev = ISNULL(SUM(CASE WHEN Fecha >= @PrevIni AND Fecha < DATEADD(DAY, 1, @PrevFin) THEN Total END), 0)
    FROM #Ventas;
    SELECT @ComprasPer = ISNULL(SUM(CASE WHEN Fecha >= @FECHA_INICIO AND Fecha < DATEADD(DAY, 1, @FECHA_FIN) THEN Total END), 0),
           @ComprasPrev = ISNULL(SUM(CASE WHEN Fecha >= @PrevIni AND Fecha < DATEADD(DAY, 1, @PrevFin) THEN Total END), 0)
    FROM #Compras;

    -- 1. Monthly sales (periodo vs periodo anterior)
    SELECT @VentasPer AS Total,
           CAST(CASE WHEN @VentasPrev = 0 THEN CASE WHEN @VentasPer > 0 THEN 100 ELSE 0 END
                     ELSE (@VentasPer - @VentasPrev) * 100.0 / @VentasPrev END AS DECIMAL(18,2)) AS PercentageChange,
           CAST('vs periodo anterior' AS VARCHAR(30)) AS ComparisonLabel;

    -- 2. Monthly purchases
    SELECT @ComprasPer AS Total,
           CAST(CASE WHEN @ComprasPrev = 0 THEN CASE WHEN @ComprasPer > 0 THEN 100 ELSE 0 END
                     ELSE (@ComprasPer - @ComprasPrev) * 100.0 / @ComprasPrev END AS DECIMAL(18,2)) AS PercentageChange,
           CAST('vs periodo anterior' AS VARCHAR(30)) AS ComparisonLabel;

    -- 3. Sales vs purchases (por bloques de 7 dias desde @FECHA_INICIO)
    ;WITH Semanas AS (
        SELECT 0 AS N
        UNION ALL
        SELECT N + 1 FROM Semanas WHERE N < (@Dias - 1) / 7
    ),
    Rango AS (
        SELECT N,
               DATEADD(DAY, N * 7, @FECHA_INICIO) AS PeriodStart,
               CASE WHEN DATEADD(DAY, N * 7 + 6, @FECHA_INICIO) > @FECHA_FIN
                    THEN @FECHA_FIN ELSE DATEADD(DAY, N * 7 + 6, @FECHA_INICIO) END AS PeriodEnd
        FROM Semanas
    )
    SELECT 'Semana ' + CAST(r.N + 1 AS VARCHAR(3)) AS Label,
           r.PeriodStart, r.PeriodEnd,
           CAST(ISNULL((SELECT SUM(v.Total) FROM #Ventas v
                        WHERE v.Fecha >= r.PeriodStart AND v.Fecha < DATEADD(DAY, 1, r.PeriodEnd)), 0) AS DECIMAL(18,2)) AS SalesTotal,
           CAST(ISNULL((SELECT SUM(c.Total) FROM #Compras c
                        WHERE c.Fecha >= r.PeriodStart AND c.Fecha < DATEADD(DAY, 1, r.PeriodEnd)), 0) AS DECIMAL(18,2)) AS PurchaseTotal,
           CAST(ISNULL((SELECT SUM(v.Total) FROM #Ventas v
                        WHERE v.Fecha >= r.PeriodStart AND v.Fecha < DATEADD(DAY, 1, r.PeriodEnd)), 0)
              - ISNULL((SELECT SUM(c.Total) FROM #Compras c
                        WHERE c.Fecha >= r.PeriodStart AND c.Fecha < DATEADD(DAY, 1, r.PeriodEnd)), 0) AS DECIMAL(18,2)) AS Balance
    FROM Rango r
    ORDER BY r.N
    OPTION (MAXRECURSION 100);

    -- 4. Operational balance (ventas - compras, bruto)
    SELECT @VentasPer AS SalesTotal, @ComprasPer AS PurchaseTotal,
           CAST(@VentasPer - @ComprasPer AS DECIMAL(18,2)) AS Balance;

    -- 5. Monthly projection (mes calendario actual, proyeccion lineal simple)
    DECLARE @VentasMes DECIMAL(18,2) = ISNULL((SELECT SUM(Total) FROM #Ventas
                                               WHERE Fecha >= @MesIni AND Fecha < DATEADD(DAY, 1, @Hoy)), 0);
    DECLARE @Transcurridos INT = DAY(@Hoy), @DiasMes INT = DAY(EOMONTH(@Hoy));
    SELECT @VentasMes AS CurrentMonthSales,
           @Transcurridos AS ElapsedDays,
           @DiasMes AS DaysInMonth,
           CAST(@VentasMes / @Transcurridos AS DECIMAL(18,2)) AS DailyAverage,
           CAST(@VentasMes / @Transcurridos * @DiasMes AS DECIMAL(18,2)) AS ProjectedMonthSales;

    -- 6. Sales by payment method (periodo, neto de vueltos)
    ;WITH Pagos AS (
        SELECT ISNULL(p.VENPA_MEDIOPAGO_ID, 0) AS PaymentMethodId,
               ISNULL(m.PARAM_DESCRIPCION, N'Sin m' + NCHAR(233) + N'todo') AS PaymentMethod,
               SUM(p.VENPA_VALOR_RECIBE - ISNULL(p.VENPA_VALOR_CAMBIO, 0)) AS Total
        FROM #Ventas v
        JOIN dbo.TBL_VENTA_PAGO p ON p.VENPA_VENTA_ID = v.Id AND p.VENPA_ESTADO = 'A'
        LEFT JOIN dbo.TBL_PARAMETROS m ON m.PARAM_ID = p.VENPA_MEDIOPAGO_ID
        WHERE v.Fecha >= @FECHA_INICIO AND v.Fecha < DATEADD(DAY, 1, @FECHA_FIN)
        GROUP BY ISNULL(p.VENPA_MEDIOPAGO_ID, 0), ISNULL(m.PARAM_DESCRIPCION, N'Sin m' + NCHAR(233) + N'todo')
    )
    SELECT PaymentMethodId, PaymentMethod,
           CAST(Total AS DECIMAL(18,2)) AS Total,
           CAST(Total * 100.0 / NULLIF(SUM(Total) OVER (), 0) AS DECIMAL(18,2)) AS Percentage
    FROM Pagos
    ORDER BY Total DESC;
END
GO
