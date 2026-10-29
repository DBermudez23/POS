using AutoMapper;
using UtpPos.Framework.Application.DTO.Business;
using UtpPos.Framework.Application.Interface.Business;
using UtpPos.Framework.Domain.Interface.Business;
using UtpPos.Framework.Transversal.Common;
using DomainBusiness = UtpPos.Framework.Domain.Entity.Business.Business;

namespace UtpPos.Framework.Application.Main.Business
{
    public class BusinessApplication : IBusinessApplication
    {
        private readonly IBusinessDomain _domain;
        private readonly IMapper _mapper;

        public BusinessApplication(IBusinessDomain domain, IMapper mapper)
        {
            _domain = domain;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<BusinessDto>>> ListAsync(BusinessListFilterDto filter)
        {
            try
            {
                var businesses = await _domain.ListAsync(filter.Nit, filter.Name, filter.Status);
                return Success(_mapper.Map<IEnumerable<BusinessDto>>(businesses), "Businesses listed successfully.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<BusinessDto>>(ex.Message);
            }
        }

        public async Task<Response<BusinessDto>> GetByIdAsync(int id)
        {
            try
            {
                var business = await _domain.GetByIdAsync(id);
                if (business is null)
                    return Fail<BusinessDto>("Business was not found.");

                return Success(_mapper.Map<BusinessDto>(business), "Business found successfully.");
            }
            catch (Exception ex)
            {
                return Fail<BusinessDto>(ex.Message);
            }
        }

        public async Task<Response<int>> CreateAsync(BusinessCreateDto dto)
        {
            try
            {
                var business = _mapper.Map<DomainBusiness>(dto);
                var id = await _domain.CreateAsync(business);
                return Success(id, "Business created successfully.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
            }
        }

        public async Task<Response<int>> UpdateAsync(int id, BusinessUpdateDto dto)
        {
            try
            {
                var business = _mapper.Map<DomainBusiness>(dto);
                business.Id = id;

                var updatedId = await _domain.UpdateAsync(business, dto.ClearPhone);
                return Success(updatedId, "Business updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
            }
        }

        public async Task<Response<int>> DeleteAsync(int id)
        {
            try
            {
                var deletedId = await _domain.DeleteAsync(id);
                return Success(deletedId, "Business deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
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
