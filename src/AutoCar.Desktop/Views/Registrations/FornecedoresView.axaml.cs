using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Listagem de fornecedores. Tabela renderizada por <see cref="ListBox"/> virtualizado.
/// Seleção, hover, teclado (↑/↓) e scroll vêm do ListBox; Enter/duplo-clique abre o form.
/// Badge PF/PJ e máscara de documento são feitos por converters no ItemTemplate.
/// </summary>
public partial class FornecedoresView : UserControl
{
    private FornecedoresViewModel? _vm;

    public FornecedoresView()
    {
        InitializeComponent();

        var lista = this.FindControl<ListBox>("ListaFornecedores");
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
        if (DataContext is not FornecedoresViewModel vm)
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
        if (sender is ListBox { SelectedItem: FornecedorListaDto dto }
            && _vm?.AbrirCommand.CanExecute(dto) == true)
            _vm.AbrirCommand.Execute(dto);
    }
}
