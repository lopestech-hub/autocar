using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Janela do formulário de Cliente (não-modal). Mesmo padrão da MarcaWindow:
/// barra de título customizada + <see cref="ClienteFormView"/>. Fecha ao salvar ou cancelar.
/// </summary>
public partial class ClienteWindow : Window
{
    public ClienteWindow()
    {
        InitializeComponent();
    }

    public ClienteWindow(ClienteFormViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.Salvo += Close;
        viewModel.Cancelado += Close;
        Closed += (_, _) =>
        {
            viewModel.Salvo -= Close;
            viewModel.Cancelado -= Close;
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
