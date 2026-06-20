using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Janela do formulário de Fornecedor (não-modal). Mesmo padrão da MarcaWindow:
/// barra de título customizada + <see cref="FornecedorFormView"/>. Fecha ao salvar ou cancelar.
/// </summary>
public partial class FornecedorWindow : Window
{
    public FornecedorWindow()
    {
        InitializeComponent();
    }

    public FornecedorWindow(FornecedorFormViewModel viewModel) : this()
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
