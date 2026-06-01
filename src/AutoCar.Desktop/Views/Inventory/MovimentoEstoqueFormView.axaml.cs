using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AutoCar.Desktop.Views.Inventory;

/// <summary>
/// Formulário de movimentação de estoque (hospedado pela MovimentoEstoqueWindow): saldo atual,
/// campos do movimento (tipo/quantidade/observação) e histórico de movimentos do produto.
/// </summary>
public partial class MovimentoEstoqueFormView : UserControl
{
    public MovimentoEstoqueFormView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
