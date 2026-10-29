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
using UtpPos.Framework.Interface.Product;
using UtpPos.Framework.Interface.File;
using UtpPos.Framework.Main;
using UtpPos.Framework.Main.Auth;
using UtpPos.Framework.Main.Authorization;
using UtpPos.Framework.Main.Business;
using UtpPos.Framework.Main.Dashboard;
using UtpPos.Framework.Main.Parameter;
using UtpPos.Framework.Main.Purchase;
using UtpPos.Framework.Main.Rbac;
using UtpPos.Framework.Main.Sale;
using UtpPos.Framework.Main.ThirdParty;
using UtpPos.Framework.Main.User;
using UtpPos.Framework.Main.Product;
using UtpPos.Framework.Main.File;
using UtpPos.MudBlazorWeb.Client.Pages;
using UtpPos.MudBlazorWeb.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

var culture = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

#region UI Services

builder.Services.AddMudServices();

#endregion

#region Razor Components

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

#endregion

#region Http Client

builder.Services.AddScoped<HttpClient>();

#endregion

#region Browser Storage

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddBlazoredLocalStorage();

#endregion

#region Authorization

builder.Services.AddAuthorization();
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
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IThirdPartyService, ThirdPartyService>();

#endregion

#region Products Services

builder.Services.AddScoped<IProductService, ProductService>();

#endregion

var app = builder.Build();

#region HTTP Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

#endregion

#region Endpoints

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(UtpPos.MudBlazorWeb.Client._Imports).Assembly)
    .AllowAnonymous();

#endregion

app.Run();
