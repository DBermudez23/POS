using AutoMapper;
using UtpPos.Framework.Application.DTO.Parameter;
using UtpPos.Framework.Application.Interface.Parameter;
using UtpPos.Framework.Domain.Interface.Parameter;
using UtpPos.Framework.Transversal.Common;
using DomainParameter = UtpPos.Framework.Domain.Entity.Parameter.Parameter;

namespace UtpPos.Framework.Application.Main.Parameter
{
    public class ParameterApplication : IParameterApplication
    {
        private readonly IParameterDomain _domain;
        private readonly IMapper _mapper;

        public ParameterApplication(IParameterDomain domain, IMapper mapper)
        {
            _domain = domain;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ParameterDto>>> ListAsync(ParameterListFilterDto filter)
        {
            try
            {
                var parameters = await _domain.ListAsync(filter.ClassId, filter.ClassDescription, filter.Description, filter.Status);
                return Success(_mapper.Map<IEnumerable<ParameterDto>>(parameters), "Parameters listed successfully.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<ParameterDto>>(ex.Message);
            }
        }

        public async Task<Response<IEnumerable<ParameterOptionDto>>> ListByClassCodeAsync(string classCode)
        {
            try
            {
                var parameters = await _domain.ListByClassCodeAsync(classCode, "A");

                if (!parameters.Any())
                {
                    return Success(Enumerable.Empty<ParameterOptionDto>(), "No se encontraron parámetros para la clase solicitada.");
                }

                var dto = parameters.Select(param => new ParameterOptionDto
                {
                    Id = param.Id,
                    Code = param.Code,
                    Description = param.Description
                });

                return Success(dto, "Parámetros listados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<ParameterOptionDto>>(ex.Message);
            }
        }

        public async Task<Response<ParameterDto>> GetByIdAsync(int id)
        {
            try
            {
                var parameter = await _domain.GetByIdAsync(id);
                if (parameter is null)
                    return Fail<ParameterDto>("Parameter was not found.");

                return Success(_mapper.Map<ParameterDto>(parameter), "Parameter found successfully.");
            }
            catch (Exception ex)
            {
                return Fail<ParameterDto>(ex.Message);
            }
        }

        public async Task<Response<ParameterDto>>GetByCodeAsync(string parameterCode)
        {
            try
            {
                var parameter = await _domain.GetByCodeAsync(parameterCode);
                if (parameter is null)
                    return Fail<ParameterDto>("Parameter was not found.");

                return Success(_mapper.Map<ParameterDto>(parameter), "Parameter found successfully.");
            }
            catch (Exception ex)
            {
                return Fail<ParameterDto>(ex.Message);
            }
        }

        public async Task<Response<int>> CreateAsync(ParameterCreateDto dto)
        {
            try
            {
                var parameter = _mapper.Map<DomainParameter>(dto);
                var id = await _domain.CreateAsync(parameter);
                return Success(id, "Parameter created successfully.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
            }
        }

        public async Task<Response<int>> UpdateAsync(int id, ParameterUpdateDto dto)
        {
            try
            {
                var parameter = _mapper.Map<DomainParameter>(dto);
                parameter.Id = id;

                var updatedId = await _domain.UpdateAsync(parameter);
                return Success(updatedId, "Parameter updated successfully.");
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
                return Success(deletedId, "Parameter deleted successfully.");
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
