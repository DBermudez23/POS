using AutoMapper;
using UtpPos.Framework.Application.DTO.Kardex;
using UtpPos.Framework.Application.Interface.Kardex;
using UtpPos.Framework.Domain.Interface.Kardex;
using UtpPos.Framework.Transversal.Common;
using DomainKardexFilter = UtpPos.Framework.Domain.Entity.Kardex.KardexFilter;

namespace UtpPos.Framework.Application.Main.Kardex
{
    public sealed class KardexApplication : IKardexApplication
    {
        private readonly IKardexDomain _domain;
        private readonly IMapper _mapper;

        public KardexApplication(IKardexDomain domain, IMapper mapper)
        {
            _domain = domain;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<KardexDto>>> ListAsync(KardexListFilterDto filter)
        {
            try
            {
                filter ??= new KardexListFilterDto();

                var kardex = await _domain.ListAsync(_mapper.Map<DomainKardexFilter>(filter));
                return Success(_mapper.Map<IEnumerable<KardexDto>>(kardex), "Kardex listado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<KardexDto>>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<KardexDto>>> ListByProductAsync(int productId)
        {
            try
            {
                var kardex = await _domain.ListAsync(new DomainKardexFilter
                {
                    ProductId = productId
                });

                return Success(_mapper.Map<IEnumerable<KardexDto>>(kardex), "Kardex del producto listado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<KardexDto>>(ex.Message);
            }
        }

        public async Task<Response<KardexDto>> GetByIdAsync(int kardexId)
        {
            try
            {
                var kardex = await _domain.GetByIdAsync(kardexId);

                if (kardex is null)
                    return Fail<KardexDto>("Movimiento de Kardex no encontrado.");

                return Success(_mapper.Map<KardexDto>(kardex), "Movimiento de Kardex encontrado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<KardexDto>(ex.Message);
            }
        }

        private static Response<T> Success<T>(T data, string message)
        {
            return new Response<T> { IsSuccess = true, Data = data, Message = message };
        }

        private static Response<T> Fail<T>(string message)
        {
            return new Response<T> { IsSuccess = false, Data = default, Message = message };
        }
    }
}
