using System.Net.Mail;
using AutoMapper;
using UtpPos.Framework.Application.DTO.User;
using UtpPos.Framework.Application.Interface.User;
using UtpPos.Framework.InfraStructure.Interface.User;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using DomainUser = UtpPos.Framework.Domain.Entity.User.User;

namespace UtpPos.Framework.Application.Main.User
{
    public class UserApplication : IUserApplication
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasherService _passwordHasherService;

        public UserApplication(
            IUserRepository repository,
            IMapper mapper,
            IPasswordHasherService passwordHasherService)
        {
            _repository = repository;
            _mapper = mapper;
            _passwordHasherService = passwordHasherService;
        }

        public async Task<Response<IEnumerable<UserDto>>> ListAsync(UserListFilterDto filter)
        {
            filter ??= new UserListFilterDto();

            if (filter.BusinessId.HasValue && filter.BusinessId <= 0)
                return Fail<IEnumerable<UserDto>>("El negocio no es válido.");

            if (filter.DocumentTypeId.HasValue && filter.DocumentTypeId <= 0)
                return Fail<IEnumerable<UserDto>>("El tipo de documento no es válido.");

            var status = EmptyToNull(filter.Status)?.ToUpperInvariant();
            if (status is not null && !IsValidStatus(status))
                return Fail<IEnumerable<UserDto>>("El estado debe ser A o I.");

            try
            {
                var users = await _repository.ListAsync(
                    filter.BusinessId,
                    filter.DocumentTypeId,
                    status,
                    EmptyToNull(filter.DocumentNumber),
                    EmptyToNull(filter.Name),
                    EmptyToNull(filter.Email)?.ToLowerInvariant());

                return Success(
                    _mapper.Map<IEnumerable<UserDto>>(users),
                    "Usuarios consultados correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<IEnumerable<UserDto>>(ex.Message);
            }
        }

        public async Task<Response<UserDto>> GetByIdAsync(int userId)
        {
            if (userId <= 0)
                return Fail<UserDto>("El usuario no es valido.");

            try
            {
                var user = await _repository.GetByIdAsync(userId);
                if (user is null)
                    return Fail<UserDto>("No se encontro el usuario solicitado.");

                return Success(
                    _mapper.Map<UserDto>(user),
                    "Usuario consultado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<UserDto>(ex.Message);
            }
        }

        public async Task<Response<UserCreateResultDto>> CreateAsync(UserCreateDto request)
        {
            var validation = ValidateCreateRequest(request);
            if (validation is not null)
                return Fail<UserCreateResultDto>(validation);

            request.DocumentNumber = request.DocumentNumber.Trim();
            request.Name = request.Name.Trim();
            request.Email = request.Email.Trim().ToLowerInvariant();
            request.PasswordHash = request.PasswordHash.Trim();
            request.Phone = EmptyToNull(request.Phone);
            request.Status = request.Status.Trim().ToUpperInvariant();

            try
            {
                var user = _mapper.Map<DomainUser>(request);
                var userId = await _repository.CreateAsync(user);
                return Success(
                    new UserCreateResultDto { UserId = userId },
                    "Usuario creado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<UserCreateResultDto>(ex.Message);
            }
        }

        public async Task<Response<bool>> UpdateAsync(UserUpdateDto request)
        {
            var validation = ValidateUpdateRequest(request);
            if (validation is not null)
                return Fail<bool>(validation);

            request.DocumentNumber = EmptyToNull(request.DocumentNumber);
            request.Name = EmptyToNull(request.Name);
            request.Email = EmptyToNull(request.Email)?.ToLowerInvariant();
            request.PasswordHash = EmptyToNull(request.PasswordHash);
            request.Phone = EmptyToNull(request.Phone);
            request.Status = EmptyToNull(request.Status)?.ToUpperInvariant();

            try
            {
                var user = new DomainUser
                {
                    Id = request.UserId,
                    BusinessId = request.BusinessId,
                    DocumentTypeId = request.DocumentTypeId ?? 0,
                    DocumentNumber = request.DocumentNumber ?? string.Empty,
                    Name = request.Name ?? string.Empty,
                    Email = request.Email ?? string.Empty,
                    PasswordHash = request.PasswordHash ?? string.Empty,
                    Phone = request.Phone,
                    Status = request.Status ?? string.Empty
                };

                await _repository.UpdateAsync(user, request.ClearBusinessId, request.ClearPhone);
                return Success(true, "Usuario actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

        public async Task<Response<bool>> DeactivateAsync(int userId)
        {
            if (userId <= 0)
                return Fail<bool>("El usuario no es valido.");

            try
            {
                await _repository.DeactivateAsync(userId);
                return Success(true, "Usuario inactivado correctamente.");
            }
            catch (Exception ex)
            {
                return Fail<bool>(ex.Message);
            }
        }

        private string? ValidateCreateRequest(UserCreateDto? request)
        {
            if (request is null)
                return "Los datos del usuario son obligatorios.";

            if (request.BusinessId.HasValue && request.BusinessId <= 0)
                return "El negocio debe ser mayor que cero cuando se especifica.";

            if (request.DocumentTypeId <= 0)
                return "El tipo de documento es obligatorio y debe ser mayor que cero.";

            if (string.IsNullOrWhiteSpace(request.DocumentNumber))
                return "El número de documento es obligatorio.";

            if (request.DocumentNumber.Trim().Length > 20)
                return "El número de documento no puede superar 20 caracteres.";

            if (string.IsNullOrWhiteSpace(request.Name))
                return "El nombre es obligatorio.";

            if (request.Name.Trim().Length > 100)
                return "El nombre no puede superar 100 caracteres.";

            if (string.IsNullOrWhiteSpace(request.Email))
                return "El correo es obligatorio.";

            var email = request.Email.Trim();
            if (email.Length > 80)
                return "El correo no puede superar 80 caracteres.";

            if (!MailAddress.TryCreate(email, out var parsedEmail) ||
                !string.Equals(parsedEmail.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                return "El formato del correo no es válido.";
            }

            if (string.IsNullOrWhiteSpace(request.PasswordHash))
                return "El hash de contraseña es obligatorio.";

            if (request.PasswordHash.Trim().Length > 255)
                return "El hash de contraseña no puede superar 255 caracteres.";

            if (!_passwordHasherService.IsValidHash(request.PasswordHash.Trim()))
                return "El valor de contraseña debe ser un hash BCrypt válido.";

            if (EmptyToNull(request.Phone)?.Length > 20)
                return "El teléfono no puede superar 20 caracteres.";

            if (!IsValidStatus(request.Status))
                return "El estado debe ser A o I.";

            return null;
        }

        private string? ValidateUpdateRequest(UserUpdateDto? request)
        {
            if (request is null)
                return "Los datos del usuario son obligatorios.";

            if (request.UserId <= 0)
                return "El usuario es obligatorio y debe ser mayor que cero.";

            if (request.BusinessId.HasValue && request.BusinessId <= 0)
                return "El negocio debe ser mayor que cero cuando se especifica.";

            if (request.DocumentTypeId.HasValue && request.DocumentTypeId <= 0)
                return "El tipo de documento debe ser mayor que cero cuando se especifica.";

            if (request.ClearBusinessId && request.BusinessId.HasValue)
                return "No puede enviar BusinessId y ClearBusinessId al mismo tiempo.";

            if (request.ClearPhone && !string.IsNullOrWhiteSpace(request.Phone))
                return "No puede enviar Phone y ClearPhone al mismo tiempo.";

            if (EmptyToNull(request.DocumentNumber)?.Length > 20)
                return "El numero de documento no puede superar 20 caracteres.";

            if (EmptyToNull(request.Name)?.Length > 100)
                return "El nombre no puede superar 100 caracteres.";

            var email = EmptyToNull(request.Email);
            if (email is not null)
            {
                if (email.Length > 80)
                    return "El correo no puede superar 80 caracteres.";

                if (!MailAddress.TryCreate(email, out var parsedEmail) ||
                    !string.Equals(parsedEmail.Address, email, StringComparison.OrdinalIgnoreCase))
                {
                    return "El formato del correo no es valido.";
                }
            }

            if (EmptyToNull(request.PasswordHash) is { } passwordHash)
            {
                if (passwordHash.Length > 255)
                    return "El hash de contrasena no puede superar 255 caracteres.";

                if (!_passwordHasherService.IsValidHash(passwordHash))
                    return "El valor de contrasena debe ser un hash BCrypt valido.";
            }

            if (EmptyToNull(request.Phone)?.Length > 20)
                return "El telefono no puede superar 20 caracteres.";

            if (request.Status is not null && !IsValidStatus(request.Status))
                return "El estado debe ser A o I.";

            var hasDataToUpdate =
                request.BusinessId.HasValue ||
                request.DocumentTypeId.HasValue ||
                !string.IsNullOrWhiteSpace(request.DocumentNumber) ||
                !string.IsNullOrWhiteSpace(request.Name) ||
                !string.IsNullOrWhiteSpace(request.Email) ||
                !string.IsNullOrWhiteSpace(request.PasswordHash) ||
                !string.IsNullOrWhiteSpace(request.Phone) ||
                !string.IsNullOrWhiteSpace(request.Status) ||
                request.ClearBusinessId ||
                request.ClearPhone;

            return hasDataToUpdate ? null : "Debes enviar al menos un cambio para actualizar el usuario.";
        }

        private static bool IsValidStatus(string? status)
        {
            return string.Equals(status?.Trim(), "A", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status?.Trim(), "I", StringComparison.OrdinalIgnoreCase);
        }

        private static string? EmptyToNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
