using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AutoCar.Desktop.Views.Sales;

/// <summary>
/// Formulário de devolução de venda (hospedado pela DevolucaoWindow): cabeçalho, motivo e o grid de
/// itens devolvíveis com a quantidade a devolver editável por linha.
/// </summary>
public partial class DevolucaoFormView : UserControl
{
    public DevolucaoFormView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
