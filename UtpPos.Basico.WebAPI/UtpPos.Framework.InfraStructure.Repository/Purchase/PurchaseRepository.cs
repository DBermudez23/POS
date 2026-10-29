using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.Purchase;
using UtpPos.Framework.Transversal.Interface;
using DomainPurchase = UtpPos.Framework.Domain.Entity.Purchase.Purchase;
using DomainPurchaseCancel = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCancel;
using DomainPurchaseCancelResult = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCancelResult;
using DomainPurchaseCreate = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreate;
using DomainPurchaseCreateDetail = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreateDetail;
using DomainPurchaseCreatePayment = UtpPos.Framework.Domain.Entity.Purchase.PurchaseCreatePayment;
using DomainPurchaseDetail = UtpPos.Framework.Domain.Entity.Purchase.PurchaseDetail;
using DomainPurchaseFilter = UtpPos.Framework.Domain.Entity.Purchase.PurchaseFilter;
using DomainPurchasePayment = UtpPos.Framework.Domain.Entity.Purchase.PurchasePayment;
using DomainPurchaseWriteResult = UtpPos.Framework.Domain.Entity.Purchase.PurchaseWriteResult;

namespace UtpPos.Framework.InfraStructure.Repository.Purchase
{
    public sealed class PurchaseRepository : IPurchaseRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public PurchaseRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainPurchase>> ListAsync(DomainPurchaseFilter filter)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@COMPRA_NEG_ID", filter.BusinessId, DbType.Int32);
            parameters.Add("@COMPRA_TER_ID", filter.ThirdPartyId, DbType.Int32);
            parameters.Add("@COMPRA_USR_ID", filter.UserId, DbType.Int32);
            parameters.Add("@COMPRA_ESTADO", EmptyToNull(filter.Status), DbType.String);
            parameters.Add("@FECHA_INICIAL", filter.StartDate, DbType.DateTime);
            parameters.Add("@FECHA_FINAL", filter.EndDate, DbType.DateTime);

            var results = await connection.QueryAsync<PurchaseHeaderDbModel>(
                "dbo.UspListaCompras",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapHeaderToDomain).ToList();
        }

        public async Task<DomainPurchase?> GetByIdAsync(int purchaseId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@COMPRA_ID", purchaseId, DbType.Int32);

            using var result = await connection.QueryMultipleAsync(
                "dbo.UspConsultaCompra",
                parameters,
                commandType: CommandType.StoredProcedure);

            var header = await result.ReadFirstOrDefaultAsync<PurchaseHeaderDbModel>();
            if (header is null)
                return null;

            var details = (await result.ReadAsync<PurchaseDetailDbModel>()).Select(MapDetailToDomain).ToArray();
            var payments = (await result.ReadAsync<PurchasePaymentDbModel>()).Select(MapPaymentToDomain).ToArray();

            var purchase = MapHeaderToDomain(header);
            purchase.Details = details;
            purchase.Payments = payments;
            return purchase;
        }

        public async Task<DomainPurchaseWriteResult> CreateAsync(DomainPurchaseCreate purchase)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@COMPRA_NEG_ID", purchase.BusinessId, DbType.Int32);
            parameters.Add("@COMPRA_TER_ID", purchase.ThirdPartyId, DbType.Int32);
            parameters.Add("@COMPRA_USR_ID", purchase.UserId, DbType.Int32);
            parameters.Add("@KRDX_TIPO_MOV_ID_COMPRA", purchase.KardexMovementTypeId, DbType.Int32);
            parameters.Add("@DETALLES", BuildDetailsTable(purchase.Details).AsTableValuedParameter("dbo.CompraDetalleType"));
            parameters.Add("@PAGOS", BuildPaymentsTable(purchase.Payments).AsTableValuedParameter("dbo.CompraPagoType"));

            var result = await connection.QueryFirstOrDefaultAsync<PurchaseWriteDbResult>(
                "dbo.UspRegistraCompra",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureWriteResult(result);
        }

        public async Task<DomainPurchaseCancelResult> CancelAsync(DomainPurchaseCancel cancellation)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@COMPRA_ID", cancellation.PurchaseId, DbType.Int32);
            parameters.Add("@COMPRA_USR_ID", cancellation.UserId, DbType.Int32);
            parameters.Add("@KRDX_TIPO_MOV_ID_ANULACION", cancellation.CancellationKardexMovementTypeId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<PurchaseCancelDbResult>(
                "dbo.UspAnulaCompra",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureCancelResult(result);
        }

        private static DataTable BuildDetailsTable(IEnumerable<DomainPurchaseCreateDetail> details)
        {
            var table = new DataTable();
            table.Columns.Add("COMDET_PROD_ID", typeof(int));
            table.Columns.Add("COMDET_CANTIDAD", typeof(decimal));
            table.Columns.Add("COMDET_VALOR_UNIDAD", typeof(decimal));
            table.Columns.Add("COMDET_PORCENTAJE_IVA", typeof(decimal));

            foreach (var detail in details)
            {
                table.Rows.Add(detail.ProductId, detail.Quantity, detail.UnitValue, detail.VatPercentage);
            }

            return table;
        }

        private static DataTable BuildPaymentsTable(IEnumerable<DomainPurchaseCreatePayment> payments)
        {
            var table = new DataTable();
            table.Columns.Add("COMPA_MEDIOPAGO_ID", typeof(int));
            table.Columns.Add("COMPA_VALOR_RECIBE", typeof(decimal));
            table.Columns.Add("COMPA_VALOR_CAMBIO", typeof(decimal));

            foreach (var payment in payments)
            {
                table.Rows.Add(payment.PaymentMethodId, payment.ReceivedValue, payment.ChangeValue);
            }

            return table;
        }

        private static DomainPurchase MapHeaderToDomain(PurchaseHeaderDbModel model)
        {
            return new DomainPurchase
            {
                Id = model.COMPRA_ID,
                BusinessId = model.COMPRA_NEG_ID,
                BusinessName = model.NEG_NOMBRE ?? string.Empty,
                ThirdPartyId = model.COMPRA_TER_ID,
                ThirdPartyName = model.TER_NOMBRE ?? string.Empty,
                ThirdPartyDocumentNumber = model.TER_NUMERO_DOCUMENTO ?? string.Empty,
                UserId = model.COMPRA_USR_ID,
                UserName = model.USUARIO_COMPRA ?? string.Empty,
                Status = model.COMPRA_ESTADO ?? string.Empty,
                Date = model.COMPRA_FECHA,
                Subtotal = model.COMPRA_SUBTOTAL,
                VatValue = model.COMPRA_VALOR_IVA,
                Total = model.COMPRA_TOTAL,
                TotalPaid = model.COMPRA_TOTAL_PAGADO
            };
        }

        private static DomainPurchaseDetail MapDetailToDomain(PurchaseDetailDbModel model)
        {
            return new DomainPurchaseDetail
            {
                Id = model.COMDET_ID,
                PurchaseId = model.COMDET_COMPRA_ID,
                ProductId = model.COMDET_PROD_ID,
                ProductName = model.PROD_NOMBRE ?? string.Empty,
                ProductExternalId = model.PROD_EXTERNAL_ID,
                Quantity = model.COMDET_CANTIDAD,
                UnitValue = model.COMDET_VALOR_UNIDAD,
                Subtotal = model.COMDET_SUBTOTAL,
                VatValue = model.COMDET_VALOR_IVA,
                Total = model.COMDET_TOTAL,
                VatPercentage = model.COMDET_PORCENTAJE_IVA,
                Status = model.COMDET_ESTADO ?? string.Empty
            };
        }

        private static DomainPurchasePayment MapPaymentToDomain(PurchasePaymentDbModel model)
        {
            return new DomainPurchasePayment
            {
                Id = model.COMPA_ID,
                PurchaseId = model.COMPA_COMPRA_ID,
                PaymentMethodId = model.COMPA_MEDIOPAGO_ID,
                PaymentMethodName = model.MEDIO_PAGO ?? model.MEDIOPAGO ?? model.PARAM_DESCRIPCION ?? string.Empty,
                ReceivedValue = model.COMPA_VALOR_RECIBE,
                ChangeValue = model.COMPA_VALOR_CAMBIO,
                Status = model.COMPA_ESTADO ?? string.Empty
            };
        }

        private static DomainPurchaseWriteResult EnsureWriteResult(PurchaseWriteDbResult? result)
        {
            if (result is null)
                throw new InvalidOperationException("El stored procedure no retornó un resultado para la compra.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? "El stored procedure reportó una operación fallida al registrar la compra.");

            return new DomainPurchaseWriteResult
            {
                PurchaseId = result.COMPRA_ID
                    ?? throw new InvalidOperationException("El stored procedure no retornó COMPRA_ID al registrar la compra."),
                Subtotal = result.COMPRA_SUBTOTAL ?? 0,
                VatValue = result.COMPRA_VALOR_IVA ?? 0,
                Total = result.COMPRA_TOTAL ?? 0,
                TotalPaid = result.COMPRA_TOTAL_PAGADO ?? 0,
                Message = result.Mensaje ?? string.Empty
            };
        }

        private static DomainPurchaseCancelResult EnsureCancelResult(PurchaseCancelDbResult? result)
        {
            if (result is null)
                throw new InvalidOperationException("El stored procedure no retornó un resultado para la anulación de compra.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? "El stored procedure reportó una operación fallida al anular la compra.");

            return new DomainPurchaseCancelResult
            {
                PurchaseId = result.COMPRA_ID
                    ?? throw new InvalidOperationException("El stored procedure no retornó COMPRA_ID al anular la compra."),
                Message = result.Mensaje ?? string.Empty
            };
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class PurchaseHeaderDbModel
        {
            public int COMPRA_ID { get; set; }
            public int COMPRA_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public int COMPRA_TER_ID { get; set; }
            public string? TER_NOMBRE { get; set; }
            public string? TER_NUMERO_DOCUMENTO { get; set; }
            public int COMPRA_USR_ID { get; set; }
            public string? USUARIO_COMPRA { get; set; }
            public string? COMPRA_ESTADO { get; set; }
            public DateTime COMPRA_FECHA { get; set; }
            public decimal COMPRA_SUBTOTAL { get; set; }
            public decimal COMPRA_VALOR_IVA { get; set; }
            public decimal COMPRA_TOTAL { get; set; }
            public decimal COMPRA_TOTAL_PAGADO { get; set; }
        }

        private sealed class PurchaseDetailDbModel
        {
            public int COMDET_ID { get; set; }
            public int COMDET_COMPRA_ID { get; set; }
            public int COMDET_PROD_ID { get; set; }
            public string? PROD_NOMBRE { get; set; }
            public string? PROD_EXTERNAL_ID { get; set; }
            public decimal COMDET_CANTIDAD { get; set; }
            public decimal COMDET_VALOR_UNIDAD { get; set; }
            public decimal COMDET_SUBTOTAL { get; set; }
            public decimal COMDET_VALOR_IVA { get; set; }
            public decimal COMDET_TOTAL { get; set; }
            public decimal COMDET_PORCENTAJE_IVA { get; set; }
            public string? COMDET_ESTADO { get; set; }
        }

        private sealed class PurchasePaymentDbModel
        {
            public int COMPA_ID { get; set; }
            public int COMPA_COMPRA_ID { get; set; }
            public int COMPA_MEDIOPAGO_ID { get; set; }
            public string? MEDIO_PAGO { get; set; }
            public string? MEDIOPAGO { get; set; }
            public string? PARAM_DESCRIPCION { get; set; }
            public decimal COMPA_VALOR_RECIBE { get; set; }
            public decimal COMPA_VALOR_CAMBIO { get; set; }
            public string? COMPA_ESTADO { get; set; }
        }

        private sealed class PurchaseWriteDbResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? COMPRA_ID { get; set; }
            public decimal? COMPRA_SUBTOTAL { get; set; }
            public decimal? COMPRA_VALOR_IVA { get; set; }
            public decimal? COMPRA_TOTAL { get; set; }
            public decimal? COMPRA_TOTAL_PAGADO { get; set; }
        }

        private sealed class PurchaseCancelDbResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? COMPRA_ID { get; set; }
        }
    }
}
