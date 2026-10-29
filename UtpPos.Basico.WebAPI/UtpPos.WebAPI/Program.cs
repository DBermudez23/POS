using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using UtpPos.Framework.Domain.Entity.Helpers;
using UtpPos.Framework.InfraStructure.Data;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using UtpPos.Framework.Transversal.Interface;
using UtpPos.Framework.Transversal.Interface.Auth;
using UtpPos.Framework.Transversal.Logging;
using UtpPos.Framework.Transversal.Mapper;
using UtpPos.Mystery.Services.WebApi;
using Microsoft.OpenApi.Models;
using UtpPos.Framework.Application.Interface.Auth;
using UtpPos.Framework.Application.Main.Auth;
using UtpPos.Framework.InfraStructure.Interface.Auth;
using UtpPos.Framework.InfraStructure.Repository.Auth;
using UtpPos.Framework.Application.Interface.Business;
using UtpPos.Framework.Application.Interface.Class;
using UtpPos.Framework.Application.Interface.Dashboard;
using UtpPos.Framework.Application.Interface.Product;
using UtpPos.Framework.Application.Interface.Parameter;
using UtpPos.Framework.Application.Interface.Inventory;
using UtpPos.Framework.Application.Interface.Kardex;
using UtpPos.Framework.Application.Interface.Purchase;
using UtpPos.Framework.Application.Interface.Sale;
using UtpPos.Framework.Application.Interface.ThirdParty;
using UtpPos.Framework.Application.Interface.File;
using UtpPos.Framework.Application.Main.Business;
using UtpPos.Framework.Application.Main.Class;
using UtpPos.Framework.Application.Main.Dashboard;
using UtpPos.Framework.Application.Main.Product;
using UtpPos.Framework.Application.Main.Parameter;
using UtpPos.Framework.Application.Main.Inventory;
using UtpPos.Framework.Application.Main.Kardex;
using UtpPos.Framework.Application.Main.Purchase;
using UtpPos.Framework.Application.Main.Sale;
using UtpPos.Framework.Application.Main.ThirdParty;
using UtpPos.Framework.Application.Main.File;
using UtpPos.Framework.Domain.Core.Business;
using UtpPos.Framework.Domain.Core.Class;
using UtpPos.Framework.Domain.Core.Parameter;
using UtpPos.Framework.Domain.Core.Product;
using UtpPos.Framework.Domain.Core.Inventory;
using UtpPos.Framework.Domain.Core.Kardex;
using UtpPos.Framework.Domain.Core.Purchase;
using UtpPos.Framework.Domain.Core.Sale;
using UtpPos.Framework.Domain.Core.ThirdParty;
using UtpPos.Framework.Domain.Interface.Business;
using UtpPos.Framework.Domain.Interface.Class;
using UtpPos.Framework.Domain.Interface.Parameter;
using UtpPos.Framework.Domain.Interface.Product;
using UtpPos.Framework.Domain.Interface.Inventory;
using UtpPos.Framework.Domain.Interface.Kardex;
using UtpPos.Framework.Domain.Interface.Purchase;
using UtpPos.Framework.Domain.Interface.Sale;
using UtpPos.Framework.Domain.Interface.ThirdParty;
using UtpPos.Framework.InfraStructure.Interface.Product;
using UtpPos.Framework.InfraStructure.Interface.Business;
using UtpPos.Framework.InfraStructure.Interface.Class;
using UtpPos.Framework.InfraStructure.Interface.Dashboard;
using UtpPos.Framework.InfraStructure.Interface.Parameter;
using UtpPos.Framework.InfraStructure.Interface.Inventory;
using UtpPos.Framework.InfraStructure.Interface.Kardex;
using UtpPos.Framework.InfraStructure.Interface.Purchase;
using UtpPos.Framework.InfraStructure.Interface.Sale;
using UtpPos.Framework.InfraStructure.Interface.ThirdParty;
using UtpPos.Framework.InfraStructure.Interface.Storage;
using UtpPos.Framework.InfraStructure.Repository.Business;
using UtpPos.Framework.InfraStructure.Repository.Class;
using UtpPos.Framework.InfraStructure.Repository.Dashboard;
using UtpPos.Framework.InfraStructure.Repository.Parameter;
using UtpPos.Framework.InfraStructure.Repository.Product;
using UtpPos.Framework.InfraStructure.Repository.Inventory;
using UtpPos.Framework.InfraStructure.Repository.Kardex;
using UtpPos.Framework.InfraStructure.Repository.Purchase;
using UtpPos.Framework.InfraStructure.Repository.Sale;
using UtpPos.Framework.InfraStructure.Repository.ThirdParty;
using UtpPos.Framework.InfraStructure.Repository.Storage;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Application.Main.Rbac;
using UtpPos.Framework.InfraStructure.Interface.Rbac;
using UtpPos.Framework.InfraStructure.Repository.Rbac;
using UtpPos.Framework.Application.Interface.User;
using UtpPos.Framework.Application.Main.User;
using UtpPos.Framework.InfraStructure.Interface.User;
using UtpPos.Framework.InfraStructure.Repository.User;
using UtpPos.Framework.Services.WebApi.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

#region services

builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .SetIsOriginAllowed((host) => true)
               .AllowCredentials()
    );
});

//builder.Services.AddSignalR();
builder.Services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                });

builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = context.ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                ? "La solicitud no es válida."
                : error.ErrorMessage)
            .FirstOrDefault() ?? "La solicitud no es válida.";

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
            new Response<object>
            {
                IsSuccess = false,
                Data = null,
                Message = message
            });
    };
});
#endregion

#region Configuración de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SupportNonNullableReferenceTypes();

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UTP POS API",
        Version = "v1",
        Description = "UTP POS backend API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
});
#endregion


#region app
ConfigurationManager Configuration = builder.Configuration; // allows both to access and to set up the config
IWebHostEnvironment environment = builder.Environment;

//IConfiguration Configuration = app.Configuration;
//IWebHostEnvironment environment = app.Environment;
var appSettingsSection = Configuration.GetSection("Config");
builder.Services.Configure<AppSettings>(appSettingsSection);

var jwtOptions = Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("La sección de configuración Jwt es obligatoria.");
ValidateJwtOptions(jwtOptions, environment.IsDevelopment());

builder.Services.AddSingleton<IConfiguration>(Configuration);
builder.Services.AddSingleton<IConnectionFactory, ConnectionFactory>();

builder.Services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));

#region Inyeccion Autenticacion
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddSingleton<IRefreshTokenHashService, RefreshTokenHashService>();
builder.Services.AddScoped<IAuthApplication, AuthApplication>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
#endregion

#region Inyeccion Negocios
builder.Services.AddScoped<IBusinessApplication, BusinessApplication>();
builder.Services.AddScoped<IBusinessDomain, BusinessDomain>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
#endregion

#region Inyeccion Clases
builder.Services.AddScoped<IClassApplication, ClassApplication>();
builder.Services.AddScoped<IClassDomain, ClassDomain>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
#endregion

#region Inyeccion Parametros
builder.Services.AddScoped<IParameterApplication, ParameterApplication>();
builder.Services.AddScoped<IParameterDomain, ParameterDomain>();
builder.Services.AddScoped<IParameterRepository, ParameterRepository>();
#endregion

#region Inyeccion RBAC
builder.Services.AddScoped<IRbacRepository, RbacRepository>();
builder.Services.AddScoped<IRbacApplication, RbacApplication>();
#endregion

#region Inyeccion Usuarios
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserApplication, UserApplication>();
#endregion

#region Inyeccion Productos
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductDomain, ProductDomain>();
builder.Services.AddScoped<IProductApplication, ProductApplication>();
#endregion

#region Inyeccion Inventario
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IInventoryDomain, InventoryDomain>();
builder.Services.AddScoped<IInventoryApplication, InventoryApplication>();
#endregion

#region Inyeccion Kardex
builder.Services.AddScoped<IKardexRepository, KardexRepository>();
builder.Services.AddScoped<IKardexDomain, KardexDomain>();
builder.Services.AddScoped<IKardexApplication, KardexApplication>();
#endregion

#region Inyeccion Compras
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<IPurchaseDomain, PurchaseDomain>();
builder.Services.AddScoped<IPurchaseApplication, PurchaseApplication>();
#endregion

#region Inyeccion Ventas
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<ISaleDomain, SaleDomain>();
builder.Services.AddScoped<ISaleApplication, SaleApplication>();
#endregion

#region Inyeccion Terceros
builder.Services.AddScoped<IThirdPartyRepository, ThirdPartyRepository>();
builder.Services.AddScoped<IThirdPartyDomain, ThirdPartyDomain>();
builder.Services.AddScoped<IThirdPartyApplication, ThirdPartyApplication>();
#endregion

#region Inyeccion Dashboard
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardApplication, DashboardApplication>();
#endregion

#region Inyección cloudinary
builder.Services.Configure<CloudinaryOptions>(
    builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
builder.Services.AddScoped<IFileUploadApplication, FileUploadApplication>();
#endregion

var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
        .AddJwtBearer(x =>
        {
            x.MapInboundClaims = false;
            x.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = async context =>
                {
                    if (context.Response.HasStarted)
                        return;

                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(
                        new Response<object>
                        {
                            IsSuccess = false,
                            Data = null,
                            Message = "No autorizado. El access token es inválido o expiró."
                        });
                },
                OnForbidden = async context =>
                {
                    if (context.Response.HasStarted)
                        return;

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(
                        new Response<object>
                        {
                            IsSuccess = false,
                            Data = null,
                            Message = "No tiene permisos para ejecutar esta acción."
                        });
                }
            };
            x.RequireHttpsMetadata = false;
            x.SaveToken = false;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = AuthClaimTypes.Name
            };
        });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

var app = builder.Build();
app.UseRouting();
app.UseCors("AllowSpecificOrigin");

#region Directory Browser
////app.UseDirectoryBrowser(new DirectoryBrowserOptions
////{
////    FileProvider = new PhysicalFileProvider(
////Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
////    RequestPath = "/uploads"
////});

//app.UseStaticFiles();
////app.UseStaticFiles(new StaticFileOptions()
////{
////    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"uploads")),
////    RequestPath = "/uploads"
////});
#endregion

// Configure the HTTP request pipeline.

app.UseAuthentication();

app.UseAuthorization();

//app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<HubMystery>("/HubMystery");
});
#endregion

if (environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UTP POS API V1");
    });
}

app.Run();

static void ValidateJwtOptions(JwtOptions options, bool isDevelopment)
{
    if (string.IsNullOrWhiteSpace(options.Issuer))
        throw new InvalidOperationException("Jwt:Issuer es obligatorio.");

    if (string.IsNullOrWhiteSpace(options.Audience))
        throw new InvalidOperationException("Jwt:Audience es obligatorio.");

    if (Encoding.UTF8.GetByteCount(options.SecretKey ?? string.Empty) < 32)
        throw new InvalidOperationException("Jwt:SecretKey debe tener al menos 32 bytes.");

    if (Encoding.UTF8.GetByteCount(options.RefreshTokenHashKey ?? string.Empty) < 32)
        throw new InvalidOperationException("Jwt:RefreshTokenHashKey debe tener al menos 32 bytes.");

    if (options.AccessTokenExpirationMinutes <= 0)
        throw new InvalidOperationException("Jwt:AccessTokenExpirationMinutes debe ser mayor que cero.");

    if (options.RefreshTokenExpirationDays <= 0)
        throw new InvalidOperationException("Jwt:RefreshTokenExpirationDays debe ser mayor que cero.");

    if (!isDevelopment &&
        (options.SecretKey?.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) == true ||
         options.RefreshTokenHashKey?.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) == true))
    {
        throw new InvalidOperationException(
            "Las claves Jwt de marcador deben reemplazarse mediante configuración segura del entorno.");
    }
}

public partial class Program { }
