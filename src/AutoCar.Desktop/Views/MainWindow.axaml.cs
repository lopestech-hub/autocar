using Avalonia.Controls;
using AutoCar.Desktop.ViewModels;
using AutoCar.Desktop.ViewModels.Catalogo;
using AutoCar.Desktop.Views.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += AoTrocarDataContext;
    }

    private void AoTrocarDataContext(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SairSolicitado += FazerLogout;
            vm.AbrirJanelaSolicitado += AbrirEmJanela;
        }
    }

    // Módulos marcados com FlgAbreEmJanela abrem em janela própria maximizada (não embutem no shell).
    private void AbrirEmJanela(string rota)
    {
        switch (rota)
        {
            case "catalogo":
                // Catálogo em CONSULTA (só pesquisa, com painel de detalhe). VM novo = tela limpa.
                var catalogoVm = App.Services.GetRequiredService<CatalogoViewModel>();
                new CatalogoSeletorWindow(catalogoVm).Show(this);
                break;
        }
    }

    // Logout: reabre a janela de login e fecha o shell atual.
    private void FazerLogout()
    {
        var login = new LoginWindow();
        login.Show();
        Close();
    }
}
