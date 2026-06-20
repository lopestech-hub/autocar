using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Janela do formulário de Mecânico (não-modal). Mesmo padrão da MarcaWindow:
/// barra de título customizada + <see cref="MecanicoFormView"/>. Fecha ao salvar ou cancelar.
/// </summary>
public partial class MecanicoWindow : Window
{
    public MecanicoWindow()
    {
        InitializeComponent();
    }

    public MecanicoWindow(MecanicoFormViewModel viewModel) : this()
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
