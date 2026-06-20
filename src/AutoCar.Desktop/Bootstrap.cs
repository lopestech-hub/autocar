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
using AutoCar.Desktop.ViewModels.Service;
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
        services.AddTransient<Func<ClienteFormViewModel>>(sp => () => sp.GetRequiredService<ClienteFormViewModel>());
        services.AddTransient<FornecedoresViewModel>();
        services.AddTransient<FornecedorFormViewModel>();
        services.AddTransient<Func<FornecedorFormViewModel>>(sp => () => sp.GetRequiredService<FornecedorFormViewModel>());
        services.AddTransient<MarcasViewModel>();
        services.AddTransient<MarcaFormViewModel>();
        services.AddTransient<Func<MarcaFormViewModel>>(sp => () => sp.GetRequiredService<MarcaFormViewModel>());
        services.AddTransient<CategoriasViewModel>();
        services.AddTransient<CategoriaFormViewModel>();
        services.AddTransient<Func<CategoriaFormViewModel>>(sp => () => sp.GetRequiredService<CategoriaFormViewModel>());
        services.AddTransient<GruposViewModel>();
        services.AddTransient<GrupoFormViewModel>();
        services.AddTransient<Func<GrupoFormViewModel>>(sp => () => sp.GetRequiredService<GrupoFormViewModel>());
        services.AddTransient<PosicoesViewModel>();
        services.AddTransient<PosicaoFormViewModel>();
        services.AddTransient<Func<PosicaoFormViewModel>>(sp => () => sp.GetRequiredService<PosicaoFormViewModel>());
        services.AddTransient<LadosViewModel>();
        services.AddTransient<LadoFormViewModel>();
        services.AddTransient<Func<LadoFormViewModel>>(sp => () => sp.GetRequiredService<LadoFormViewModel>());
        services.AddTransient<ServicosViewModel>();
        services.AddTransient<ServicoFormViewModel>();
        services.AddTransient<Func<ServicoFormViewModel>>(sp => () => sp.GetRequiredService<ServicoFormViewModel>());
        services.AddTransient<MecanicosViewModel>();
        services.AddTransient<MecanicoFormViewModel>();
        services.AddTransient<Func<MecanicoFormViewModel>>(sp => () => sp.GetRequiredService<MecanicoFormViewModel>());
        services.AddTransient<ProdutosViewModel>();
        services.AddTransient<ProdutoFormViewModel>();
        // Factory do form de produto: cada janela recebe um ViewModel novo (limpo), permitindo o
        // mesmo padrão não-modal do estoque/pré-venda sem compartilhar estado entre janelas.
        services.AddTransient<Func<ProdutoFormViewModel>>(sp => () => sp.GetRequiredService<ProdutoFormViewModel>());
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
        // OrdensServicoViewModel não entra no DI: depende do UsuarioLogado (runtime, registra o
        // atendente). É montado pelo Navegador. O form, sim, vem do DI.
        services.AddTransient<OrdemServicoFormViewModel>();

        return services.BuildServiceProvider();
    }
}
