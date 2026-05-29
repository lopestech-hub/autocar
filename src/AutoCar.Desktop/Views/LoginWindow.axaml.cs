using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Security.DTOs;
using AutoCar.Desktop.Navegacao;
using AutoCar.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Desktop.Views;

/// <summary>
/// Janela de login. Ao autenticar, abre a janela principal com o usuário logado
/// e fecha a si mesma.
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();

        var vm = App.Services.GetRequiredService<LoginViewModel>();
        vm.LoginConcluido += AoConcluirLogin;
        DataContext = vm;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoConcluirLogin(UsuarioLogado usuario)
    {
        var navegador = App.Services.GetRequiredService<INavegador>();
        var principal = new MainWindow
        {
            DataContext = new MainWindowViewModel(usuario, navegador),
        };
        principal.Show();
        Close();
    }
}
