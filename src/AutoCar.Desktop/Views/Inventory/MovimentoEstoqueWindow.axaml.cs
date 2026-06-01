using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Inventory;

namespace AutoCar.Desktop.Views.Inventory;

/// <summary>
/// Janela separada (não-modal, tamanho fixo) que hospeda o formulário de movimentação de estoque de
/// um produto. O shell principal continua aberto e acessível. A janela fecha sozinha quando o
/// movimento é concluído ou cancelado.
/// </summary>
public partial class MovimentoEstoqueWindow : Window
{
    public MovimentoEstoqueWindow()
    {
        InitializeComponent();
    }

    public MovimentoEstoqueWindow(MovimentoEstoqueFormViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.Concluido += Close;
        viewModel.Cancelado += Close;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
