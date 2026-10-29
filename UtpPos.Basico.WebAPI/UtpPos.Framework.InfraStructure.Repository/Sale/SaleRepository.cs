using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.Sale;
using UtpPos.Framework.Transversal.Interface;
using DomainSale = UtpPos.Framework.Domain.Entity.Sale.Sale;
using DomainSaleCancel = UtpPos.Framework.Domain.Entity.Sale.SaleCancel;
using DomainSaleCancelResult = UtpPos.Framework.Domain.Entity.Sale.SaleCancelResult;
using DomainSaleCreate = UtpPos.Framework.Domain.Entity.Sale.SaleCreate;
using DomainSaleCreateDetail = UtpPos.Framework.Domain.Entity.Sale.SaleCreateDetail;
using DomainSaleCreatePayment = UtpPos.Framework.Domain.Entity.Sale.SaleCreatePayment;
using DomainSaleDetail = UtpPos.Framework.Domain.Entity.Sale.SaleDetail;
using DomainSaleFilter = UtpPos.Framework.Domain.Entity.Sale.SaleFilter;
using DomainSalePayment = UtpPos.Framework.Domain.Entity.Sale.SalePayment;
using DomainSaleWriteResult = UtpPos.Framework.Domain.Entity.Sale.SaleWriteResult;

namespace UtpPos.Framework.InfraStructure.Repository.Sale
{
    public sealed class SaleRepository : ISaleRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public SaleRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainSale>> ListAsync(DomainSaleFilter filter)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@VENTA_NEG_ID", filter.BusinessId, DbType.Int32);
            parameters.Add("@VENTA_TER_ID", filter.ThirdPartyId, DbType.Int32);
            parameters.Add("@VENTA_USR_ID", filter.UserId, DbType.Int32);
            parameters.Add("@VENTA_ESTADO", EmptyToNull(filter.Status), DbType.String);
            parameters.Add("@FECHA_INICIAL", filter.StartDate, DbType.DateTime);
            parameters.Add("@FECHA_FINAL", filter.EndDate, DbType.DateTime);

            var results = await connection.QueryAsync<SaleHeaderDbModel>(
                "dbo.UspListaVentas",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapHeaderToDomain).ToList();
        }

        public async Task<DomainSale?> GetByIdAsync(int saleId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@VENTA_ID", saleId, DbType.Int32);

            using var result = await connection.QueryMultipleAsync(
                "dbo.UspConsultaVenta",
                parameters,
                commandType: CommandType.StoredProcedure);

            var header = await result.ReadFirstOrDefaultAsync<SaleHeaderDbModel>();
            if (header is null)
                return null;

            var details = (await result.ReadAsync<SaleDetailDbModel>()).Select(MapDetailToDomain).ToArray();
            var payments = (await result.ReadAsync<SalePaymentDbModel>()).Select(MapPaymentToDomain).ToArray();

            var sale = MapHeaderToDomain(header);
            sale.Details = details;
            sale.Payments = payments;
            return sale;
        }

        public async Task<DomainSaleWriteResult> CreateAsync(DomainSaleCreate sale)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@VENTA_NEG_ID", sale.BusinessId, DbType.Int32);
            parameters.Add("@VENTA_TER_ID", sale.ThirdPartyId, DbType.Int32);
            parameters.Add("@VENTA_USR_ID", sale.UserId, DbType.Int32);
            parameters.Add("@KRDX_TIPO_MOV_ID_VENTA", sale.KardexMovementTypeId, DbType.Int32);
            parameters.Add("@DETALLES", BuildDetailsTable(sale.Details).AsTableValuedParameter("dbo.VentaDetalleType"));
            parameters.Add("@PAGOS", BuildPaymentsTable(sale.Payments).AsTableValuedParameter("dbo.VentaPagoType"));

            var result = await connection.QueryFirstOrDefaultAsync<SaleWriteDbResult>(
                "dbo.UspRegistraVenta",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureWriteResult(result);
        }

        public async Task<DomainSaleCancelResult> CancelAsync(DomainSaleCancel cancellation)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@VENTA_ID", cancellation.SaleId, DbType.Int32);
            parameters.Add("@VENTA_USR_ID", cancellation.UserId, DbType.Int32);
            parameters.Add("@KRDX_TIPO_MOV_ID_ANULACION", cancellation.CancellationKardexMovementTypeId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<SaleCancelDbResult>(
                "dbo.UspAnulaVenta",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureCancelResult(result);
        }

        private static DataTable BuildDetailsTable(IEnumerable<DomainSaleCreateDetail> details)
        {
            var table = new DataTable();
            table.Columns.Add("VENTDE_PROD_ID", typeof(int));
            table.Columns.Add("VENDET_CANTIDAD", typeof(decimal));
            table.Columns.Add("VENDET_VALOR_UNIDAD", typeof(decimal));
            table.Columns.Add("VENDET_PORCENTAJE_IVA", typeof(decimal));

            foreach (var detail in details)
            {
                table.Rows.Add(detail.ProductId, detail.Quantity, detail.UnitValue, detail.VatPercentage);
            }

            return table;
        }

        private static DataTable BuildPaymentsTable(IEnumerable<DomainSaleCreatePayment> payments)
        {
            var table = new DataTable();
            table.Columns.Add("VENPA_MEDIOPAGO_ID", typeof(int));
            table.Columns.Add("VENPA_VALOR_RECIBE", typeof(decimal));
            table.Columns.Add("VENPA_VALOR_CAMBIO", typeof(decimal));

            foreach (var payment in payments)
            {
                table.Rows.Add(payment.PaymentMethodId, payment.ReceivedValue, payment.ChangeValue);
            }

            return table;
        }

        private static DomainSale MapHeaderToDomain(SaleHeaderDbModel model)
        {
            return new DomainSale
            {
                Id = model.VENTA_ID,
                BusinessId = model.VENTA_NEG_ID,
                BusinessName = model.NEG_NOMBRE ?? string.Empty,
                ThirdPartyId = model.VENTA_TER_ID,
                ThirdPartyName = model.TER_NOMBRE ?? string.Empty,
                ThirdPartyDocumentNumber = model.TER_NUMERO_DOCUMENTO ?? string.Empty,
                UserId = model.VENTA_USR_ID,
                UserName = model.USUARIO_VENTA ?? string.Empty,
                Status = model.VENTA_ESTADO ?? string.Empty,
                Date = model.VENTA_FECHA,
                Subtotal = model.VENTA_SUBTOTAL,
                VatValue = model.VENTA_VALOR_IVA,
                Total = model.VENTA_TOTAL,
                CostTotal = model.VENTA_COSTO_TOTAL,
                GrossProfit = model.VENTA_UTILIDAD_BRUTA,
                TotalPaid = model.VENTA_TOTAL_PAGADO
            };
        }

        private static DomainSaleDetail MapDetailToDomain(SaleDetailDbModel model)
        {
            return new DomainSaleDetail
            {
                Id = model.VENTDE_ID,
                SaleId = model.VENTDE_VENTA_ID,
                ProductId = model.VENTDE_PROD_ID,
                ProductName = model.PROD_NOMBRE ?? string.Empty,
                ProductExternalId = model.PROD_EXTERNAL_ID,
                Quantity = model.VENDET_CANTIDAD,
                UnitValue = model.VENDET_VALOR_UNIDAD,
                Subtotal = model.VENDET_SUBTOTAL,
                VatValue = model.VENDET_VALOR_IVA,
                Total = model.VENDET_TOTAL,
                VatPercentage = model.VENDET_PORCENTAJE_IVA,
                UnitCost = model.VENDET_COSTO_UNIDAD,
                CostTotal = model.VENDET_COSTO_TOTAL,
                Status = model.VENDET_ESTADO ?? string.Empty
            };
        }

        private static DomainSalePayment MapPaymentToDomain(SalePaymentDbModel model)
        {
            return new DomainSalePayment
            {
                Id = model.VENPA_ID,
                SaleId = model.VENPA_VENTA_ID,
                PaymentMethodId = model.VENPA_MEDIOPAGO_ID,
                PaymentMethodName = model.MEDIO_PAGO ?? model.MEDIOPAGO ?? model.PARAM_DESCRIPCION ?? string.Empty,
                ReceivedValue = model.VENPA_VALOR_RECIBE,
                ChangeValue = model.VENPA_VALOR_CAMBIO,
                Status = model.VENPA_ESTADO ?? string.Empty
            };
        }

        private static DomainSaleWriteResult EnsureWriteResult(SaleWriteDbResult? result)
        {
            if (result is null)
                throw new InvalidOperationException("El stored procedure no retornó un resultado para la venta.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? "El stored procedure reportó una operación fallida al registrar la venta.");

            return new DomainSaleWriteResult
            {
                SaleId = result.VENTA_ID
                    ?? throw new InvalidOperationException("El stored procedure no retornó VENTA_ID al registrar la venta."),
                Subtotal = result.VENTA_SUBTOTAL ?? 0,
                VatValue = result.VENTA_VALOR_IVA ?? 0,
                Total = result.VENTA_TOTAL ?? 0,
                TotalPaid = result.VENTA_TOTAL_PAGADO ?? 0,
                Message = result.Mensaje ?? string.Empty
            };
        }

        private static DomainSaleCancelResult EnsureCancelResult(SaleCancelDbResult? result)
        {
            if (result is null)
                throw new InvalidOperationException("El stored procedure no retornó un resultado para la anulación de venta.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? "El stored procedure reportó una operación fallida al anular la venta.");

            return new DomainSaleCancelResult
            {
                SaleId = result.VENTA_ID
                    ?? throw new InvalidOperationException("El stored procedure no retornó VENTA_ID al anular la venta."),
                Message = result.Mensaje ?? string.Empty
            };
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class SaleHeaderDbModel
        {
            public int VENTA_ID { get; set; }
            public int VENTA_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public int VENTA_TER_ID { get; set; }
            public string? TER_NOMBRE { get; set; }
            public string? TER_NUMERO_DOCUMENTO { get; set; }
            public int VENTA_USR_ID { get; set; }
            public string? USUARIO_VENTA { get; set; }
            public string? VENTA_ESTADO { get; set; }
            public DateTime VENTA_FECHA { get; set; }
            public decimal VENTA_SUBTOTAL { get; set; }
            public decimal VENTA_VALOR_IVA { get; set; }
            public decimal VENTA_TOTAL { get; set; }
            public decimal VENTA_COSTO_TOTAL { get; set; }
            public decimal VENTA_UTILIDAD_BRUTA { get; set; }
            public decimal VENTA_TOTAL_PAGADO { get; set; }
        }

        private sealed class SaleDetailDbModel
        {
            public int VENTDE_ID { get; set; }
            public int VENTDE_VENTA_ID { get; set; }
            public int VENTDE_PROD_ID { get; set; }
            public string? PROD_NOMBRE { get; set; }
            public string? PROD_EXTERNAL_ID { get; set; }
            public decimal VENDET_CANTIDAD { get; set; }
            public decimal VENDET_VALOR_UNIDAD { get; set; }
            public decimal VENDET_SUBTOTAL { get; set; }
            public decimal VENDET_VALOR_IVA { get; set; }
            public decimal VENDET_TOTAL { get; set; }
            public decimal VENDET_PORCENTAJE_IVA { get; set; }
            public decimal VENDET_COSTO_UNIDAD { get; set; }
            public decimal VENDET_COSTO_TOTAL { get; set; }
            public string? VENDET_ESTADO { get; set; }
        }

        private sealed class SalePaymentDbModel
        {
            public int VENPA_ID { get; set; }
            public int VENPA_VENTA_ID { get; set; }
            public int VENPA_MEDIOPAGO_ID { get; set; }
            public string? MEDIO_PAGO { get; set; }
            public string? MEDIOPAGO { get; set; }
            public string? PARAM_DESCRIPCION { get; set; }
            public decimal VENPA_VALOR_RECIBE { get; set; }
            public decimal VENPA_VALOR_CAMBIO { get; set; }
            public string? VENPA_ESTADO { get; set; }
        }

        private sealed class SaleWriteDbResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? VENTA_ID { get; set; }
            public decimal? VENTA_SUBTOTAL { get; set; }
            public decimal? VENTA_VALOR_IVA { get; set; }
            public decimal? VENTA_TOTAL { get; set; }
            public decimal? VENTA_TOTAL_PAGADO { get; set; }
        }

        private sealed class SaleCancelDbResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? VENTA_ID { get; set; }
        }
    }
}
