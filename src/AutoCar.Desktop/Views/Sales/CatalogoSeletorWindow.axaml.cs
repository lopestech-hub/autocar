using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Desktop.ViewModels.Catalogo;
using AutoCar.Desktop.Views.Catalogo;

namespace AutoCar.Desktop.Views.Sales;

/// <summary>
/// Janela seletora de peça (aberta com F2 na pré-venda). Hospeda o Catálogo completo em modo
/// seletor — filtros de veículo + busca de peça + lista. Clicar numa peça invoca o callback de
/// seleção e fecha a janela. Reusa CatalogoView/CatalogoViewModel (não duplica a busca).
/// </summary>
public partial class CatalogoSeletorWindow : Window
{
    private readonly Action<CatalogoItemDto>? _aoSelecionar;
    private CatalogoView? _catalogo;

    public CatalogoSeletorWindow()
    {
        InitializeComponent();
    }

    public CatalogoSeletorWindow(CatalogoViewModel catalogoVm, Action<CatalogoItemDto> aoSelecionar) : this()
    {
        _aoSelecionar = aoSelecionar;

        _catalogo = this.FindControl<CatalogoView>("Catalogo")!;
        _catalogo.ModoSeletor = true;
        _catalogo.DataContext = catalogoVm;
        _catalogo.PecaSelecionada += peca =>
        {
            _aoSelecionar?.Invoke(peca);
            Close();
        };

        // Ao abrir, o foco vai para o campo de busca de peça (vendedor digita o termo, depois ↓).
        Opened += (_, _) => _catalogo.FocarBusca();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoFechar(object? sender, RoutedEventArgs e) => Close();

    // Teclado do seletor: ↑/↓ navegam a lista, Enter confirma, Esc fecha sem selecionar.
    private void AoTeclar(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
            case Key.Down:
                _catalogo?.NavegarBaixo();
                e.Handled = true;
                break;
            case Key.Up:
                _catalogo?.NavegarCima();
                e.Handled = true;
                break;
            case Key.Enter:
                // Se há linha destacada, seleciona (a janela fecha pelo callback de PecaSelecionada).
                _catalogo?.SelecionarAtual();
                e.Handled = true;
                break;
        }
    }
}
