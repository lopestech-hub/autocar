using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Janela do formulário de Marca (não-modal). Mesmo padrão da ProdutoWindow:
/// a barra de título é a <see cref="Shared.BarraTituloJanela"/> e o conteúdo é o
/// <see cref="MarcaFormView"/>. Fecha sozinha ao salvar ou cancelar.
/// </summary>
public partial class MarcaWindow : Window
{
    public MarcaWindow()
    {
        InitializeComponent();
    }

    public MarcaWindow(MarcaFormViewModel viewModel) : this()
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
