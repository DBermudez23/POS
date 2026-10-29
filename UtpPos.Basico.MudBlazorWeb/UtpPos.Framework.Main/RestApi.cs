using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Auth;
using UtpPos.Framework.Main.Auth;
using UtpPos.Framework.Shared;
using Microsoft.Extensions.Configuration;

namespace UtpPos.Framework.Main
{
    public class RestApi : IRestApi
    {
        private const string BaseUrlConfigurationKey = "ApiSettings:BaseUrl";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ITokenStorageService _tokenStorageService;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

        public RestApi(
            HttpClient httpClient,
            IConfiguration configuration,
            ITokenStorageService tokenStorageService,
            CustomAuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _tokenStorageService = tokenStorageService;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public Task<Response<TResponse>> GetAsync<TResponse>(string endpoint, bool authorized = false) =>
            SendAsync<TResponse>(HttpMethod.Get, endpoint, null, authorized);

        public Task<Response<TResponse>> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest request,
            bool authorized = false) =>
            SendAsync<TResponse>(HttpMethod.Post, endpoint, request, authorized);

        public Task<Response<TResponse>> PutAsync<TRequest, TResponse>(
            string endpoint,
            TRequest request,
            bool authorized = false) =>
            SendAsync<TResponse>(HttpMethod.Put, endpoint, request, authorized);

        public Task<Response<TResponse>> PatchAsync<TRequest, TResponse>(
            string endpoint,
            TRequest request,
            bool authorized = false) =>
            SendAsync<TResponse>(HttpMethod.Patch, endpoint, request, authorized);

        public Task<Response<TResponse>> DeleteAsync<TResponse>(string endpoint, bool authorized = false) =>
            SendAsync<TResponse>(HttpMethod.Delete, endpoint, null, authorized);

        private async Task<Response<TResponse>> SendAsync<TResponse>(
            HttpMethod method,
            string endpoint,
            object? request,
            bool authorized)
        {
            try
            {
                using var httpRequest = new HttpRequestMessage(method, BuildRequestUri(endpoint));

                if (request is not null)
                {
                    httpRequest.Content = JsonContent.Create(request, options: JsonOptions);
                }

                if (authorized)
                {
                    var accessToken = await _tokenStorageService.GetAccessTokenAsync();
                    if (string.IsNullOrWhiteSpace(accessToken))
                    {
                        await _authenticationStateProvider.ClearAuthenticationAsync();
                        return Failure<TResponse>("No hay una sesión activa. Inicie sesión nuevamente.", HttpStatusCode.Unauthorized);
                    }

                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }

                using var httpResponse = await _httpClient.SendAsync(httpRequest);
                return await ReadResponseAsync<TResponse>(httpResponse, method, endpoint);
            }
            catch (HttpRequestException)
            {
                return Failure<TResponse>("No fue posible conectar con el servidor.");
            }
            catch (TaskCanceledException)
            {
                return Failure<TResponse>("La solicitud al servidor excedió el tiempo de espera.");
            }
            catch (JsonException)
            {
                return Failure<TResponse>("El servidor devolvió una respuesta no válida.");
            }
            catch (InvalidOperationException exception)
            {
                return Failure<TResponse>(exception.Message);
            }
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

        private async Task<Response<TResponse>> ReadResponseAsync<TResponse>(HttpResponseMessage response, HttpMethod method, string endpoint)
        {
            Response<TResponse>? apiResponse = null;

            if (response.Content.Headers.ContentLength is not 0)
            {
                apiResponse = await response.Content.ReadFromJsonAsync<Response<TResponse>>(JsonOptions);
            }

            if (response.IsSuccessStatusCode)
            {
                if (apiResponse is not null)
                {
                    apiResponse.StatusCode = (int)response.StatusCode;
                    return apiResponse;
                }

                return Failure<TResponse>("El servidor devolvió una respuesta vacía.", response.StatusCode);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _authenticationStateProvider.ClearAuthenticationAsync();
            }

            var message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => ResolveUnauthorizedMessage(apiResponse?.Message),
                HttpStatusCode.Forbidden => ResolveForbiddenMessage(apiResponse?.Message, method, endpoint),
                _ => apiResponse?.Message
            };

            return Failure<TResponse>(string.IsNullOrWhiteSpace(message)
                ? $"La solicitud falló con el código {(int)response.StatusCode}."
                : message,
                response.StatusCode);
        }

        private static string ResolveUnauthorizedMessage(string? apiMessage) => string.IsNullOrWhiteSpace(apiMessage)
            ? "Sesión expirada o no autorizada. Inicia sesión nuevamente."
            : apiMessage;

        private static string ResolveForbiddenMessage(string? apiMessage, HttpMethod method, string endpoint)
        {
            var detail = string.IsNullOrWhiteSpace(apiMessage)
                ? "El usuario autenticado no tiene el permiso requerido."
                : apiMessage.Trim();

            return $"Permiso denegado al llamar {method.Method} {endpoint}: {detail}";
        }

        private static Response<TResponse> Failure<TResponse>(string message, HttpStatusCode? statusCode = null) => new()
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode.HasValue ? (int)statusCode.Value : null
        };
    }
}
