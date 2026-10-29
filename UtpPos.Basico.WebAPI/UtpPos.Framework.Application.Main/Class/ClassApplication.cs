using AutoMapper;
using UtpPos.Framework.Application.DTO.Class;
using UtpPos.Framework.Application.Interface.Class;
using UtpPos.Framework.Domain.Interface.Class;
using UtpPos.Framework.Transversal.Common;
using DomainClass = UtpPos.Framework.Domain.Entity.Class.Class;

namespace UtpPos.Framework.Application.Main.Class
{
    public class ClassApplication : IClassApplication
    {
        private readonly IClassDomain _domain;
        private readonly IMapper _mapper;

        public ClassApplication(IClassDomain domain, IMapper mapper)
        {
            _domain = domain;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<ClassDto>>> ListAsync(ClassListFilterDto filter)
        {
            try
            {
                var classes = await _domain.ListAsync(filter.Description, filter.Status, filter.IsEditable);
                return Success(_mapper.Map<IEnumerable<ClassDto>>(classes), "Classes listed successfully.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<ClassDto>>(ex.Message);
            }
        }

        public async Task<Response<ClassDto>> GetByIdAsync(int id)
        {
            try
            {
                var classEntity = await _domain.GetByIdAsync(id);
                if (classEntity is null)
                    return Fail<ClassDto>("Class was not found.");

                return Success(_mapper.Map<ClassDto>(classEntity), "Class found successfully.");
            }
            catch (Exception ex)
            {
                return Fail<ClassDto>(ex.Message);
            }
        }

        public async Task<Response<int>> CreateAsync(ClassCreateDto dto)
        {
            try
            {
                var classEntity = _mapper.Map<DomainClass>(dto);
                var id = await _domain.CreateAsync(classEntity);
                return Success(id, "Class created successfully.");
            }
            catch (Exception ex)
            {
                return Fail<int>(ex.Message);
            }
        }

        public async Task<Response<int>> UpdateAsync(int id, ClassUpdateDto dto)
        {
            try
            {
                var classEntity = _mapper.Map<DomainClass>(dto);
                classEntity.Id = id;

                var updatedId = await _domain.UpdateAsync(classEntity);
                return Success(updatedId, "Class updated successfully.");
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
                return Success(deletedId, "Class deleted successfully.");
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
