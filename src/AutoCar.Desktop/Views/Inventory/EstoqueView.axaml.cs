using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Estoque.DTOs;
using AutoCar.Desktop.ViewModels.Inventory;

namespace AutoCar.Desktop.Views.Inventory;

/// <summary>
/// Listagem de saldos de estoque. Tabela renderizada por <see cref="ListBox"/> virtualizado.
/// Seleção, hover, teclado e scroll vêm do ListBox; Enter/duplo-clique dispara o MovimentarCommand
/// (que pede a abertura da janela de movimentação — ver AbrirJanelaMovimentacao).
/// </summary>
public partial class EstoqueView : UserControl
{
    private EstoqueViewModel? _vm;

    public EstoqueView()
    {
        InitializeComponent();

        var lista = this.FindControl<ListBox>("ListaSaldos");
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
        if (DataContext is not EstoqueViewModel vm)
            return;

        _vm = vm;
        vm.AbrirMovimentacaoSolicitado += AbrirJanelaMovimentacao;

        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
    }

    /// <summary>
    /// Abre a movimentação de estoque numa janela separada (não-modal): o shell principal continua
    /// acessível. Ao concluir o movimento (Concluido), recarrega a listagem para refletir o saldo novo.
    /// </summary>
    private void AbrirJanelaMovimentacao(MovimentoEstoqueFormViewModel form)
    {
        form.Concluido += () => _vm?.RecarregarAsync();

        var janela = new MovimentoEstoqueWindow(form);
        var dono = TopLevel.GetTopLevel(this) as Window;
        if (dono is not null)
            janela.Show(dono);
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
        if (sender is ListBox { SelectedItem: SaldoEstoqueListaDto dto }
            && _vm?.MovimentarCommand.CanExecute(dto) == true)
            _vm.MovimentarCommand.Execute(dto);
    }
}
