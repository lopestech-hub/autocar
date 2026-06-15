using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Mecanicos.DTOs;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Listagem de mecânicos. Tabela renderizada por <see cref="ListBox"/> virtualizado.
/// Seleção, hover, teclado (↑/↓) e scroll vêm do ListBox; Enter/duplo-clique abre o form.
/// </summary>
public partial class MecanicosView : UserControl
{
    private MecanicosViewModel? _vm;

    public MecanicosView()
    {
        InitializeComponent();

        var lista = this.FindControl<ListBox>("ListaMecanicos");
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
        if (DataContext is not MecanicosViewModel vm)
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
        if (sender is ListBox { SelectedItem: MecanicoDto dto }
            && _vm?.AbrirCommand.CanExecute(dto) == true)
            _vm.AbrirCommand.Execute(dto);
    }
}
