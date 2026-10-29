using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UtpPos.Framework.Interface.Auth;
using UtpPos.Framework.Interface.File;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.File;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;

namespace UtpPos.Framework.Main.File
{
    public sealed class FileUploadService : IFileUploadService
    {
        private const string BaseUrlConfigurationKey = "ApiSettings:BaseUrl";
        private const string ProductImageUploadEndpoint = "api/uploads/product-images";

        private const long MaxFileSizeBytes = 3 * 1024 * 1024;

        private static readonly string[] AllowedContentTypes =
        [
            "image/jpeg",
            "image/png",
            "image/webp"
        ];

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ITokenStorageService _tokenStorageService;

        public FileUploadService(
            HttpClient httpClient,
            IConfiguration configuration,
            ITokenStorageService tokenStorageService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tokenStorageService = tokenStorageService;
        }

        public async Task<Response<FileUploadResultDto>> UploadProductImageAsync(IBrowserFile file)
        {
            try
            {
                var validationMessage = ValidateFile(file);
                if (!string.IsNullOrWhiteSpace(validationMessage))
                {
                    return Failure(validationMessage);
                }

                var accessToken = await _tokenStorageService.GetAccessTokenAsync();
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return Failure("No hay una sesión activa. Inicie sesión nuevamente.");
                }

                using var content = new MultipartFormDataContent();
                await using var fileStream = file.OpenReadStream(MaxFileSizeBytes);

                // Comenzamos a leer el archívo por bloques de bytes y lo vamos agregando al contenido de la petición HTTP
                using var fileContent = new StreamContent(fileStream);

                // En esta linea de código se marca el tipo de contenido del archívo que se va a subir
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.Name);

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    BuildRequestUri(ProductImageUploadEndpoint));

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Content = content;

                using var response = await _httpClient.SendAsync(request);

                return await ReadResponseAsync(response);
            }
            catch (HttpRequestException)
            {
                return Failure("No fue posible conectar con el servidor.");
            }
            catch (TaskCanceledException)
            {
                return Failure("La carga de la imagen excedió el tiempo de espera.");
            }
            catch (JsonException)
            {
                return Failure("El servidor devolvió una respuesta no válida.");
            }
            catch (InvalidOperationException exception)
            {
                return Failure(exception.Message);
            }
            catch (IOException)
            {
                return Failure("No fue posible leer el archivo seleccionado.");
            }
        }

        private static string? ValidateFile(IBrowserFile? file)
        {
            if (file is null)
                return "Debe seleccionar una imagen.";

            if (file.Size <= 0)
                return "La imagen seleccionada está vacía.";

            if (file.Size > MaxFileSizeBytes)
                return "La imagen no puede superar 3 MB.";

            if (!AllowedContentTypes.Contains(file.ContentType))
                return "Solo se permiten imágenes JPG, PNG o WEBP.";

            return null;
        }

        private Uri BuildRequestUri(string endpoint)
        {
            var baseUrl = _configuration[BaseUrlConfigurationKey];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException($"No se configuró la URL del servidor en '{BaseUrlConfigurationKey}'.");
            }

            var normalizedBaseUrl = baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/";
            return new Uri(new Uri(normalizedBaseUrl), endpoint.TrimStart('/'));
        }

        private static async Task<Response<FileUploadResultDto>> ReadResponseAsync(HttpResponseMessage response)
        {
            Response<FileUploadResultDto>? apiResponse = null;

            if (response.Content.Headers.ContentLength is not 0)
            {
                apiResponse = await response.Content.ReadFromJsonAsync<Response<FileUploadResultDto>>(JsonOptions);
            }

            if (response.IsSuccessStatusCode)
            {
                return apiResponse ?? Failure("El servidor devolvió una respuesta vacía.");
            }

            var message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Sesión expirada o no autorizada. Inicia sesión nuevamente.",
                HttpStatusCode.Forbidden => "No tienes permisos para cargar imágenes de productos.",
                _ => apiResponse?.Message
            };

            return Failure(string.IsNullOrWhiteSpace(message)
                ? $"La carga falló con el código {(int)response.StatusCode}."
                : message);
        }

        private static Response<FileUploadResultDto> Failure(string message) => new()
        {
            IsSuccess = false,
            Message = message,
            Data = null
        };
    }
}