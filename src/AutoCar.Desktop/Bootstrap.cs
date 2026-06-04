using System;
using System.IO;
using AutoCar.Application;
using AutoCar.Desktop.Navegacao;
using AutoCar.Desktop.ViewModels;
using AutoCar.Desktop.ViewModels.Catalogo;
using AutoCar.Desktop.ViewModels.Inventory;
using AutoCar.Desktop.ViewModels.Registrations;
using AutoCar.Desktop.ViewModels.Purchases;
using AutoCar.Desktop.ViewModels.Sales;
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

        // Navegação do shell (resolve rota → ViewModel da tela).
        services.AddSingleton<INavegador, Navegador>();

        // ViewModels resolvidos por DI.
        // MainWindowViewModel não entra aqui: depende do UsuarioLogado, criado em runtime
        // após o login (ver LoginWindow.AoConcluirLogin).
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ClientesViewModel>();
        services.AddTransient<ClienteFormViewModel>();
        services.AddTransient<FornecedoresViewModel>();
        services.AddTransient<FornecedorFormViewModel>();
        services.AddTransient<MarcasViewModel>();
        services.AddTransient<MarcaFormViewModel>();
        services.AddTransient<CategoriasViewModel>();
        services.AddTransient<CategoriaFormViewModel>();
        services.AddTransient<ServicosViewModel>();
        services.AddTransient<ServicoFormViewModel>();
        services.AddTransient<MecanicosViewModel>();
        services.AddTransient<MecanicoFormViewModel>();
        services.AddTransient<ProdutosViewModel>();
        services.AddTransient<ProdutoFormViewModel>();
        services.AddTransient<CatalogoViewModel>();
        // PreVendasViewModel não entra no DI: depende do UsuarioLogado (runtime). É montado
        // pelo Navegador. O form, sim, vem do DI (não depende do usuário diretamente).
        services.AddTransient<PreVendaFormViewModel>();
        services.AddTransient<DevolucaoFormViewModel>();
        // EstoqueViewModel não entra no DI: depende do UsuarioLogado (runtime, registra quem
        // movimentou). É montado pelo Navegador. O form de movimentação, sim, vem do DI.
        services.AddTransient<MovimentoEstoqueFormViewModel>();
        // ComprasViewModel não entra no DI: depende do UsuarioLogado (runtime, registra quem
        // fez a compra). É montado pelo Navegador. O form, sim, vem do DI.
        services.AddTransient<CompraFormViewModel>();

        return services.BuildServiceProvider();
    }
}
