using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Listagem de produtos. Tabela renderizada por <see cref="ListBox"/> virtualizado
/// (só desenha as linhas visíveis — não trava com milhares de produtos). Seleção, hover,
/// teclado (↑/↓) e scroll vêm do ListBox; só Enter/duplo-clique → abrir o form fica aqui.
/// </summary>
public partial class ProdutosView : UserControl
{
    private ProdutosViewModel? _vm;

    public ProdutosView()
    {
        InitializeComponent();

        // Handlers do ListBox ligados UMA vez (o controle já existe após InitializeComponent).
        // Não ligar em DataContextChanged — lá acumularia registros a cada troca de DataContext.
        var lista = this.FindControl<ListBox>("ListaProdutos");
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
        if (DataContext is not ProdutosViewModel vm)
            return;

        _vm = vm;

        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
    }

    // Enter abre o form da linha marcada (↑/↓ e a marca visual já são nativos do ListBox).
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
        if (sender is ListBox { SelectedItem: ProdutoListaDto dto }
            && _vm?.AbrirCommand.CanExecute(dto) == true)
            _vm.AbrirCommand.Execute(dto);
    }
}
