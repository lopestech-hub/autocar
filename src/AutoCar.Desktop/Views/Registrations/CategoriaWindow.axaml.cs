using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Janela do formulário de Categoria (não-modal). Mesmo padrão da MarcaWindow:
/// barra de título customizada + <see cref="CategoriaFormView"/>. Fecha ao salvar ou cancelar.
/// </summary>
public partial class CategoriaWindow : Window
{
    public CategoriaWindow()
    {
        InitializeComponent();
    }

    public CategoriaWindow(CategoriaFormViewModel viewModel) : this()
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
