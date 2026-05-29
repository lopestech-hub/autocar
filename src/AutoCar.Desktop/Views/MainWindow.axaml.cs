using Avalonia.Controls;
using AutoCar.Desktop.ViewModels;

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
            vm.SairSolicitado += FazerLogout;
    }

    // Logout: reabre a janela de login e fecha o shell atual.
    private void FazerLogout()
    {
        var login = new LoginWindow();
        login.Show();
        Close();
    }
}
