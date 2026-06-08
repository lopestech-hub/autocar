using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Desktop.ViewModels.Catalogo;

namespace AutoCar.Desktop.Views.Catalogo;

/// <summary>
/// Catálogo automotivo: resultado da busca peça×veículo. Tabela como Grid único via
/// code-behind (padrão do projeto — nunca DataGrid).
/// </summary>
public partial class CatalogoView : UserControl
{
    // CÓDIGO · DESCRIÇÃO · APLICAÇÃO (veículos) · POSIÇÃO · COD.FABRIC · UN · VENDA
    private const string ColDefs = "80,*,2*,90,120,50,90";
    private static readonly string[] Headers = { "CÓDIGO", "DESCRIÇÃO", "APLICAÇÃO", "POSIÇÃO", "COD.FABRIC", "UN", "VENDA" };

    private static readonly CultureInfo PtBr = new("pt-BR");

    private static readonly IBrush CorHover = new SolidColorBrush(Color.Parse("#EFF6FF"));
    private static readonly IBrush CorSelecionada = new SolidColorBrush(Color.Parse("#DBEAFE"));
    private static readonly IBrush CorBarraAtiva = new SolidColorBrush(Color.Parse("#3B82F6"));
    private static readonly IBrush CorNormal = Brushes.Transparent;
    private static readonly IBrush CorZebra = new SolidColorBrush(Color.Parse("#FAFBFC"));
    private static readonly IBrush CorBordaSuave = new SolidColorBrush(Color.Parse("#E2E8F0"));
    private static readonly IBrush CorBordaHeader = new SolidColorBrush(Color.Parse("#CBD5E1"));
    private static readonly IBrush CorFundoHeader = new SolidColorBrush(Color.Parse("#F8FAFC"));
    private static readonly IBrush CorTextoHeader = new SolidColorBrush(Color.Parse("#475569"));
    private static readonly IBrush CorTexto = new SolidColorBrush(Color.Parse("#1E293B"));
    // Tom da coluna APLICAÇÃO: slate-700 (#334155). Quase tão legível quanto a descrição (#1E293B),
    // mantendo só uma leve hierarquia — o operador realmente lê esse texto. Era #64748B (lavado).
    private static readonly IBrush CorTextoSuave = new SolidColorBrush(Color.Parse("#334155"));

    private CatalogoViewModel? _vm;

    /// <summary>
    /// Quando true, a tela funciona como SELETOR de peça (usada na pré-venda via F2): cada linha
    /// fica clicável e dispara <see cref="PecaSelecionada"/>. Quando false, é só consulta (uso normal).
    /// </summary>
    public bool ModoSeletor { get; set; }

    /// <summary>Disparado ao escolher uma peça no modo seletor (duplo-clique ou Enter).</summary>
    public event System.Action<CatalogoItemDto>? PecaSelecionada;

    // Estado da navegação por teclado no modo seletor: os Borders de cada linha (para pintar a
    // cor de fundo), as barras laterais de destaque, e o índice atualmente destacado (-1 = nenhum).
    private readonly System.Collections.Generic.List<Border> _linhas = new();
    private readonly System.Collections.Generic.List<Border> _barras = new();
    private int _indiceSelecionado = -1;

    public CatalogoView()
    {
        InitializeComponent();
        DataContextChanged += AoTrocarDataContext;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoTrocarDataContext(object? sender, System.EventArgs e)
    {
        if (DataContext is not CatalogoViewModel vm)
            return;

        _vm = vm;
        vm.Resultados.CollectionChanged += AoMudarColecao;

        if (vm.InicializarCommand.CanExecute(null))
            vm.InicializarCommand.Execute(null);
    }

    private void AoMudarColecao(object? sender, NotifyCollectionChangedEventArgs e) => RegerarTabela();

    // ===== Navegação por teclado (modo seletor — usada pela CatalogoSeletorWindow) =====

    /// <summary>Coloca o foco no campo de busca de peça (chamado ao abrir o seletor).</summary>
    public void FocarBusca() => this.FindControl<TextBox>("CampoPeca")?.Focus();

    /// <summary>Move o destaque uma linha para baixo (seta ↓). Entra na lista a partir da busca.</summary>
    public void NavegarBaixo()
    {
        if (_linhas.Count == 0) return;
        DestacarLinha(System.Math.Min(_indiceSelecionado + 1, _linhas.Count - 1));
    }

    /// <summary>Move o destaque uma linha para cima (seta ↑).</summary>
    public void NavegarCima()
    {
        if (_linhas.Count == 0) return;
        DestacarLinha(System.Math.Max(_indiceSelecionado - 1, 0));
    }

    /// <summary>Confirma a linha destacada (Enter). Retorna true se selecionou algo.</summary>
    public bool SelecionarAtual()
    {
        if (_indiceSelecionado < 0 || _indiceSelecionado >= _linhas.Count) return false;
        if (_linhas[_indiceSelecionado].Tag is CatalogoItemDto peca)
        {
            PecaSelecionada?.Invoke(peca);
            return true;
        }
        return false;
    }

    // Teclado no modo consulta (toolbar): ↓/↑ movem a marca. Sem Enter (consulta não tem ação).
    private void AoTeclarNaListaConsulta(object? sender, KeyEventArgs e)
    {
        if (_linhas.Count == 0) return;
        switch (e.Key)
        {
            case Key.Down:
                DestacarLinha(System.Math.Min(_indiceSelecionado + 1, _linhas.Count - 1));
                e.Handled = true;
                break;
            case Key.Up:
                DestacarLinha(System.Math.Max(_indiceSelecionado - 1, 0));
                e.Handled = true;
                break;
        }
    }

    // Pinta a linha destacada (azul) + barra lateral azul-primário; devolve as demais à zebra.
    private void DestacarLinha(int indice)
    {
        if (indice < 0 || indice >= _linhas.Count) return;

        for (var i = 0; i < _linhas.Count; i++)
        {
            _linhas[i].Background = i == indice ? CorSelecionada : (i % 2 == 1 ? CorZebra : CorNormal);
            _barras[i].IsVisible = i == indice; // régua azul só na linha ativa
        }

        _indiceSelecionado = indice;
        _linhas[indice].BringIntoView();
    }

    private void RegerarTabela()
    {
        var container = this.FindControl<Panel>("TabelaContainer");
        if (container is null || _vm is null)
            return;

        for (var i = container.Children.Count - 1; i >= 0; i--)
            if (container.Children[i] is DockPanel)
                container.Children.RemoveAt(i);

        // A tabela foi recriada (nova busca): zera o rastreio das linhas e a seleção do teclado.
        _linhas.Clear();
        _barras.Clear();
        _indiceSelecionado = -1;

        var header = CriarHeader();
        Control corpo = _vm.Resultados.Count > 0 ? CriarCorpo() : CriarMensagemVazia();

        var dock = new DockPanel();
        DockPanel.SetDock(header, Dock.Top);
        dock.Children.Add(header);
        dock.Children.Add(corpo);
        container.Children.Insert(0, dock);

        // No modo seletor, abre com a primeira linha já selecionada (fluxo de teclado imediato).
        if (ModoSeletor && _linhas.Count > 0)
            DestacarLinha(0);
    }

    private Grid CriarHeader()
    {
        var header = new Grid
        {
            ColumnDefinitions = ColumnDefinitions.Parse(ColDefs),
            Height = 28,
            Background = CorFundoHeader,
        };

        var baseLine = new Border { Background = CorBordaHeader, Height = 1, VerticalAlignment = VerticalAlignment.Bottom };
        Grid.SetColumnSpan(baseLine, Headers.Length);
        header.Children.Add(baseLine);

        for (var col = 0; col < Headers.Length; col++)
        {
            var txt = new TextBlock
            {
                Text = Headers[col],
                FontSize = 11,
                FontWeight = FontWeight.SemiBold,
                Foreground = CorTextoHeader,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 8, 0),
            };
            Grid.SetColumn(txt, col);
            header.Children.Add(txt);

            if (col < Headers.Length - 1)
            {
                var div = new Border { Width = 1, Background = CorBordaSuave, HorizontalAlignment = HorizontalAlignment.Right };
                Grid.SetColumn(div, col);
                header.Children.Add(div);
            }
        }

        return header;
    }

    private ScrollViewer CriarCorpo()
    {
        var grid = new Grid { ColumnDefinitions = ColumnDefinitions.Parse(ColDefs) };
        // No modo consulta (toolbar), o próprio grid captura o teclado (↓/↑ navegam a marca).
        // No modo seletor, quem cuida do teclado é a CatalogoSeletorWindow (evita handler duplicado).
        if (!ModoSeletor)
        {
            grid.Focusable = true;
            grid.KeyDown += AoTeclarNaListaConsulta;
        }
        for (var i = 0; i < _vm!.Resultados.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition(28, GridUnitType.Pixel));

        for (var i = 0; i < _vm.Resultados.Count; i++)
        {
            var item = _vm.Resultados[i];
            var corLinha = i % 2 == 1 ? CorZebra : CorNormal;

            var fundo = new Border
            {
                Background = corLinha,
                BorderBrush = CorBordaSuave,
                BorderThickness = new Thickness(0, 0, 0, 1),
            };
            Grid.SetRow(fundo, i);
            Grid.SetColumnSpan(fundo, Headers.Length);

            var indiceLinha = i;

            // Hover = destaque PASSAGEIRO, separado da seleção. Só pinta/despinta a linha que NÃO é
            // a selecionada (a marca forte vence o hover). A linha selecionada ignora o hover.
            fundo.PointerEntered += (_, _) => { if (indiceLinha != _indiceSelecionado) fundo.Background = CorHover; };
            fundo.PointerExited += (_, _) => { if (indiceLinha != _indiceSelecionado) fundo.Background = corLinha; };

            // Seleção visual vale nos DOIS modos (consulta pela toolbar e seletor pela pré-venda):
            // clique marca a linha (+ régua lateral), igual às listas de cadastro.
            fundo.Cursor = new Cursor(StandardCursorType.Hand);
            fundo.Tag = item;
            _linhas.Add(fundo);
            fundo.Tapped += (_, _) => { grid.Focus(); DestacarLinha(indiceLinha); }; // 1 clique = marca + foca (habilita setas na consulta)

            // Barra lateral azul-primário (3px) — "régua" da linha marcada.
            var barra = new Border
            {
                Width = 3,
                Background = CorBarraAtiva,
                HorizontalAlignment = HorizontalAlignment.Left,
                IsVisible = false,
            };
            Grid.SetRow(barra, i);
            Grid.SetColumnSpan(barra, Headers.Length);
            _barras.Add(barra);
            grid.Children.Add(barra);

            // Exclusivo do seletor (pré-venda): duplo-clique ADICIONA a peça.
            if (ModoSeletor)
            {
                fundo.DoubleTapped += (_, _) =>
                {
                    if (fundo.Tag is CatalogoItemDto escolhida)
                        PecaSelecionada?.Invoke(escolhida);
                };
            }

            grid.Children.Add(fundo);

            Celula(grid, i, 0, item.CodProduto.ToString(), mono: true);
            Celula(grid, i, 1, item.Descricao);
            Celula(grid, i, 2, string.IsNullOrEmpty(item.Aplicacoes) ? string.Empty : item.Aplicacoes, suave: true);
            Celula(grid, i, 3, Converters.PosicaoPecaConverter.Rotular(item.Posicao));
            Celula(grid, i, 4, item.CodFabricante ?? string.Empty, mono: true);
            Celula(grid, i, 5, item.Unidade.ToString());
            Celula(grid, i, 6, item.VlrVenda.ToString("N2", PtBr), mono: true, alinharDireita: true);
        }

        return new ScrollViewer
        {
            Content = grid,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
    }

    private static Border CriarMensagemVazia() => new()
    {
        Padding = new Thickness(0, 40, 0, 0),
        Child = new TextBlock
        {
            Text = "Nenhuma peça encontrada. Ajuste os filtros de veículo ou o termo da peça.",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#94A3B8")),
            HorizontalAlignment = HorizontalAlignment.Center,
        },
    };

    private static void Celula(Grid grid, int row, int col, string texto,
        bool mono = false, bool alinharDireita = false, bool suave = false)
    {
        var txt = new TextBlock
        {
            Text = texto,
            FontSize = 12,
            FontFamily = mono ? new FontFamily("Consolas") : new FontFamily("Segoe UI"),
            Foreground = suave ? CorTextoSuave : CorTexto,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = alinharDireita ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin = new Thickness(8, 0, 8, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        Grid.SetRow(txt, row);
        Grid.SetColumn(txt, col);
        grid.Children.Add(txt);
    }
}
