using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Parameter;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Parameter;

namespace UtpPos.Framework.Main.Parameter
{
    public sealed class ParameterService : IParameterService
    {
        private const string Endpoint = "api/parametros";

        private readonly IRestApi _restApi;

        public ParameterService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public Task<Response<ParameterDto>> GetByCodeAsync(string parameterCode)
        {
            var safeParameterCode = Uri.EscapeDataString(parameterCode.Trim());

            return _restApi.GetAsync<ParameterDto>(
                $"{Endpoint}/por-codigo/{safeParameterCode}",
                authorized: true);
        }

        public Task<Response<IReadOnlyList<ParameterOptionDto>>> ListByClassCodeAsync(string classCode)
        {
            var safeClassCode = Uri.EscapeDataString(classCode.Trim());

            return _restApi.GetAsync<IReadOnlyList<ParameterOptionDto>>(
                $"{Endpoint}/por-clase/{safeClassCode}",
                authorized: true);
        }

        public async Task<Response<IReadOnlyList<ParameterDto>>> ListAsync(ParameterListFilterDto filter)
        {
            var response = await _restApi.GetAsync<IEnumerable<ParameterDto>>(
                BuildListEndpoint(filter),
                authorized: true);

            return new Response<IReadOnlyList<ParameterDto>>
            {
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                StatusCode = response.StatusCode,
                Data = response.Data?.ToArray() ?? Array.Empty<ParameterDto>()
            };
        }

        private static string BuildListEndpoint(ParameterListFilterDto filter)
        {
            var query = new List<string>();
            AddQueryValue(query, "ClassId", filter.ClassId);
            AddQueryValue(query, "ClassDescription", filter.ClassDescription);
            AddQueryValue(query, "Description", filter.Description);
            AddQueryValue(query, "Status", filter.Status);

            return query.Count == 0 ? Endpoint : $"{Endpoint}?{string.Join("&", query)}";
        }

        private static void AddQueryValue(ICollection<string> query, string name, int? value)
        {
            if (value.HasValue)
            {
                query.Add($"{name}={value.Value}");
            }
        }

        private static void AddQueryValue(ICollection<string> query, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
            }
        }
    }
}
