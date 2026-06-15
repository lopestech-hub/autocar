using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Sales.PreVendas.DTOs;
using AutoCar.Desktop.ViewModels.Sales;

namespace AutoCar.Desktop.Views.Sales;

/// <summary>
/// Listagem de pré-vendas. Tabela renderizada por <see cref="ListBox"/> virtualizado.
/// Seleção, hover, teclado e scroll vêm do ListBox; Enter/duplo-clique dispara o AbrirCommand
/// (que pede a abertura do formulário em janela separada — ver AbrirJanelaPreVenda).
/// </summary>
public partial class PreVendasView : UserControl
{
    private PreVendasViewModel? _vm;

    public PreVendasView()
    {
        InitializeComponent();

        var lista = this.FindControl<ListBox>("ListaPreVendas");
        if (lista is not null)
        {
            lista.DoubleTapped += AoConfirmarLinha;
            lista.KeyDown += AoTeclarNaLista;
        }

        DataContextChanged += AoTrocarDataContext;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoTrocarDataContext(object? sender, System.EventArgs e)
    {
        if (DataContext is not PreVendasViewModel vm)
            return;

        _vm = vm;
        vm.AbrirJanelaSolicitado += AbrirJanelaPreVenda;

        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
    }

    /// <summary>
    /// Abre o formulário de pré-venda numa janela separada maximizada (não-modal): o shell
    /// principal continua acessível. Ao fechar com sucesso (Salvo), recarrega a listagem.
    /// </summary>
    private void AbrirJanelaPreVenda(PreVendaFormViewModel form)
    {
        form.Salvo += () => _vm?.RecarregarAsync();
        form.Faturado += () => _vm?.RecarregarAsync();

        var janela = new PreVendaWindow(form);
        var dono = TopLevel.GetTopLevel(this) as Window;
        if (dono is not null)
            janela.Show(dono); // dona = shell; não-modal, mantém o principal acessível
        else
            janela.Show();
    }

    private void AoTeclarNaLista(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AoConfirmarLinha(sender, e);
            e.Handled = true;
        }
    }

    private void AoConfirmarLinha(object? sender, RoutedEventArgs e)
    {
        if (sender is ListBox { SelectedItem: PreVendaListaDto dto }
            && _vm?.AbrirCommand.CanExecute(dto) == true)
            _vm.AbrirCommand.Execute(dto);
    }
}
