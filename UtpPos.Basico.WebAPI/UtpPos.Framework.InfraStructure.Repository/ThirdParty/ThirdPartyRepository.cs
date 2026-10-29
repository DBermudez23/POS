using System.Data;
using Dapper;
using UtpPos.Framework.InfraStructure.Interface.ThirdParty;
using UtpPos.Framework.Transversal.Interface;
using DomainThirdParty = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdParty;
using DomainThirdPartyBusiness = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusiness;
using DomainThirdPartyBusinessAssociation = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessAssociation;
using DomainThirdPartyBusinessFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessFilter;
using DomainThirdPartyBusinessResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessResult;
using DomainThirdPartyCreate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreate;
using DomainThirdPartyFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyFilter;
using DomainThirdPartyResult = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyResult;
using DomainThirdPartyUpdate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyUpdate;

namespace UtpPos.Framework.InfraStructure.Repository.ThirdParty
{
    public sealed class ThirdPartyRepository : IThirdPartyRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public ThirdPartyRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<DomainThirdParty>> ListAsync(DomainThirdPartyFilter filter)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TER_TIPO_DOC_ID", filter.DocumentTypeId, DbType.Int32);
            parameters.Add("@TER_ESTADO", EmptyToNull(filter.Status), DbType.String);
            parameters.Add("@TER_NUMERO_DOCUMENTO", EmptyToNull(filter.DocumentNumber), DbType.String);
            parameters.Add("@TER_NOMBRE", EmptyToNull(filter.Name), DbType.String);
            parameters.Add("@TER_TELEFONO", EmptyToNull(filter.Phone), DbType.String);

            var results = await connection.QueryAsync<ThirdPartyDbModel>(
                "dbo.UspListaTerceros",
                parameters,
                commandType: CommandType.StoredProcedure);

            return results.Select(MapToDomain).ToList();
        }

        public async Task<DomainThirdParty?> GetByIdAsync(int thirdPartyId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TER_ID", thirdPartyId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ThirdPartyDbModel>(
                "dbo.UspConsultaTercero",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result is null ? null : MapToDomain(result);
        }

        public async Task<DomainThirdPartyResult> CreateAsync(DomainThirdPartyCreate thirdParty)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TER_TIPO_DOC_ID", thirdParty.DocumentTypeId, DbType.Int32);
            parameters.Add("@TER_NUMERO_DOCUMENTO", thirdParty.DocumentNumber, DbType.String);
            parameters.Add("@TER_NOMBRE", thirdParty.Name, DbType.String);
            parameters.Add("@TER_CIUDAD_ID", thirdParty.CityId, DbType.Int32);
            parameters.Add("@TER_DEPTO_ID", thirdParty.DepartmentId, DbType.Int32);
            parameters.Add("@TER_DIRECCION", EmptyToNull(thirdParty.Address), DbType.String);
            parameters.Add("@TER_TELEFONO", thirdParty.Phone, DbType.String);
            parameters.Add("@TER_CREADO_POR_USUARIO_ID", thirdParty.CreatedByUserId, DbType.Int32);
            parameters.Add("@TER_ESTADO", EmptyToNull(thirdParty.Status) ?? "A", DbType.String);
            parameters.Add("@TER_TIPO_TERCERO_ID", thirdParty.ThirdPartyTypeId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ThirdPartyWriteResult>(
                "dbo.UspCreaTercero",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureResult(result, "crear");
        }

        public async Task<DomainThirdPartyResult> UpdateAsync(DomainThirdPartyUpdate thirdParty)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TER_ID", thirdParty.Id, DbType.Int32);
            parameters.Add("@TER_TIPO_DOC_ID", thirdParty.DocumentTypeId, DbType.Int32);
            parameters.Add("@TER_NUMERO_DOCUMENTO", EmptyToNull(thirdParty.DocumentNumber), DbType.String);
            parameters.Add("@TER_NOMBRE", EmptyToNull(thirdParty.Name), DbType.String);
            parameters.Add("@TER_CIUDAD_ID", thirdParty.CityId, DbType.Int32);
            parameters.Add("@TER_DEPTO_ID", thirdParty.DepartmentId, DbType.Int32);
            parameters.Add("@TER_DIRECCION", EmptyToNull(thirdParty.Address), DbType.String);
            parameters.Add("@TER_TELEFONO", EmptyToNull(thirdParty.Phone), DbType.String);
            parameters.Add("@TER_ESTADO", EmptyToNull(thirdParty.Status), DbType.String);
            parameters.Add("@LIMPIAR_DIRECCION", thirdParty.ClearAddress, DbType.Boolean);
            parameters.Add("@TER_TIPO_TERCERO_ID", thirdParty.ThirdPartyTypeId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ThirdPartyWriteResult>(
                "dbo.UspActualizaTercero",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureResult(result, "actualizar");
        }

        public async Task<DomainThirdPartyResult> DeleteAsync(int thirdPartyId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TER_ID", thirdPartyId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ThirdPartyWriteResult>(
                "dbo.UspEliminaTercero",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureResult(result, "inactivar");
        }

        public async Task<IEnumerable<DomainThirdPartyBusiness>> ListByBusinessAsync(DomainThirdPartyBusinessFilter filter)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NEG_ID", filter.BusinessId, DbType.Int32);
            parameters.Add("@TER_ESTADO", EmptyToNull(filter.Status), DbType.String);
            parameters.Add("@TER_NUMERO_DOCUMENTO", EmptyToNull(filter.DocumentNumber), DbType.String);
            parameters.Add("@TER_NOMBRE", EmptyToNull(filter.Name), DbType.String);
            parameters.Add("@TER_TELEFONO", EmptyToNull(filter.Phone), DbType.String);

            var results = await connection.QueryAsync<ThirdPartyBusinessDbModel>(
                "dbo.UspListaTercerosPorNegocio",
                parameters,
                commandType: CommandType.StoredProcedure);

            var thirdParties = results.Select(MapBusinessToDomain);
            if (filter.DocumentTypeId.HasValue)
                thirdParties = thirdParties.Where(thirdParty => thirdParty.DocumentTypeId == filter.DocumentTypeId.Value);

            return thirdParties.ToList();
        }

        public async Task<DomainThirdPartyBusinessResult> AssociateBusinessAsync(DomainThirdPartyBusinessAssociation association)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NEGTER_NEG_ID", association.BusinessId, DbType.Int32);
            parameters.Add("@NEGTER_TER_ID", association.ThirdPartyId, DbType.Int32);
            parameters.Add("@NEGTER_CREADO_POR_USUARIO_ID", association.UserId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ThirdPartyBusinessWriteResult>(
                "dbo.UspAsociaTerceroNegocio",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureBusinessResult(result, association.ThirdPartyId, "asociar");
        }

        public async Task<DomainThirdPartyBusinessResult> InactivateBusinessAsync(DomainThirdPartyBusinessAssociation association)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NEGTER_NEG_ID", association.BusinessId, DbType.Int32);
            parameters.Add("@NEGTER_TER_ID", association.ThirdPartyId, DbType.Int32);
            parameters.Add("@NEGTER_MODIFICADO_POR_USUARIO_ID", association.UserId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ThirdPartyBusinessWriteResult>(
                "dbo.UspInactivaTerceroNegocio",
                parameters,
                commandType: CommandType.StoredProcedure);

            return EnsureBusinessResult(result, association.ThirdPartyId, "inactivar relación de");
        }

        public async Task<bool> ValidateBusinessAsync(int businessId, int thirdPartyId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@NEG_ID", businessId, DbType.Int32);
            parameters.Add("@TER_ID", thirdPartyId, DbType.Int32);

            var result = await connection.QueryFirstOrDefaultAsync<ThirdPartyBusinessValidationResult>(
                "dbo.UspValidaTerceroNegocio",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result?.ExisteRelacionActiva == 1;
        }

        private static DomainThirdParty MapToDomain(ThirdPartyDbModel model)
        {
            return new DomainThirdParty
            {
                Id = model.TER_ID,
                DocumentTypeId = model.TER_TIPO_DOC_ID,
                DocumentTypeName = model.TIPO_DOCUMENTO ?? string.Empty,
                DocumentNumber = model.TER_NUMERO_DOCUMENTO ?? string.Empty,
                Name = model.TER_NOMBRE ?? string.Empty,
                DepartmentId = model.TER_DEPTO_ID,
                DepartmentName = model.DEPARTAMENTO ?? string.Empty,
                CityId = model.TER_CIUDAD_ID,
                CityName = model.CIUDAD ?? string.Empty,
                Address = model.TER_DIRECCION,
                Phone = model.TER_TELEFONO ?? string.Empty,
                CreatedByUserId = model.TER_CREADO_POR_USUARIO_ID,
                CreatedByUserName = model.CREADO_POR_USUARIO ?? string.Empty,
                Status = model.TER_ESTADO ?? string.Empty,
                CreatedAt = model.TER_FECHA_CREADO,
                ThirdPartyTypeId = model.TER_TIPO_TERCERO_ID,
                ThirdPartyTypeName = model.TIPO_TERCERO ?? string.Empty
            };
        }

        private static DomainThirdPartyBusiness MapBusinessToDomain(ThirdPartyBusinessDbModel model)
        {
            return new DomainThirdPartyBusiness
            {
                Id = model.TER_ID,
                NegterId = model.NEGTER_ID,
                BusinessId = model.NEGTER_NEG_ID,
                BusinessName = model.NEG_NOMBRE ?? string.Empty,
                RelationStatus = model.NEGTER_ESTADO ?? string.Empty,
                DocumentTypeId = model.TER_TIPO_DOC_ID,
                DocumentTypeName = model.TIPO_DOCUMENTO ?? string.Empty,
                DocumentNumber = model.TER_NUMERO_DOCUMENTO ?? string.Empty,
                Name = model.TER_NOMBRE ?? string.Empty,
                DepartmentId = model.TER_DEPTO_ID,
                DepartmentName = model.DEPARTAMENTO ?? string.Empty,
                CityId = model.TER_CIUDAD_ID,
                CityName = model.CIUDAD ?? string.Empty,
                Address = model.TER_DIRECCION,
                Phone = model.TER_TELEFONO ?? string.Empty,
                CreatedByUserId = model.TER_CREADO_POR_USUARIO_ID,
                CreatedByUserName = model.CREADO_POR_USUARIO ?? string.Empty,
                Status = model.TER_ESTADO ?? string.Empty,
                CreatedAt = model.TER_FECHA_CREADO,
                ThirdPartyTypeId = model.TER_TIPO_TERCERO_ID,
                ThirdPartyTypeName = model.TIPO_TERCERO ?? string.Empty
            };
        }

        private static DomainThirdPartyResult EnsureResult(ThirdPartyWriteResult? result, string operation)
        {
            if (result is null)
                throw new InvalidOperationException($"El stored procedure no retornó un resultado al {operation} tercero.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? $"El stored procedure reportó una operación fallida al {operation} tercero.");

            return new DomainThirdPartyResult
            {
                ThirdPartyId = result.TER_ID
                    ?? throw new InvalidOperationException($"El stored procedure no retornó TER_ID al {operation} tercero."),
                Message = result.Mensaje ?? string.Empty
            };
        }

        private static DomainThirdPartyBusinessResult EnsureBusinessResult(ThirdPartyBusinessWriteResult? result, int fallbackThirdPartyId, string operation)
        {
            if (result is null)
                throw new InvalidOperationException($"El stored procedure no retornó un resultado al {operation} tercero a negocio.");

            if (result.Exitoso != 1)
                throw new InvalidOperationException(result.Mensaje ?? $"El stored procedure reportó una operación fallida al {operation} tercero a negocio.");

            return new DomainThirdPartyBusinessResult
            {
                ThirdPartyId = result.TER_ID ?? fallbackThirdPartyId,
                NegterId = result.NEGTER_ID,
                Message = result.Mensaje ?? string.Empty
            };
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class ThirdPartyDbModel
        {
            public int TER_ID { get; set; }
            public int TER_TIPO_DOC_ID { get; set; }
            public string? TIPO_DOCUMENTO { get; set; }
            public string? TER_NUMERO_DOCUMENTO { get; set; }
            public string? TER_NOMBRE { get; set; }
            public int TER_DEPTO_ID { get; set; }
            public string? DEPARTAMENTO { get; set; }
            public int TER_CIUDAD_ID { get; set; }
            public string? CIUDAD { get; set; }
            public string? TER_DIRECCION { get; set; }
            public string? TER_TELEFONO { get; set; }
            public int TER_CREADO_POR_USUARIO_ID { get; set; }
            public string? CREADO_POR_USUARIO { get; set; }
            public string? TER_ESTADO { get; set; }
            public DateTime TER_FECHA_CREADO { get; set; }
            public int TER_TIPO_TERCERO_ID { get; set; }
            public string? TIPO_TERCERO { get; set; }
        }

        private sealed class ThirdPartyBusinessDbModel
        {
            public int TER_ID { get; set; }
            public int? NEGTER_ID { get; set; }
            public int NEGTER_NEG_ID { get; set; }
            public string? NEG_NOMBRE { get; set; }
            public string? NEGTER_ESTADO { get; set; }
            public int TER_TIPO_DOC_ID { get; set; }
            public string? TIPO_DOCUMENTO { get; set; }
            public string? TER_NUMERO_DOCUMENTO { get; set; }
            public string? TER_NOMBRE { get; set; }
            public int TER_DEPTO_ID { get; set; }
            public string? DEPARTAMENTO { get; set; }
            public int TER_CIUDAD_ID { get; set; }
            public string? CIUDAD { get; set; }
            public string? TER_DIRECCION { get; set; }
            public string? TER_TELEFONO { get; set; }
            public int TER_CREADO_POR_USUARIO_ID { get; set; }
            public string? CREADO_POR_USUARIO { get; set; }
            public string? TER_ESTADO { get; set; }
            public DateTime TER_FECHA_CREADO { get; set; }
            public int TER_TIPO_TERCERO_ID { get; set; }
            public string? TIPO_TERCERO { get; set; }
        }

        private sealed class ThirdPartyWriteResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? TER_ID { get; set; }
        }

        private sealed class ThirdPartyBusinessWriteResult
        {
            public int Exitoso { get; set; }
            public string? Mensaje { get; set; }
            public int? TER_ID { get; set; }
            public int? NEGTER_ID { get; set; }
        }

        private sealed class ThirdPartyBusinessValidationResult
        {
            public int ExisteRelacionActiva { get; set; }
        }
    }
}
