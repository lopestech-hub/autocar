using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Janela do formulário de Serviço (não-modal). Mesmo padrão da MarcaWindow:
/// barra de título customizada + <see cref="ServicoFormView"/>. Fecha ao salvar ou cancelar.
/// </summary>
public partial class ServicoWindow : Window
{
    public ServicoWindow()
    {
        InitializeComponent();
    }

    public ServicoWindow(ServicoFormViewModel viewModel) : this()
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
