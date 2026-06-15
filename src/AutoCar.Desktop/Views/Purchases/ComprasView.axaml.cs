using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Purchases.Compras.DTOs;
using AutoCar.Desktop.ViewModels.Purchases;

namespace AutoCar.Desktop.Views.Purchases;

/// <summary>
/// Listagem de compras. Tabela renderizada por <see cref="ListBox"/> virtualizado.
/// Seleção, hover, teclado e scroll vêm do ListBox; Enter/duplo-clique dispara o AbrirCommand
/// (que pede a abertura do formulário em janela separada — ver AbrirJanelaCompra).
/// </summary>
public partial class ComprasView : UserControl
{
    private ComprasViewModel? _vm;

    public ComprasView()
    {
        InitializeComponent();

        var lista = this.FindControl<ListBox>("ListaCompras");
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
        if (DataContext is not ComprasViewModel vm)
            return;

        _vm = vm;
        vm.AbrirJanelaSolicitado += AbrirJanelaCompra;

        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
    }

    /// <summary>
    /// Abre o formulário de compra numa janela separada maximizada (não-modal): o shell principal
    /// continua acessível. Ao fechar com sucesso (Salvo), recarrega a listagem.
    /// </summary>
    private void AbrirJanelaCompra(CompraFormViewModel form)
    {
        form.Salvo += () => _vm?.RecarregarAsync();

        var janela = new CompraWindow(form);
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
        if (sender is ListBox { SelectedItem: CompraListaDto dto }
            && _vm?.AbrirCommand.CanExecute(dto) == true)
            _vm.AbrirCommand.Execute(dto);
    }
}
