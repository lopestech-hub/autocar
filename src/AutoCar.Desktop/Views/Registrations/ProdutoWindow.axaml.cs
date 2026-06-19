using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Janela separada (não-modal) que hospeda o formulário de produto, no mesmo padrão da janela de
/// movimentação de estoque: a listagem de produtos continua aberta atrás. Fecha sozinha ao salvar
/// ou cancelar. A barra de título azul é o controle <c>BarraTituloJanela</c>.
/// </summary>
public partial class ProdutoWindow : Window
{
    public ProdutoWindow()
    {
        InitializeComponent();
    }

    public ProdutoWindow(ProdutoFormViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.Salvo += Close;
        viewModel.Cancelado += Close;
        // O form é descartado junto com a janela; soltar os handlers evita reter a instância.
        Closed += (_, _) =>
        {
            viewModel.Salvo -= Close;
            viewModel.Cancelado -= Close;
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
