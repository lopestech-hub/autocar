using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels;
using AutoCar.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Desktop;

public partial class App : Avalonia.Application
{
    /// <summary>Provider de DI da aplicação. Definido em Program.Main antes da UI subir.</summary>
    public static IServiceProvider Services { get; set; } = default!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // A aplicação inicia na tela de login. A MainWindow é aberta pela
            // própria LoginWindow após autenticação bem-sucedida.
            desktop.MainWindow = new LoginWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
