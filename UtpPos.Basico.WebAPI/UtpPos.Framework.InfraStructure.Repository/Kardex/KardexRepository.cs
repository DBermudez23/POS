using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.Kardex;
using UtpPos.Framework.Transversal.Interface;
using DomainKardex = UtpPos.Framework.Domain.Entity.Kardex.Kardex;
using DomainKardexFilter = UtpPos.Framework.Domain.Entity.Kardex.KardexFilter;

namespace UtpPos.Framework.InfraStructure.Repository.Kardex
{
    public sealed class KardexRepository : IKardexRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public KardexRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainKardex>> ListAsync(DomainKardexFilter filter)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@KRDX_NEGOCIO_ID", filter.BusinessId, DbType.Int32);
            parameters.Add("@KRDX_INVENTARIO_ID", filter.InventoryId, DbType.Int32);
            parameters.Add("@KRDX_PROD_ID", filter.ProductId, DbType.Int32);
            parameters.Add("@KRDX_TIPO_MOV_ID", filter.MovementTypeId, DbType.Int32);
            parameters.Add("@KRDX_ESTADO", EmptyToNull(filter.Status), DbType.String);
            parameters.Add("@FECHA_INICIAL", filter.StartDate, DbType.DateTime);
            parameters.Add("@FECHA_FINAL", filter.EndDate, DbType.DateTime);

            var results = await connection.QueryAsync<KardexDbModel>(
                "dbo.UspListaKardex",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapToDomain).ToList();
        }

        public async Task<DomainKardex?> GetByIdAsync(int kardexId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@KRDX_ID", kardexId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<KardexDbModel>(
                "dbo.UspConsultaKardex",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapToDomain(result);
        }

        private static DomainKardex MapToDomain(KardexDbModel model)
        {
            return new DomainKardex
            {
                Id = model.KRDX_ID,
                BusinessId = model.KRDX_NEGOCIO_ID,
                BusinessName = model.NEG_NOMBRE ?? string.Empty,
                InventoryId = model.KRDX_INVENTARIO_ID,
                ProductId = model.KRDX_PROD_ID,
                ProductName = model.PROD_NOMBRE ?? string.Empty,
                CreatedByUserId = model.KRDX_CREADO_POR_USUARIO_ID,
                CreatedByUserName = model.CREADO_POR_USUARIO ?? string.Empty,
                MovementTypeId = model.KRDX_TIPO_MOV_ID,
                MovementTypeName = model.TIPO_MOVIMIENTO ?? string.Empty,
                InputQuantity = model.KRDX_CANT_ENTRADA,
                OutputQuantity = model.KRDX_CANT_SALIDA,
                PreviousBalance = model.KRDX_SALDO_ANTERIOR,
                CurrentBalance = model.KRDX_SALDO_ACTUAL,
                UnitCost = model.KRDX_COSTO_UNIDAD,
                PreviousWeightedAverageCost = model.KRDX_COSTO_PONDERADO_ANT,
                CurrentWeightedAverageCost = model.KRDX_COSTO_PONDERADO_ACT,
                Total = model.KRDX_TOTAL,
                Observation = model.KRDX_OBSERVACION,
                Status = model.KRDX_ESTADO ?? string.Empty,
                CreatedAt = model.KRDX_FECHA_CREACION
            };
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class KardexDbModel
        {
            public int KRDX_ID { get; set; }
            public int KRDX_NEGOCIO_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public int KRDX_INVENTARIO_ID { get; set; }
            public int KRDX_PROD_ID { get; set; }
            public string? PROD_NOMBRE { get; set; }
            public int KRDX_CREADO_POR_USUARIO_ID { get; set; }
            public string? CREADO_POR_USUARIO { get; set; }
            public int KRDX_TIPO_MOV_ID { get; set; }
            public string? TIPO_MOVIMIENTO { get; set; }
            public decimal KRDX_CANT_ENTRADA { get; set; }
            public decimal KRDX_CANT_SALIDA { get; set; }
            public decimal KRDX_SALDO_ANTERIOR { get; set; }
            public decimal KRDX_SALDO_ACTUAL { get; set; }
            public decimal KRDX_COSTO_UNIDAD { get; set; }
            public decimal KRDX_COSTO_PONDERADO_ANT { get; set; }
            public decimal KRDX_COSTO_PONDERADO_ACT { get; set; }
            public decimal KRDX_TOTAL { get; set; }
            public string? KRDX_OBSERVACION { get; set; }
            public string? KRDX_ESTADO { get; set; }
            public DateTime KRDX_FECHA_CREACION { get; set; }
        }
    }
}
