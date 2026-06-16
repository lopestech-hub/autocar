using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Listagem de lados da peça. Tabela renderizada por <see cref="ListBox"/> virtualizado.
/// Seleção, hover, teclado (↑/↓) e scroll vêm do ListBox; só Enter/duplo-clique → abrir o form fica aqui.
/// </summary>
public partial class LadosView : UserControl
{
    private LadosViewModel? _vm;

    public LadosView()
    {
        InitializeComponent();

        var lista = this.FindControl<ListBox>("ListaLados");
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
        if (DataContext is not LadosViewModel vm)
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
        if (sender is ListBox { SelectedItem: LadoPecaDto dto }
            && _vm?.AbrirCommand.CanExecute(dto) == true)
            _vm.AbrirCommand.Execute(dto);
    }
}
