using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Janela do formulário de Grupo de produto (não-modal). Mesmo padrão da MarcaWindow:
/// barra de título customizada + <see cref="GrupoFormView"/>. Fecha ao salvar ou cancelar.
/// </summary>
public partial class GrupoWindow : Window
{
    public GrupoWindow()
    {
        InitializeComponent();
    }

    public GrupoWindow(GrupoFormViewModel viewModel) : this()
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
