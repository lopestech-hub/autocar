using System;
using System.IO;
using AutoCar.Application;
using AutoCar.Desktop.ViewModels;
using AutoCar.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace AutoCar.Desktop;

/// <summary>
/// Composition root da aplicação desktop. Monta configuração (appsettings.json),
/// Serilog e o contêiner de DI com as camadas Application e Infrastructure.
/// </summary>
public static class Bootstrap
{
    public static IServiceProvider ConstruirServicos()
    {
        var basePath = AppContext.BaseDirectory;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        Log.Logger = logger;

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default não configurada.");

        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder => builder.AddSerilog(logger, dispose: true));

        services.AddApplication();
        services.AddInfrastructure(connectionString);

        // ViewModels resolvidos por DI.
        // MainWindowViewModel não entra aqui: depende do UsuarioLogado, criado em runtime
        // após o login (ver LoginWindow.AoConcluirLogin).
        services.AddTransient<LoginViewModel>();

        return services.BuildServiceProvider();
    }
}
