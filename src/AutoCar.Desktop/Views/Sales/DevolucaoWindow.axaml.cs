using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.ViewModels.Sales;
using AutoCar.Desktop.Views;

namespace AutoCar.Desktop.Views.Sales;

/// <summary>
/// Janela separada (não-modal) que hospeda o formulário de devolução de uma venda. O shell principal
/// segue acessível. Antes de registrar a devolução (ação que repõe estoque), pede confirmação via
/// ConfirmacaoWindow. A janela fecha sozinha ao concluir ou cancelar.
/// </summary>
public partial class DevolucaoWindow : Window
{
    private DevolucaoFormViewModel? _vm;

    public DevolucaoWindow()
    {
        InitializeComponent();
    }

    public DevolucaoWindow(DevolucaoFormViewModel viewModel) : this()
    {
        _vm = viewModel;
        DataContext = viewModel;
        viewModel.Concluido += Close;
        viewModel.Cancelado += Close;
        viewModel.ConfirmacaoDevolucaoSolicitada += ConfirmarDevolucao;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Confirmação antes de registrar a devolução (repõe o estoque). Se confirmado, o ViewModel
    /// efetiva; em sucesso, ele dispara Concluido e a janela fecha.
    /// </summary>
    private async void ConfirmarDevolucao()
    {
        if (_vm is null)
            return;

        var confirmado = await ConfirmacaoWindow.MostrarAsync(
            this,
            "Registrar devolução",
            "Os itens selecionados voltarão ao estoque e a devolução será registrada. Deseja continuar?",
            textoConfirmar: "Devolver");

        if (confirmado)
            await _vm.ConfirmarDevolucaoAsync();
    }
}
