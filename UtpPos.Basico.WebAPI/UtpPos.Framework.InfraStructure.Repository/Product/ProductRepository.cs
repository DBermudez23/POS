using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.Product;
using UtpPos.Framework.Transversal.Interface;
using DomainProduct = UtpPos.Framework.Domain.Entity.Product.Product;
using DomainProductUpdate = UtpPos.Framework.Domain.Entity.Product.ProductUpdate;
using DomainProductFilter = UtpPos.Framework.Domain.Entity.Product.ProductFilter;

namespace UtpPos.Framework.InfraStructure.Repository.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public ProductRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainProduct>> ListAsync(DomainProductFilter filter)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PROD_NEG_ID", filter.BusinessId, DbType.Int32);
            parameters.Add("@PROD_CATEGORIA_ID", filter.CategoryId, DbType.Int32);
            parameters.Add("@PROD_ESTADO", EmptyToNull(filter.Status), DbType.String);
            parameters.Add("@PROD_NOMBRE", EmptyToNull(filter.Name), DbType.String);
            parameters.Add("@PROD_EXTERNAL_ID", EmptyToNull(filter.ExternalId), DbType.String);

            var results = await connection.QueryAsync<ProductDbModel>(
                "dbo.UspListaProductos",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapToDomain).ToList();
        }

        public async Task<DomainProduct?> GetByIdAsync(int productId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PROD_ID", productId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ProductDbModel>(
                "dbo.UspConsultaProducto",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapToDomain(result);
        }

        public async Task<int> CreateAsync(DomainProduct product)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PROD_NOMBRE", product.Name, DbType.String);
            parameters.Add("@PROD_DESCRIPCION", product.Description, DbType.String);
            parameters.Add("@PROD_CATEGORIA_ID", product.CategoryId, DbType.Int32);
            parameters.Add("@PROD_IMAGEN", EmptyToNull(product.Image), DbType.String);
            parameters.Add("@PROD_VALOR_VENTA", 0m, DbType.Decimal); // Technical default; recalculated by weighted cost and profit margin.
            parameters.Add("@PROD_PROCENTAJE_IVA", product.VatPercentage, DbType.Decimal);
            parameters.Add("@PROD_PORCENTAJE_MARGEN_GAN", product.ProfitMarginPercentage, DbType.Decimal);
            parameters.Add("@PROD_EXTERNAL_ID", EmptyToNull(product.ExternalId), DbType.String);
            parameters.Add("@PROD_NEG_ID", product.BusinessId, DbType.Int32);
            parameters.Add("@PROD_CREADO_POR_USUARIO_ID", product.CreatedByUserId, DbType.Int32);
            parameters.Add("@PROD_ESTADO", EmptyToNull(product.Status) ?? "A", DbType.String);
            var result = await connection.QueryFirstOrDefaultAsync<ProductWriteResult>(
                "dbo.UspCreaProducto",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureProductId(result, "crear");
        }

        public async Task<int> UpdateAsync(DomainProductUpdate product)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PROD_ID", product.Id, DbType.Int32);
            parameters.Add("@PROD_NOMBRE", EmptyToNull(product.Name), DbType.String);
            parameters.Add("@PROD_DESCRIPCION", EmptyToNull(product.Description), DbType.String);
            parameters.Add("@PROD_CATEGORIA_ID", product.CategoryId, DbType.Int32);
            parameters.Add("@PROD_IMAGEN", EmptyToNull(product.Image), DbType.String);
            parameters.Add("@PROD_VALOR_VENTA", 0m, DbType.Decimal); // Technical default; recalculated by weighted cost and profit margin.
            parameters.Add("@PROD_PROCENTAJE_IVA", product.VatPercentage, DbType.Decimal);
            parameters.Add("@PROD_PORCENTAJE_MARGEN_GAN", product.ProfitMarginPercentage, DbType.Decimal);
            parameters.Add("@PROD_EXTERNAL_ID", EmptyToNull(product.ExternalId), DbType.String);
            parameters.Add("@PROD_NEG_ID", product.BusinessId, DbType.Int32);
            parameters.Add("@PROD_ESTADO", EmptyToNull(product.Status), DbType.String);
            parameters.Add("@LIMPIAR_IMAGEN", product.ClearImage, DbType.Boolean);
            parameters.Add("@LIMPIAR_EXTERNAL_ID", product.ClearExternalId, DbType.Boolean);
            parameters.Add("@LIMPIAR_NEGOCIO", product.ClearBusiness, DbType.Boolean);

            var result = await connection.QueryFirstOrDefaultAsync<ProductWriteResult>(
                "dbo.UspActualizaProducto",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureProductId(result, "actualizar");
        }

        public async Task<int> DeleteAsync(int productId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PROD_ID", productId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ProductWriteResult>(
                "dbo.UspEliminaProducto",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureProductId(result, "inactivar");
        }

        private static DomainProduct MapToDomain(ProductDbModel model)
        {
            return new DomainProduct
            {
                Id = model.PROD_ID,
                Name = model.PROD_NOMBRE ?? string.Empty,
                Description = model.PROD_DESCRIPCION ?? string.Empty,
                CategoryId = model.PROD_CATEGORIA_ID,
                CategoryName = model.CATEGORIA,
                Image = model.PROD_IMAGEN,
                SaleValue = model.PROD_VALOR_VENTA,
                VatPercentage = model.PROD_PROCENTAJE_IVA,
                ProfitMarginPercentage = model.PROD_PORCENTAJE_MARGEN_GAN,
                ExternalId = model.PROD_EXTERNAL_ID,
                BusinessId = model.PROD_NEG_ID,
                BusinessName = model.NEG_NOMBRE,
                CreatedByUserId = model.PROD_CREADO_POR_USUARIO_ID,
                CreatedByUserName = model.CREADO_POR_USUARIO,
                Status = model.PROD_ESTADO ?? string.Empty,
                CreatedAt = model.PROD_FECHA_CREADO
            };
        }

        private static int EnsureProductId(ProductWriteResult? result, string operation)
        {
            if (result is null)
                throw new InvalidOperationException(
                    $"El stored procedure no retornó un resultado de operación al {operation} producto.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(
                    result.Mensaje ?? $"El stored procedure reportó una operación fallida al {operation} producto.");

            return result.PROD_ID
                ?? throw new InvalidOperationException(
                    $"El stored procedure no retornó PROD_ID al {operation} producto.");
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class ProductDbModel
        {
            public int PROD_ID { get; set; }
            public string? PROD_NOMBRE { get; set; }
            public string? PROD_DESCRIPCION { get; set; }
            public int PROD_CATEGORIA_ID { get; set; }
            public string? CATEGORIA { get; set; }
            public string? PROD_IMAGEN { get; set; }
            public decimal? PROD_VALOR_VENTA { get; set; }
            public decimal PROD_PROCENTAJE_IVA { get; set; }
            public decimal PROD_PORCENTAJE_MARGEN_GAN { get; set; }
            public string? PROD_EXTERNAL_ID { get; set; }
            public int? PROD_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public int PROD_CREADO_POR_USUARIO_ID { get; set; }
            public string? CREADO_POR_USUARIO { get; set; }
            public string? PROD_ESTADO { get; set; }
            public DateTime PROD_FECHA_CREADO { get; set; }
        }

        private sealed class ProductWriteResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? PROD_ID { get; set; }
        }
    }
}
