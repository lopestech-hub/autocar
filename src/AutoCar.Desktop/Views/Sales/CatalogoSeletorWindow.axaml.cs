using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Desktop.ViewModels.Catalogo;
using AutoCar.Desktop.Views.Catalogo;

namespace AutoCar.Desktop.Views.Sales;

/// <summary>Como a janela do Catálogo é usada.</summary>
public enum ModoCatalogo
{
    /// <summary>Consulta (aberta pela toolbar): só pesquisa, com o painel de detalhe visível.
    /// Não adiciona nada — fecha com "Fechar"/Esc.</summary>
    Consulta,

    /// <summary>Seleção de peça (aberta com F2 na pré-venda): lista densa, sem painel; clicar/Enter
    /// devolve a peça para o documento e fecha.</summary>
    AdicionarNaVenda,
}

/// <summary>
/// Janela que hospeda o Catálogo. Serve a DOIS fluxos pelo <see cref="ModoCatalogo"/>:
/// <list type="bullet">
/// <item><b>AdicionarNaVenda</b> (F2 da pré-venda): clicar/Enter numa peça invoca o callback de
/// seleção e fecha — fluxo de venda, inalterado.</item>
/// <item><b>Consulta</b> (toolbar): só pesquisa, com o painel de detalhe (mestre-detalhe Cofap)
/// visível; sem callback de adicionar.</item>
/// </list>
/// Reusa CatalogoView/CatalogoViewModel (não duplica a busca). Maximizada e não-modal.
/// </summary>
public partial class CatalogoSeletorWindow : Window
{
    private readonly Action<CatalogoItemDto>? _aoSelecionar;
    private ModoCatalogo _modo = ModoCatalogo.AdicionarNaVenda;
    private CatalogoView? _catalogo;

    public CatalogoSeletorWindow()
    {
        InitializeComponent();
    }

    /// <summary>Modo SELEÇÃO (F2 da pré-venda): escolhe uma peça e devolve pelo callback.</summary>
    public CatalogoSeletorWindow(CatalogoViewModel catalogoVm, Action<CatalogoItemDto> aoSelecionar) : this()
    {
        _aoSelecionar = aoSelecionar;
        Configurar(catalogoVm, ModoCatalogo.AdicionarNaVenda);
    }

    /// <summary>Modo CONSULTA (toolbar): só pesquisa, painel de detalhe visível, sem adicionar.</summary>
    public CatalogoSeletorWindow(CatalogoViewModel catalogoVm) : this()
    {
        Configurar(catalogoVm, ModoCatalogo.Consulta);
    }

    private void Configurar(CatalogoViewModel catalogoVm, ModoCatalogo modo)
    {
        _modo = modo;

        _catalogo = this.FindControl<CatalogoView>("Catalogo")!;
        // No modo seletor a CatalogoView esconde o painel de detalhe (lista densa para a venda);
        // na consulta o painel aparece.
        _catalogo.ModoSeletor = modo == ModoCatalogo.AdicionarNaVenda;
        _catalogo.DataContext = catalogoVm;

        if (modo == ModoCatalogo.AdicionarNaVenda)
        {
            _catalogo.PecaSelecionada += peca =>
            {
                _aoSelecionar?.Invoke(peca);
                Close();
            };
            Title = "Catálogo: selecionar peça";
            this.FindControl<TextBlock>("Dica")!.Text =
                "Clique numa peça para adicionar à pré-venda. Esc para fechar.";
        }
        else
        {
            Title = "Catálogo: consulta de peças";
            this.FindControl<TextBlock>("Dica")!.Text =
                "Pesquise por veículo ou peça. Esc para fechar.";
        }

        // Ao abrir, o foco vai para o campo de busca de peça.
        Opened += (_, _) => _catalogo.FocarBusca();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoFechar(object? sender, RoutedEventArgs e) => Close();

    // Teclado: ↑/↓ navegam a lista nos dois modos; Enter confirma só no modo de seleção;
    // Esc sempre fecha.
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
            case Key.Enter when _modo == ModoCatalogo.AdicionarNaVenda:
                // Se há linha destacada, seleciona (a janela fecha pelo callback de PecaSelecionada).
                _catalogo?.SelecionarAtual();
                e.Handled = true;
                break;
        }
    }
}
