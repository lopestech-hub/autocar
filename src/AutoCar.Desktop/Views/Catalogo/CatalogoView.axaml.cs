using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Desktop.ViewModels.Catalogo;

namespace AutoCar.Desktop.Views.Catalogo;

/// <summary>
/// Catálogo automotivo: resultado da busca peça×veículo. Tabela renderizada por
/// <see cref="ListBox"/> virtualizado (só desenha as linhas visíveis — não trava com muitas peças).
/// Funciona em dois modos: consulta (toolbar) e seletor (pré-venda F2). A API pública
/// (FocarBusca/NavegarBaixo/NavegarCima/SelecionarAtual + evento PecaSelecionada) é consumida
/// pela CatalogoSeletorWindow e foi preservada — só a implementação interna mudou.
/// </summary>
public partial class CatalogoView : UserControl
{
    private CatalogoViewModel? _vm;
    private ListBox? _lista;
    private Control? _painelDetalhe;
    private bool _modoSeletor;

    /// <summary>
    /// Quando true, a tela funciona como SELETOR de peça (usada na pré-venda via F2): duplo-clique
    /// ou Enter dispara <see cref="PecaSelecionada"/> e a 1ª linha já vem marcada ao abrir.
    /// Quando false, é só consulta (clique marca a linha, sem ação).
    /// O painel de detalhe (mestre-detalhe estilo Cofap) só aparece na CONSULTA — no seletor a tela
    /// é lista densa pura, para não atrapalhar o fluxo rápido de venda por teclado.
    /// </summary>
    public bool ModoSeletor
    {
        get => _modoSeletor;
        set { _modoSeletor = value; AplicarModo(); }
    }

    // Esconde o painel de detalhe no modo seletor (a tabela ocupa tudo). Idempotente e seguro de
    // chamar antes de o painel existir (o setter de ModoSeletor pode rodar antes do load).
    private void AplicarModo()
    {
        if (_painelDetalhe is not null)
            _painelDetalhe.IsVisible = !_modoSeletor;
    }

    /// <summary>Disparado ao escolher uma peça no modo seletor (duplo-clique ou Enter).</summary>
    public event System.Action<CatalogoItemDto>? PecaSelecionada;

    public CatalogoView()
    {
        InitializeComponent();

        // DoubleTapped ligado UMA vez (o ListBox já existe após InitializeComponent).
        _lista = this.FindControl<ListBox>("ListaResultados");
        if (_lista is not null)
            _lista.DoubleTapped += AoConfirmarLinha;

        // Painel de detalhe (mestre-detalhe). Reaplica o modo: se ModoSeletor já foi setado antes
        // do load, agora que o painel existe a visibilidade é ajustada.
        _painelDetalhe = this.FindControl<Control>("PainelDetalhe");
        AplicarModo();

        DataContextChanged += AoTrocarDataContext;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoTrocarDataContext(object? sender, System.EventArgs e)
    {
        if (DataContext is not CatalogoViewModel vm)
            return;

        // Desinscreve do VM anterior antes de trocar (evita acúmulo se a View for reusada).
        if (_vm is not null)
            _vm.Resultados.CollectionChanged -= AoMudarResultados;

        _vm = vm;
        _vm.Resultados.CollectionChanged += AoMudarResultados;

        if (vm.InicializarCommand.CanExecute(null))
            vm.InicializarCommand.Execute(null);
    }

    // Ao chegarem resultados, pré-seleciona a 1ª linha. No seletor é o fluxo de teclado imediato;
    // na consulta, deixa o painel de detalhe já preenchido com a 1ª peça.
    private void AoMudarResultados(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_lista is not null && _vm?.Resultados.Count > 0 && _lista.SelectedIndex < 0)
            _lista.SelectedIndex = 0;
    }

    // ===== API pública consumida pela CatalogoSeletorWindow (modo seletor) =====

    /// <summary>Coloca o foco no campo de busca de peça (chamado ao abrir o seletor).</summary>
    public void FocarBusca() => this.FindControl<TextBox>("CampoPeca")?.Focus();

    /// <summary>Move a marca uma linha para baixo (seta ↓).</summary>
    public void NavegarBaixo()
    {
        if (_lista is null || _lista.ItemCount == 0) return;
        _lista.SelectedIndex = System.Math.Min(_lista.SelectedIndex + 1, _lista.ItemCount - 1);
        GarantirVisivel();
    }

    /// <summary>Move a marca uma linha para cima (seta ↑).</summary>
    public void NavegarCima()
    {
        if (_lista is null || _lista.ItemCount == 0) return;
        _lista.SelectedIndex = System.Math.Max(_lista.SelectedIndex - 1, 0);
        GarantirVisivel();
    }

    /// <summary>Confirma a linha marcada (Enter). Retorna true se selecionou algo.</summary>
    public bool SelecionarAtual()
    {
        if (_lista?.SelectedItem is CatalogoItemDto peca)
        {
            PecaSelecionada?.Invoke(peca);
            return true;
        }
        return false;
    }

    private void GarantirVisivel()
    {
        if (_lista?.SelectedItem is { } item)
            _lista.ScrollIntoView(item);
    }

    // Duplo-clique (e Enter via SelecionarAtual) adiciona a peça — só no modo seletor.
    private void AoConfirmarLinha(object? sender, RoutedEventArgs e)
    {
        if (ModoSeletor && sender is ListBox { SelectedItem: CatalogoItemDto peca })
            PecaSelecionada?.Invoke(peca);
    }

    // Clique na miniatura da foto (painel de detalhe) abre a imagem ampliada numa janela modal.
    private void AoClicarFoto(object? sender, PointerPressedEventArgs e)
    {
        if (_vm?.PecaSelecionada?.ArquivoImagem is not { } arquivo)
            return;

        var janela = new Views.ImagemAmpliadaWindow(arquivo);
        if (TopLevel.GetTopLevel(this) is Window dono)
            janela.ShowDialog(dono);
        else
            janela.Show();
    }

    // Texto da última célula sob o clique direito. O ContextMenu anexado via Style NÃO popula
    // PlacementTarget no Avalonia 11 (vem nulo), então capturamos o alvo no clique direito.
    private string? _textoCelulaParaCopiar;

    // Clique direito numa linha da tabela: guarda o texto da célula (TextBlock) sob o ponteiro,
    // para o "Copiar" do menu. e.Source é o controle realmente clicado (a célula).
    private void AoClicarCelula(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            _textoCelulaParaCopiar = (e.Source as TextBlock)?.Text;
    }

    // "Copiar" do menu de contexto: copia o texto da célula clicada (capturado em AoClicarCelula).
    // Padrão do Catálogo Cofap (clique direito → Copiar).
    private async void AoCopiarCelula(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_textoCelulaParaCopiar))
            return;

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is not null)
            await clipboard.SetTextAsync(_textoCelulaParaCopiar);
    }
}
