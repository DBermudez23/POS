using System.Reflection;
using System.Text;
using AutoMapper;
using UtpPos.Console.Tools.Menus;
using UtpPos.Console.Tools.Runners;
using UtpPos.Framework.Application.Interface.Rbac;
using UtpPos.Framework.Application.Main.Rbac;
using UtpPos.Framework.Application.Interface.User;
using UtpPos.Framework.Application.Main.User;
using UtpPos.Framework.InfraStructure.Data;
using UtpPos.Framework.InfraStructure.Interface.Rbac;
using UtpPos.Framework.InfraStructure.Repository.Rbac;
using UtpPos.Framework.InfraStructure.Interface.User;
using UtpPos.Framework.InfraStructure.Repository.User;
using UtpPos.Framework.Transversal.Common;
using UtpPos.Framework.Transversal.Common.Auth;
using UtpPos.Framework.Transversal.Interface;
using UtpPos.Framework.Transversal.Mapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

System.Console.OutputEncoding = Encoding.UTF8;

var webApiDirectory = FindWebApiDirectory();
var configuration = new ConfigurationBuilder()
    .SetBasePath(webApiDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
{
    throw new InvalidOperationException(
        "No se encontró la connection string 'DefaultConnection' en UtpPos.WebAPI o en las variables de entorno.");
}

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<IConnectionFactory, ConnectionFactory>();
services.AddSingleton<IMapper>(_ =>
{
    var mapperConfiguration = new MapperConfiguration(config =>
        config.AddMaps(Assembly.GetAssembly(typeof(MappingProfile))));
    return mapperConfiguration.CreateMapper();
});
services.AddScoped<IRbacRepository, RbacRepository>();
services.AddScoped<IRbacApplication, RbacApplication>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IUserApplication, UserApplication>();
services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
services.AddScoped<MainMenu>();
services.AddScoped<ModuleRunner>();
services.AddScoped<ActionRunner>();
services.AddScoped<PermissionRunner>();
services.AddScoped<UserRunner>();
services.AddScoped<RoleRunner>();
services.AddScoped<RolePermissionRunner>();
services.AddScoped<UserRoleRunner>();
services.AddScoped<UserPermissionRunner>();

using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateOnBuild = true,
    ValidateScopes = true
});
using var scope = serviceProvider.CreateScope();
var mainMenu = scope.ServiceProvider.GetRequiredService<MainMenu>();
await mainMenu.RunAsync();

static string FindWebApiDirectory()
{
    var startingDirectories = new[]
    {
        new DirectoryInfo(Directory.GetCurrentDirectory()),
        new DirectoryInfo(AppContext.BaseDirectory)
    };

    foreach (var startingDirectory in startingDirectories)
    {
        for (var directory = startingDirectory; directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "UtpPos.WebAPI");
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
                return candidate;
        }
    }

    throw new DirectoryNotFoundException(
        "No se encontró ../UtpPos.WebAPI/appsettings.json desde el directorio del proyecto o de ejecución.");
}
