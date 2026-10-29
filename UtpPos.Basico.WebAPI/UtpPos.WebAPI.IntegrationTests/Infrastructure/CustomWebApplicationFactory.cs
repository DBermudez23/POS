using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Application.Interface.Product;
using UtpPos.Framework.Application.Interface.ThirdParty;
using UtpPos.Framework.InfraStructure.Interface.Dashboard;
using UtpPos.WebAPI.IntegrationTests.Auth;
using UtpPos.WebAPI.IntegrationTests.Fakes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace UtpPos.WebAPI.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeThirdPartyApplication ThirdPartyApplication { get; } = new();
    public FakeProductApplication ProductApplication { get; } = new();
    public FakeDashboardRepository DashboardRepository { get; } = new();
    public FakeRbacApplication? RbacApplication { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "integration-tests",
                ["Jwt:Audience"] = "integration-tests",
                ["Jwt:SecretKey"] = "integration-tests-secret-key-32-bytes-minimum",
                ["Jwt:RefreshTokenHashKey"] = "integration-tests-refresh-key-32-bytes-minimum",
                ["Jwt:AccessTokenExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddHttpContextAccessor();

            services.RemoveAll<IRbacApplication>();
            services.RemoveAll<IDashboardRepository>();
            services.RemoveAll<IProductApplication>();
            services.RemoveAll<IThirdPartyApplication>();

            services.AddSingleton<IDashboardRepository>(DashboardRepository);
            services.AddSingleton<IProductApplication>(ProductApplication);
            services.AddSingleton<IThirdPartyApplication>(ThirdPartyApplication);
            services.AddScoped<IRbacApplication>(provider =>
            {
                var fake = new FakeRbacApplication(provider.GetRequiredService<IHttpContextAccessor>());
                RbacApplication = fake;
                return fake;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                options.DefaultForbidScheme = TestAuthHandler.AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });
        });
    }
}
