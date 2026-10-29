using AutoMapper;
using UtpPos.Framework.Application.DTO.Sale;
using UtpPos.Framework.Application.Interface.Sale;
using UtpPos.Framework.Domain.Interface.Sale;
using UtpPos.Framework.Transversal.Common;
using DomainSaleCancel = UtpPos.Framework.Domain.Entity.Sale.SaleCancel;
using DomainSaleCreate = UtpPos.Framework.Domain.Entity.Sale.SaleCreate;
using DomainSaleFilter = UtpPos.Framework.Domain.Entity.Sale.SaleFilter;

namespace UtpPos.Framework.Application.Main.Sale
{
    public sealed class SaleApplication : ISaleApplication
    {
        private readonly ISaleDomain _domain;
        private readonly IMapper _mapper;

        public SaleApplication(ISaleDomain domain, IMapper mapper)
        {
            _domain = domain;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<SaleDto>>> ListAsync(SaleListFilterDto filter)
        {
            try
            {
                filter ??= new SaleListFilterDto();
                var sales = await _domain.ListAsync(_mapper.Map<DomainSaleFilter>(filter));
                return Success(_mapper.Map<IEnumerable<SaleDto>>(sales), "Ventas listadas correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<SaleDto>>(ex.Message);
            }
        }

        public async Task<Response<SaleDetailDto>> GetByIdAsync(int saleId)
        {
            try
            {
                var sale = await _domain.GetByIdAsync(saleId);

                if (sale is null)
                    return Fail<SaleDetailDto>("Venta no encontrada.");

                return Success(_mapper.Map<SaleDetailDto>(sale), "Venta encontrada correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<SaleDetailDto>(ex.Message);
            }
        }

        public async Task<Response<SaleWriteResultDto>> CreateAsync(SaleCreateDto dto, int userId)
        {
            try
            {
                if (dto is null)
                    return Fail<SaleWriteResultDto>("La solicitud de venta es obligatoria.");

                var sale = _mapper.Map<DomainSaleCreate>(dto);
                sale.UserId = userId;

                var result = await _domain.CreateAsync(sale);
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Venta registrada correctamente."
                    : result.Message;

                return Success(_mapper.Map<SaleWriteResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<SaleWriteResultDto>(ex.Message);
            }
        }

        public async Task<Response<SaleCancelResultDto>> CancelAsync(int saleId, SaleCancelDto dto, int userId)
        {
            try
            {
                if (dto is null)
                    return Fail<SaleCancelResultDto>("La solicitud de anulación de venta es obligatoria.");

                var cancellation = _mapper.Map<DomainSaleCancel>(dto);
                cancellation.SaleId = saleId;
                cancellation.UserId = userId;

                var result = await _domain.CancelAsync(cancellation);
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Venta anulada correctamente."
                    : result.Message;

                return Success(_mapper.Map<SaleCancelResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<SaleCancelResultDto>(ex.Message);
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
