using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Listagem de posições da peça. Tabela renderizada por <see cref="ListBox"/> virtualizado.
/// Seleção, hover, teclado (↑/↓) e scroll vêm do ListBox; só Enter/duplo-clique → abrir o form fica aqui.
/// </summary>
public partial class PosicoesView : UserControl
{
    private PosicoesViewModel? _vm;

    public PosicoesView()
    {
        InitializeComponent();

        var lista = this.FindControl<ListBox>("ListaPosicoes");
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
        if (DataContext is not PosicoesViewModel vm)
            return;

        _vm = vm;

        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
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
        if (sender is ListBox { SelectedItem: PosicaoPecaDto dto }
            && _vm?.AbrirCommand.CanExecute(dto) == true)
            _vm.AbrirCommand.Execute(dto);
    }
}
