using AutoMapper;
using UtpPos.Framework.Application.DTO.ThirdParty;
using UtpPos.Framework.Application.Interface.ThirdParty;
using UtpPos.Framework.Domain.Interface.ThirdParty;
using UtpPos.Framework.Transversal.Common;
using DomainThirdPartyBusinessAssociation = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessAssociation;
using DomainThirdPartyBusinessFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyBusinessFilter;
using DomainThirdPartyCreateForBusiness = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreateForBusiness;
using DomainThirdPartyCreate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyCreate;
using DomainThirdPartyFilter = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyFilter;
using DomainThirdPartyUpdate = UtpPos.Framework.Domain.Entity.ThirdParty.ThirdPartyUpdate;

namespace UtpPos.Framework.Application.Main.ThirdParty
{
    public sealed class ThirdPartyApplication : IThirdPartyApplication
    {
        private readonly IThirdPartyDomain _domain;
        private readonly IMapper _mapper;

        public ThirdPartyApplication(IThirdPartyDomain domain, IMapper mapper)
        {
            _domain = domain;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ThirdPartyDto>>> ListAsync(ThirdPartyListFilterDto filter)
        {
            try
            {
                filter ??= new ThirdPartyListFilterDto();
                var thirdParties = await _domain.ListAsync(_mapper.Map<DomainThirdPartyFilter>(filter));
                return Success(_mapper.Map<IEnumerable<ThirdPartyDto>>(thirdParties), "Terceros listados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<ThirdPartyDto>>(ex.Message);
            }
        }

        public async Task<Response<ThirdPartyDto>> GetByIdAsync(int thirdPartyId)
        {
            try
            {
                var thirdParty = await _domain.GetByIdAsync(thirdPartyId);

                if (thirdParty is null)
                    return Fail<ThirdPartyDto>("Tercero no encontrado.");

                return Success(_mapper.Map<ThirdPartyDto>(thirdParty), "Tercero encontrado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<ThirdPartyDto>(ex.Message);
            }
        }

        public async Task<Response<ThirdPartyResultDto>> CreateAsync(ThirdPartyCreateDto dto, int userId)
        {
            try
            {
                if (dto is null)
                    return Fail<ThirdPartyResultDto>("La solicitud de tercero es obligatoria.");

                var thirdParty = _mapper.Map<DomainThirdPartyCreate>(dto);
                thirdParty.CreatedByUserId = userId;

                var result = await _domain.CreateAsync(thirdParty);
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Tercero creado correctamente."
                    : result.Message;

                return Success(_mapper.Map<ThirdPartyResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<ThirdPartyResultDto>(ex.Message);
            }
        }

        public async Task<Response<ThirdPartyResultDto>> UpdateAsync(int thirdPartyId, ThirdPartyUpdateDto dto)
        {
            try
            {
                if (dto is null)
                    return Fail<ThirdPartyResultDto>("La solicitud de actualización de tercero es obligatoria.");

                var thirdParty = _mapper.Map<DomainThirdPartyUpdate>(dto);
                thirdParty.Id = thirdPartyId;

                var result = await _domain.UpdateAsync(thirdParty);
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Tercero actualizado correctamente."
                    : result.Message;

                return Success(_mapper.Map<ThirdPartyResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<ThirdPartyResultDto>(ex.Message);
            }
        }

        public async Task<Response<ThirdPartyResultDto>> DeleteAsync(int thirdPartyId)
        {
            try
            {
                var result = await _domain.DeleteAsync(thirdPartyId);
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Tercero inactivado correctamente."
                    : result.Message;

                return Success(_mapper.Map<ThirdPartyResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<ThirdPartyResultDto>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<ThirdPartyBusinessDto>>> ListByBusinessAsync(int businessId, ThirdPartyBusinessListFilterDto filter)
        {
            try
            {
                filter ??= new ThirdPartyBusinessListFilterDto();
                var domainFilter = _mapper.Map<DomainThirdPartyBusinessFilter>(filter);
                domainFilter.BusinessId = businessId;

                var thirdParties = await _domain.ListByBusinessAsync(domainFilter);
                return Success(_mapper.Map<IEnumerable<ThirdPartyBusinessDto>>(thirdParties), "Terceros del negocio listados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<ThirdPartyBusinessDto>>(ex.Message);
            }
        }

        public async Task<Response<ThirdPartyBusinessResultDto>> AssociateBusinessAsync(int businessId, int thirdPartyId, int userId)
        {
            try
            {
                var result = await _domain.AssociateBusinessAsync(new DomainThirdPartyBusinessAssociation
                {
                    BusinessId = businessId,
                    ThirdPartyId = thirdPartyId,
                    UserId = userId
                });

                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Tercero asociado al negocio correctamente."
                    : result.Message;

                return Success(_mapper.Map<ThirdPartyBusinessResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<ThirdPartyBusinessResultDto>(ex.Message);
            }
        }

        public async Task<Response<ThirdPartyBusinessResultDto>> InactivateBusinessAsync(int businessId, int thirdPartyId, int userId)
        {
            try
            {
                var result = await _domain.InactivateBusinessAsync(new DomainThirdPartyBusinessAssociation
                {
                    BusinessId = businessId,
                    ThirdPartyId = thirdPartyId,
                    UserId = userId
                });

                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Relación tercero-negocio inactivada correctamente."
                    : result.Message;

                return Success(_mapper.Map<ThirdPartyBusinessResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<ThirdPartyBusinessResultDto>(ex.Message);
            }
        }

        public async Task<Response<ThirdPartyRelationValidationDto>> ValidateBusinessAsync(int businessId, int thirdPartyId)
        {
            try
            {
                var existsActiveRelation = await _domain.ValidateBusinessAsync(businessId, thirdPartyId);
                return Success(new ThirdPartyRelationValidationDto { ExistsActiveRelation = existsActiveRelation }, "Relación tercero-negocio validada correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<ThirdPartyRelationValidationDto>(ex.Message);
            }
        }

        public async Task<Response<ThirdPartyBusinessResultDto>> CreateForBusinessAsync(ThirdPartyCreateForBusinessDto dto, int userId)
        {
            try
            {
                if (dto is null)
                    return Fail<ThirdPartyBusinessResultDto>("La solicitud de tercero es obligatoria.");

                var thirdParty = _mapper.Map<DomainThirdPartyCreateForBusiness>(dto);
                thirdParty.CreatedByUserId = userId;

                var result = await _domain.CreateForBusinessAsync(thirdParty);
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Tercero creado o asociado al negocio correctamente."
                    : result.Message;

                return Success(_mapper.Map<ThirdPartyBusinessResultDto>(result), message);
            }
            catch (Exception ex)
            {
                return Fail<ThirdPartyBusinessResultDto>(ex.Message);
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
