using Avalonia;
using System;
using AutoCar.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Serilog;

namespace AutoCar.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Registra o provider de ícones FontAwesome (usado nos atalhos da toolbar).
        IconProvider.Current.Register<FontAwesomeIconProvider>();

        // Monta DI/Serilog e inicializa o banco (migrations + seed admin) ANTES de
        // subir a UI — fora do contexto STA do Avalonia, evitando deadlock async.
        App.Services = Bootstrap.ConstruirServicos();
        InicializarBanco();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void InicializarBanco()
    {
        try
        {
            using var scope = App.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<DbInitializer>().Inicializar();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Falha ao inicializar o banco de dados.");
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
