using System.Globalization;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using UtpPos.Framework.Interface;
using UtpPos.Framework.Interface.Auth;
using UtpPos.Framework.Interface.Authorization;
using UtpPos.Framework.Interface.Business;
using UtpPos.Framework.Interface.Dashboard;
using UtpPos.Framework.Interface.Parameter;
using UtpPos.Framework.Interface.Purchase;
using UtpPos.Framework.Interface.Rbac;
using UtpPos.Framework.Interface.Sale;
using UtpPos.Framework.Interface.ThirdParty;
using UtpPos.Framework.Interface.User;
using UtpPos.Framework.Interface.File;
using UtpPos.Framework.Interface.Inventory;
using UtpPos.Framework.Interface.Kardex;
using UtpPos.Framework.Interface.Product;
using UtpPos.Framework.Main;
using UtpPos.Framework.Main.File;
using UtpPos.Framework.Main.Auth;
using UtpPos.Framework.Main.Authorization;
using UtpPos.Framework.Main.Business;
using UtpPos.Framework.Main.Dashboard;
using UtpPos.Framework.Main.Inventory;
using UtpPos.Framework.Main.Kardex;
using UtpPos.Framework.Main.Parameter;
using UtpPos.Framework.Main.Purchase;
using UtpPos.Framework.Main.Rbac;
using UtpPos.Framework.Main.Sale;
using UtpPos.Framework.Main.ThirdParty;
using UtpPos.Framework.Main.User;
using UtpPos.Framework.Main.Product;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

#region Http Client

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

#endregion

#region UI Services

builder.Services.AddMudServices();

#endregion

#region Browser Storage

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazoredLocalStorage();

#endregion

#region Authorization

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(serviceProvider => serviceProvider.GetRequiredService<CustomAuthenticationStateProvider>());

#endregion

#region Application Services

builder.Services.AddScoped<IRestApi, RestApi>();
builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<PermissionStateService>();
builder.Services.AddScoped<ICurrentUserRoleService, CurrentUserRoleService>();
builder.Services.AddScoped<CurrentUserRoleStateService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IParameterService, ParameterService>();
builder.Services.AddScoped<IRbacManagementService, RbacManagementService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IThirdPartyService, ThirdPartyService>();

#endregion

#region Products Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IKardexService, KardexService>();
#endregion

var culture = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();
