using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Dashboard;
using UtpPos.Framework.Shared;
using UtpPos.Framework.Shared.Dashboard;

namespace UtpPos.Framework.Main.Dashboard
{
    public sealed class DashboardService : IDashboardService
    {
        private const string Endpoint = "api/dashboard";

        private readonly IRestApi _restApi;

        public DashboardService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public Task<Response<DashboardBasicOverviewDto>> GetBasicOverviewAsync(DashboardOverviewFilterDto filter)
        {
            return _restApi.GetAsync<DashboardBasicOverviewDto>(
                BuildEndpoint("basic-overview", filter, includeTop: true),
                authorized: true);
        }

        public Task<Response<DashboardFinancialOverviewDto>> GetFinancialOverviewAsync(DashboardOverviewFilterDto filter)
        {
            return _restApi.GetAsync<DashboardFinancialOverviewDto>(
                BuildEndpoint("financial-overview", filter, includeTop: false),
                authorized: true);
        }

        private static string BuildEndpoint(string resource, DashboardOverviewFilterDto filter, bool includeTop)
        {
            var query = new List<string>();
            AddQueryValue(query, "businessId", filter.BusinessId);
            AddQueryValue(query, "startDate", filter.StartDate);
            AddQueryValue(query, "endDate", filter.EndDate);

            if (includeTop)
                AddQueryValue(query, "top", filter.Top);

            return query.Count == 0
                ? $"{Endpoint}/{resource}"
                : $"{Endpoint}/{resource}?{string.Join("&", query)}";
        }

        private static void AddQueryValue(ICollection<string> query, string name, int? value)
        {
            if (value.HasValue)
                query.Add($"{name}={value.Value}");
        }

        private static void AddQueryValue(ICollection<string> query, string name, DateTime? value)
        {
            if (value.HasValue)
                query.Add($"{name}={Uri.EscapeDataString(value.Value.ToString("yyyy-MM-dd"))}");
        }
    }
}
