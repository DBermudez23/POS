using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.Inventory;
using UtpPos.Framework.Transversal.Interface;
using DomainInventory = UtpPos.Framework.Domain.Entity.Inventory.Inventory;
using DomainInventoryFilter = UtpPos.Framework.Domain.Entity.Inventory.InventoryFilter;
using DomainInventoryMovement = UtpPos.Framework.Domain.Entity.Inventory.InventoryMovement;
using DomainInventoryMovementResult = UtpPos.Framework.Domain.Entity.Inventory.InventoryMovementResult;

namespace UtpPos.Framework.InfraStructure.Repository.Inventory
{
    public sealed class InventoryRepository : IInventoryRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public InventoryRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainInventory>> ListAsync(DomainInventoryFilter filter)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@INV_NEGOCIO_ID", filter.BusinessId, DbType.Int32);
            parameters.Add("@INV_PROD_ID", filter.ProductId, DbType.Int32);
            parameters.Add("@INV_ESTADO", EmptyToNull(filter.Status), DbType.String);
            parameters.Add("@PROD_NOMBRE", EmptyToNull(filter.ProductName), DbType.String);

            var results = await connection.QueryAsync<InventoryDbModel>(
                "dbo.UspListaInventario",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapToDomain).ToList();
        }

        public async Task<DomainInventory?> GetByIdAsync(int inventoryId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@INV_ID", inventoryId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<InventoryDbModel>(
                "dbo.UspConsultaInventario",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapToDomain(result);
        }

        public async Task<DomainInventoryMovementResult> RegisterMovementAsync(DomainInventoryMovement movement)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@KRDX_NEGOCIO_ID", movement.BusinessId, DbType.Int32);
            parameters.Add("@KRDX_PROD_ID", movement.ProductId, DbType.Int32);
            parameters.Add("@KRDX_TIPO_MOV_ID", movement.MovementTypeId, DbType.Int32);
            parameters.Add("@TIPO_OPERACION", movement.OperationType, DbType.String);
            parameters.Add("@CANTIDAD", movement.Quantity, DbType.Decimal);
            parameters.Add("@COSTO_UNIDAD", movement.UnitCost, DbType.Decimal);
            parameters.Add("@KRDX_OBSERVACION", EmptyToNull(movement.Observation), DbType.String);
            parameters.Add("@KRDX_CREADO_POR_USUARIO_ID", movement.CreatedByUserId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<InventoryMovementDbResult>(
                "dbo.UspRegistraMovimientoKardex",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureMovementResult(result);
        }

        private static DomainInventory MapToDomain(InventoryDbModel model)
        {
            return new DomainInventory
            {
                Id = model.INV_ID,
                BusinessId = model.INV_NEGOCIO_ID,
                BusinessName = model.NEG_NOMBRE ?? string.Empty,
                ProductId = model.INV_PROD_ID,
                ProductName = model.PROD_NOMBRE ?? string.Empty,
                ProductDescription = model.PROD_DESCRIPCION ?? string.Empty,
                ProductCategoryId = model.PROD_CATEGORIA_ID,
                CategoryName = model.CATEGORIA ?? string.Empty,
                CurrentStock = model.INV_STOCK_ACTUAL,
                MinimumStock = model.INV_STOCK_MINIMO,
                MaximumStock = model.INV_STOCK_MAX,
                WeightedAverageCost = model.INV_COSTO_PONDERADO,
                LastCost = model.INV_ULTIMO_COSTO,
                Status = model.INV_ESTADO ?? string.Empty,
                UpdatedAt = model.INV_FECHA_ACTUALIZACION,
                ModifiedByUserId = model.INV_MODIFICADO_POR_USUARIO_ID,
                ModifiedByUserName = model.MODIFICADO_POR_USUARIO,
                CreatedByUserId = model.INV_CREADO_POR_USUARIO_ID,
                CreatedByUserName = model.CREADO_POR_USUARIO ?? string.Empty,
                CreatedAt = model.INV_FECHA_CREADO
            };
        }

        private static DomainInventoryMovementResult EnsureMovementResult(InventoryMovementDbResult? result)
        {
            if (result is null)
                throw new InvalidOperationException("El stored procedure no retornó un resultado para el movimiento de inventario.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? "El stored procedure reportó una operación fallida al registrar el movimiento de inventario.");

            return new DomainInventoryMovementResult
            {
                KardexId = result.KRDX_ID
                    ?? throw new InvalidOperationException("El stored procedure no retornó KRDX_ID al registrar el movimiento de inventario."),
                InventoryId = result.INV_ID
                    ?? throw new InvalidOperationException("El stored procedure no retornó INV_ID al registrar el movimiento de inventario."),
                PreviousBalance = result.SALDO_ANTERIOR ?? 0,
                CurrentBalance = result.SALDO_ACTUAL ?? 0,
                PreviousWeightedAverageCost = result.COSTO_PONDERADO_ANTERIOR ?? 0,
                CurrentWeightedAverageCost = result.COSTO_PONDERADO_ACTUAL ?? 0,
                TotalMovement = result.TOTAL_MOVIMIENTO ?? 0,
                Message = result.Mensaje ?? string.Empty
            };
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class InventoryDbModel
        {
            public int INV_ID { get; set; }
            public int INV_NEGOCIO_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public int INV_PROD_ID { get; set; }
            public string? PROD_NOMBRE { get; set; }
            public string? PROD_DESCRIPCION { get; set; }
            public int PROD_CATEGORIA_ID { get; set; }
            public string? CATEGORIA { get; set; }
            public decimal INV_STOCK_ACTUAL { get; set; }
            public decimal INV_STOCK_MINIMO { get; set; }
            public decimal INV_STOCK_MAX { get; set; }
            public decimal INV_COSTO_PONDERADO { get; set; }
            public decimal INV_ULTIMO_COSTO { get; set; }
            public string? INV_ESTADO { get; set; }
            public DateTime? INV_FECHA_ACTUALIZACION { get; set; }
            public int? INV_MODIFICADO_POR_USUARIO_ID { get; set; }
            public string? MODIFICADO_POR_USUARIO { get; set; }
            public int INV_CREADO_POR_USUARIO_ID { get; set; }
            public string? CREADO_POR_USUARIO { get; set; }
            public DateTime INV_FECHA_CREADO { get; set; }
        }

        private sealed class InventoryMovementDbResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? KRDX_ID { get; set; }
            public int? INV_ID { get; set; }
            public decimal? SALDO_ANTERIOR { get; set; }
            public decimal? SALDO_ACTUAL { get; set; }
            public decimal? COSTO_PONDERADO_ANTERIOR { get; set; }
            public decimal? COSTO_PONDERADO_ACTUAL { get; set; }
            public decimal? TOTAL_MOVIMIENTO { get; set; }
        }
    }
}
