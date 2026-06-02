using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using AutoCar.Application.Modules.Registrations.Fornecedores;
using AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;

namespace AutoCar.Desktop.Views.Purchases;

/// <summary>
/// Janela seletora de fornecedor (aberta com F3 na compra). Busca por nome OU código (só dígitos
/// = código), navegação por setas, Enter/duplo-clique seleciona, Esc fecha. Tabela como Grid único
/// via code-behind (padrão do projeto). Reusa IFornecedorService — não duplica busca. Espelha o
/// ClienteSeletorWindow, mas sem opção "Consumidor" (o fornecedor é obrigatório na compra).
/// </summary>
public partial class FornecedorSeletorWindow : Window
{
    // CÓDIGO · RAZÃO SOCIAL · DOCUMENTO · TELEFONE
    private const string ColDefs = "70,*,150,130";
    private static readonly string[] Headers = { "CÓDIGO", "RAZÃO SOCIAL", "DOCUMENTO", "TELEFONE" };

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

    private readonly IFornecedorService? _fornecedores;
    private readonly Action<FornecedorListaDto>? _aoSelecionar;
    private readonly List<FornecedorListaDto> _resultado = new();
    private readonly List<Border> _linhas = new();
    private readonly List<Border> _barras = new();
    private int _indiceSelecionado = -1;
    private CancellationTokenSource? _debounce;

    public FornecedorSeletorWindow()
    {
        InitializeComponent();
    }

    public FornecedorSeletorWindow(IFornecedorService fornecedores, Action<FornecedorListaDto> aoSelecionar) : this()
    {
        _fornecedores = fornecedores;
        _aoSelecionar = aoSelecionar;

        var busca = this.FindControl<TextBox>("CampoBusca")!;
        busca.TextChanged += (_, _) => AgendarBusca(busca.Text);

        Opened += async (_, _) => { busca.Focus(); await BuscarAsync(null); };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AgendarBusca(string? termo)
    {
        _debounce?.Cancel();
        _debounce = new CancellationTokenSource();
        var token = _debounce.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token);
                if (!token.IsCancellationRequested)
                    await Dispatcher.UIThread.InvokeAsync(() => BuscarAsync(termo));
            }
            catch (TaskCanceledException) { }
        });
    }

    private async Task BuscarAsync(string? termo)
    {
        if (_fornecedores is null)
            return;
        // O ListarAsync do serviço já filtra por nome/razão social ou documento. Para código,
        // filtramos em memória (só dígitos → casa com cod_fornecedor).
        var lista = await _fornecedores.ListarAsync(termo);
        IEnumerable<FornecedorListaDto> filtrada = lista;

        var t = (termo ?? string.Empty).Trim();
        if (t.Length > 0 && t.All(char.IsDigit))
            filtrada = lista.Where(f => f.CodFornecedor.ToString().Contains(t));

        _resultado.Clear();
        _resultado.AddRange(filtrada);
        RegerarTabela();
    }

    private void RegerarTabela()
    {
        var container = this.FindControl<Panel>("TabelaContainer");
        if (container is null)
            return;

        container.Children.Clear();
        _linhas.Clear();
        _barras.Clear();
        _indiceSelecionado = -1;

        var header = CriarHeader();
        Control corpo = _resultado.Count > 0 ? CriarCorpo() : CriarMensagemVazia();

        var dock = new DockPanel();
        DockPanel.SetDock(header, Dock.Top);
        dock.Children.Add(header);
        dock.Children.Add(corpo);
        container.Children.Add(dock);

        if (_resultado.Count > 0)
            DestacarLinha(0);
    }

    private Grid CriarHeader()
    {
        var header = new Grid { ColumnDefinitions = ColumnDefinitions.Parse(ColDefs), Height = 28, Background = CorFundoHeader };
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
        for (var i = 0; i < _resultado.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition(28, GridUnitType.Pixel));

        for (var i = 0; i < _resultado.Count; i++)
        {
            var f = _resultado[i];
            var corLinha = i % 2 == 1 ? CorZebra : CorNormal;
            var indiceLinha = i;

            var fundo = new Border
            {
                Background = corLinha,
                BorderBrush = CorBordaSuave,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Cursor = new Cursor(StandardCursorType.Hand),
                Tag = f,
            };
            Grid.SetRow(fundo, i);
            Grid.SetColumnSpan(fundo, Headers.Length);
            fundo.PointerEntered += (_, _) => { if (indiceLinha != _indiceSelecionado) fundo.Background = CorHover; };
            fundo.PointerExited += (_, _) => { if (indiceLinha != _indiceSelecionado) fundo.Background = corLinha; };
            fundo.Tapped += (_, _) => DestacarLinha(indiceLinha);            // 1 clique = marca
            fundo.DoubleTapped += (_, _) => SelecionarLinha(indiceLinha);    // 2 cliques = escolhe
            grid.Children.Add(fundo);

            var barra = new Border { Width = 3, Background = CorBarraAtiva, HorizontalAlignment = HorizontalAlignment.Left, IsVisible = false };
            Grid.SetRow(barra, i);
            Grid.SetColumnSpan(barra, Headers.Length);
            _barras.Add(barra);
            grid.Children.Add(barra);
            _linhas.Add(fundo);

            Celula(grid, i, 0, f.CodFornecedor.ToString("0000"), mono: true);
            Celula(grid, i, 1, f.RazaoSocial);
            Celula(grid, i, 2, FormatarDocumento(f.Documento), mono: true);
            Celula(grid, i, 3, f.Telefone ?? "");
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
            Text = "Nenhum fornecedor encontrado. Ajuste a busca.",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#94A3B8")),
            HorizontalAlignment = HorizontalAlignment.Center,
        },
    };

    private static void Celula(Grid grid, int row, int col, string texto, bool mono = false)
    {
        var txt = new TextBlock
        {
            Text = texto,
            FontSize = 12,
            FontFamily = mono ? new FontFamily("Consolas") : new FontFamily("Segoe UI"),
            Foreground = CorTexto,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        Grid.SetRow(txt, row);
        Grid.SetColumn(txt, col);
        grid.Children.Add(txt);
    }

    // Máscara leve só para exibição (CPF 000.000.000-00 / CNPJ 00.000.000/0000-00).
    // TryParse tolerante: documento não-numérico (legado) é exibido cru, sem derrubar a tabela.
    private static string FormatarDocumento(string doc)
    {
        if (string.IsNullOrEmpty(doc)) return "";
        if (!ulong.TryParse(doc, out var n)) return doc;
        return doc.Length switch
        {
            11 => n.ToString(@"000\.000\.000\-00", PtBr),
            14 => n.ToString(@"00\.000\.000\/0000\-00", PtBr),
            _ => doc,
        };
    }

    private void DestacarLinha(int indice)
    {
        if (indice < 0 || indice >= _linhas.Count) return;
        for (var i = 0; i < _linhas.Count; i++)
        {
            _linhas[i].Background = i == indice ? CorSelecionada : (i % 2 == 1 ? CorZebra : CorNormal);
            _barras[i].IsVisible = i == indice;
        }
        _indiceSelecionado = indice;
        _linhas[indice].BringIntoView();
    }

    private void SelecionarLinha(int indice)
    {
        if (indice < 0 || indice >= _resultado.Count) return;
        _aoSelecionar?.Invoke(_resultado[indice]);
        Close();
    }

    private void AoTeclar(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
            case Key.Down:
                DestacarLinha(Math.Min(_indiceSelecionado + 1, _linhas.Count - 1));
                e.Handled = true;
                break;
            case Key.Up:
                DestacarLinha(Math.Max(_indiceSelecionado - 1, 0));
                e.Handled = true;
                break;
            case Key.Enter:
                SelecionarLinha(_indiceSelecionado);
                e.Handled = true;
                break;
        }
    }

    private void AoFechar(object? sender, RoutedEventArgs e) => Close();
}
