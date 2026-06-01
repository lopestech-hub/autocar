using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AutoCar.Application.Modules.Estoque.DTOs;
using AutoCar.Desktop.ViewModels.Inventory;

namespace AutoCar.Desktop.Views.Inventory;

/// <summary>
/// Listagem de saldos de estoque. Tabela renderizada como Grid único via code-behind
/// (padrão do projeto — nunca DataGrid). Duplo-clique numa linha abre a janela de movimentação.
/// </summary>
public partial class EstoqueView : UserControl
{
    // CÓDIGO · PRODUTO · UN · SALDO · DISPONÍVEL
    private const string ColDefs = "90,*,70,110,110";
    private static readonly string[] Headers = { "CÓDIGO", "PRODUTO", "UN", "SALDO", "DISPONÍVEL" };

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
    // Saldo zerado em cinza (sem estoque): chama menos atenção que um saldo real.
    private static readonly IBrush CorSaldoZero = new SolidColorBrush(Color.Parse("#94A3B8"));

    // Estado da seleção visual (clique marca; ↓/↑ navegam; Enter/duplo-clique movimenta).
    private readonly System.Collections.Generic.List<Border> _linhas = new();
    private readonly System.Collections.Generic.List<Border> _barras = new();
    private int _indiceSelecionado = -1;

    private EstoqueViewModel? _vm;

    public EstoqueView()
    {
        InitializeComponent();
        DataContextChanged += AoTrocarDataContext;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoTrocarDataContext(object? sender, System.EventArgs e)
    {
        if (DataContext is not EstoqueViewModel vm)
            return;

        _vm = vm;
        vm.Saldos.CollectionChanged += AoMudarColecao;
        vm.AbrirMovimentacaoSolicitado += AbrirJanelaMovimentacao;

        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
    }

    /// <summary>
    /// Abre a movimentação de estoque numa janela separada (não-modal): o shell principal continua
    /// acessível. Ao concluir o movimento (Concluido), recarrega a listagem para refletir o saldo novo.
    /// </summary>
    private void AbrirJanelaMovimentacao(MovimentoEstoqueFormViewModel form)
    {
        form.Concluido += () => _vm?.RecarregarAsync();

        var janela = new MovimentoEstoqueWindow(form);
        var dono = TopLevel.GetTopLevel(this) as Window;
        if (dono is not null)
            janela.Show(dono);
        else
            janela.Show();
    }

    private void AoMudarColecao(object? sender, NotifyCollectionChangedEventArgs e) => RegerarTabela();

    private void RegerarTabela()
    {
        var container = this.FindControl<Panel>("TabelaContainer");
        if (container is null || _vm is null)
            return;

        for (var i = container.Children.Count - 1; i >= 0; i--)
            if (container.Children[i] is DockPanel)
                container.Children.RemoveAt(i);

        _linhas.Clear();
        _barras.Clear();
        _indiceSelecionado = -1;

        var header = CriarHeader();
        Control corpo = _vm.Saldos.Count > 0 ? CriarCorpo() : CriarMensagemVazia();

        var dock = new DockPanel();
        DockPanel.SetDock(header, Dock.Top);
        dock.Children.Add(header);
        dock.Children.Add(corpo);
        container.Children.Insert(0, dock);
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
                HorizontalAlignment = col >= 3 ? HorizontalAlignment.Right : HorizontalAlignment.Left,
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
        var grid = new Grid { ColumnDefinitions = ColumnDefinitions.Parse(ColDefs), Focusable = true };
        grid.KeyDown += AoTeclarNaLista;
        for (var i = 0; i < _vm!.Saldos.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition(28, GridUnitType.Pixel));

        for (var i = 0; i < _vm.Saldos.Count; i++)
        {
            var saldo = _vm.Saldos[i];
            var corLinha = i % 2 == 1 ? CorZebra : CorNormal;
            var indiceLinha = i;

            var fundo = new Border
            {
                Background = corLinha,
                BorderBrush = CorBordaSuave,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Cursor = new Cursor(StandardCursorType.Hand),
                Tag = saldo,
            };
            Grid.SetRow(fundo, i);
            Grid.SetColumnSpan(fundo, Headers.Length);

            fundo.PointerEntered += (_, _) => { if (indiceLinha != _indiceSelecionado) fundo.Background = CorHover; };
            fundo.PointerExited += (_, _) => { if (indiceLinha != _indiceSelecionado) fundo.Background = corLinha; };
            fundo.Tapped += (_, _) => { grid.Focus(); DestacarLinha(indiceLinha); };   // 1 clique = marca + foca
            fundo.DoubleTapped += (_, _) =>
            {
                if (fundo.Tag is SaldoEstoqueListaDto dto && _vm.MovimentarCommand.CanExecute(dto))
                    _vm.MovimentarCommand.Execute(dto);
            };
            grid.Children.Add(fundo);
            _linhas.Add(fundo);

            var barra = new Border { Width = 3, Background = CorBarraAtiva, HorizontalAlignment = HorizontalAlignment.Left, IsVisible = false };
            Grid.SetRow(barra, i);
            Grid.SetColumnSpan(barra, Headers.Length);
            _barras.Add(barra);
            grid.Children.Add(barra);

            Celula(grid, i, 0, saldo.CodProduto.ToString(), mono: true);
            Celula(grid, i, 1, saldo.Descricao);
            Celula(grid, i, 2, saldo.Unidade.ToString());
            CelulaSaldo(grid, i, 3, saldo.QtdSaldo);
            CelulaSaldo(grid, i, 4, saldo.QtdDisponivel);
        }

        return new ScrollViewer
        {
            Content = grid,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
    }

    // Marca a linha (fundo azul + barra lateral); devolve as demais à zebra.
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

    // Teclado na lista: ↓/↑ movem a marca; Enter movimenta a linha marcada.
    private void AoTeclarNaLista(object? sender, KeyEventArgs e)
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
            case Key.Enter:
                if (_indiceSelecionado >= 0 && _linhas[_indiceSelecionado].Tag is SaldoEstoqueListaDto dto
                    && _vm?.MovimentarCommand.CanExecute(dto) == true)
                    _vm.MovimentarCommand.Execute(dto);
                e.Handled = true;
                break;
        }
    }

    private static Border CriarMensagemVazia() => new()
    {
        Padding = new Thickness(0, 40, 0, 0),
        Child = new TextBlock
        {
            Text = "Nenhum produto encontrado.",
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
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(8, 0, 8, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        Grid.SetRow(txt, row);
        Grid.SetColumn(txt, col);
        grid.Children.Add(txt);
    }

    // Célula de saldo: número alinhado à direita, mono; cinza quando zero (sem estoque).
    private static void CelulaSaldo(Grid grid, int row, int col, int valor)
    {
        var txt = new TextBlock
        {
            Text = valor.ToString("N0", PtBr),
            FontSize = 12,
            FontFamily = new FontFamily("Consolas"),
            FontWeight = valor > 0 ? FontWeight.SemiBold : FontWeight.Normal,
            Foreground = valor > 0 ? CorTexto : CorSaldoZero,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(8, 0, 8, 0),
        };
        Grid.SetRow(txt, row);
        Grid.SetColumn(txt, col);
        grid.Children.Add(txt);
    }
}
