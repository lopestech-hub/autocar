using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Service.OrdensServico.DTOs;
using AutoCar.Desktop.ViewModels.Service;

namespace AutoCar.Desktop.Views.Service;

/// <summary>
/// Listagem de ordens de serviço. Tabela renderizada por <see cref="ListBox"/> virtualizado.
/// Seleção, hover, teclado e scroll vêm do ListBox; Enter/duplo-clique dispara o AbrirCommand
/// (que pede a abertura do formulário em janela separada — ver AbrirJanelaOrdemServico).
/// </summary>
public partial class OrdensServicoView : UserControl
{
    private OrdensServicoViewModel? _vm;

    public OrdensServicoView()
    {
        InitializeComponent();

        var lista = this.FindControl<ListBox>("ListaOrdens");
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
        if (DataContext is not OrdensServicoViewModel vm)
            return;

        _vm = vm;
        vm.AbrirJanelaSolicitado += AbrirJanelaOrdemServico;

        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
    }

    /// <summary>
    /// Abre o formulário da OS numa janela separada maximizada (não-modal): o shell principal
    /// continua acessível. Ao salvar (Salvo) ou faturar (Faturado), recarrega a listagem.
    /// </summary>
    private void AbrirJanelaOrdemServico(OrdemServicoFormViewModel form)
    {
        form.Salvo += () => _vm?.RecarregarAsync();
        form.Faturado += () => _vm?.RecarregarAsync();

        var janela = new OrdemServicoWindow(form);
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
        if (sender is ListBox { SelectedItem: OrdemServicoListaDto dto }
            && _vm?.AbrirCommand.CanExecute(dto) == true)
            _vm.AbrirCommand.Execute(dto);
    }
}
