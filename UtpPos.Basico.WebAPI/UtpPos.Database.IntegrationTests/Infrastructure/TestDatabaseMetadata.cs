namespace UtpPos.Database.IntegrationTests.Infrastructure;

public static class TestDatabaseMetadata
{
    public static readonly string[] CriticalTables =
    {
        "TBL_TERCEROS",
        "TBL_NEGOCIO_TERCEROS",
        "TBL_PRODUCTOS",
        "TBL_COMPRAS",
        "TBL_COMPRA_DETALLE",
        "TBL_COMPRA_PAGO",
        "TBL_VENTAS",
        "TBL_VENTA_DETALLE",
        "TBL_VENTA_PAGO",
        "TBL_INVENTARIO",
        "TBL_KARDEX",
        "TBL_NEGOCIO",
        "TBL_USUARIOS",
        "TBL_PARAMETROS"
    };

    public static readonly string[] CriticalStoredProcedures =
    {
        "UspCreaTercero",
        "UspAsociaTerceroNegocio",
        "UspListaTercerosPorNegocio",
        "UspValidaTerceroNegocio",
        "UspInactivaTerceroNegocio",
        "UspCreaProducto",
        "UspActualizaProducto",
        "UspRegistraCompra",
        "UspRegistraVenta",
        "UspListaInventario",
        "UspListaKardex"
    };

    public static readonly string[] CriticalTableTypes =
    {
        "CompraDetalleType",
        "CompraPagoType",
        "VentaDetalleType",
        "VentaPagoType"
    };
}
