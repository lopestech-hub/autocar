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
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Listagem de produtos. Tabela renderizada como Grid único via code-behind
/// (padrão do projeto — nunca DataGrid). Duplo-clique numa linha abre o formulário.
/// </summary>
public partial class ProdutosView : UserControl
{
    // CÓDIGO · DESCRIÇÃO · CATEGORIA · MARCA · UN · VENDA · STATUS
    private const string ColDefs = "80,*,140,130,50,90,90";
    private static readonly string[] Headers = { "CÓDIGO", "DESCRIÇÃO", "CATEGORIA", "MARCA", "UN", "VENDA", "STATUS" };

    private static readonly CultureInfo PtBr = new("pt-BR");

    private static readonly IBrush CorHover = new SolidColorBrush(Color.Parse("#EFF6FF"));
    private static readonly IBrush CorNormal = Brushes.Transparent;
    private static readonly IBrush CorZebra = new SolidColorBrush(Color.Parse("#FAFBFC"));
    private static readonly IBrush CorBordaSuave = new SolidColorBrush(Color.Parse("#E2E8F0"));
    private static readonly IBrush CorBordaHeader = new SolidColorBrush(Color.Parse("#CBD5E1"));
    private static readonly IBrush CorFundoHeader = new SolidColorBrush(Color.Parse("#F8FAFC"));
    private static readonly IBrush CorTextoHeader = new SolidColorBrush(Color.Parse("#64748B"));
    private static readonly IBrush CorTexto = new SolidColorBrush(Color.Parse("#1E293B"));

    private ProdutosViewModel? _vm;

    public ProdutosView()
    {
        InitializeComponent();
        DataContextChanged += AoTrocarDataContext;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoTrocarDataContext(object? sender, System.EventArgs e)
    {
        if (DataContext is not ProdutosViewModel vm)
            return;

        _vm = vm;
        vm.Produtos.CollectionChanged += AoMudarColecao;

        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
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

        var header = CriarHeader();
        Control corpo = _vm.Produtos.Count > 0 ? CriarCorpo() : CriarMensagemVazia();

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
                FontWeight = FontWeight.Medium,
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
        for (var i = 0; i < _vm!.Produtos.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition(28, GridUnitType.Pixel));

        for (var i = 0; i < _vm.Produtos.Count; i++)
        {
            var produto = _vm.Produtos[i];
            var corLinha = i % 2 == 1 ? CorZebra : CorNormal;

            var fundo = new Border
            {
                Background = corLinha,
                BorderBrush = CorBordaSuave,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Cursor = new Cursor(StandardCursorType.Hand),
                Tag = produto,
            };
            Grid.SetRow(fundo, i);
            Grid.SetColumnSpan(fundo, Headers.Length);

            fundo.PointerEntered += (_, _) => fundo.Background = CorHover;
            fundo.PointerExited += (_, _) => fundo.Background = corLinha;
            fundo.DoubleTapped += (_, _) =>
            {
                if (fundo.Tag is ProdutoListaDto dto && _vm.AbrirCommand.CanExecute(dto))
                    _vm.AbrirCommand.Execute(dto);
            };
            grid.Children.Add(fundo);

            Celula(grid, i, 0, produto.CodProduto.ToString(), mono: true);
            Celula(grid, i, 1, produto.Descricao);
            Celula(grid, i, 2, produto.Categoria ?? "—");
            Celula(grid, i, 3, produto.Marca ?? "—");
            Celula(grid, i, 4, produto.Unidade.ToString());
            Celula(grid, i, 5, produto.VlrVenda.ToString("N2", PtBr), mono: true, alinharDireita: true);
            BadgeStatus(grid, i, 6, produto.FlgAtivo);
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
            Text = "Nenhum produto cadastrado. Clique em \"+ Novo produto\" para começar.",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#94A3B8")),
            HorizontalAlignment = HorizontalAlignment.Center,
        },
    };

    private static void Celula(Grid grid, int row, int col, string texto, bool mono = false, bool alinharDireita = false)
    {
        var txt = new TextBlock
        {
            Text = texto,
            FontSize = 12,
            FontFamily = mono ? new FontFamily("Consolas") : new FontFamily("Segoe UI"),
            Foreground = CorTexto,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = alinharDireita ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin = new Thickness(8, 0, 8, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        Grid.SetRow(txt, row);
        Grid.SetColumn(txt, col);
        grid.Children.Add(txt);
    }

    private static void BadgeStatus(Grid grid, int row, int col, bool ativo)
    {
        var badge = new Border
        {
            Background = new SolidColorBrush(Color.Parse(ativo ? "#DCFCE7" : "#FEE2E2")),
            CornerRadius = new CornerRadius(2),
            Padding = new Thickness(6, 2, 6, 2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0),
            Child = new TextBlock
            {
                Text = ativo ? "Ativo" : "Inativo",
                FontSize = 10,
                FontWeight = FontWeight.Medium,
                Foreground = new SolidColorBrush(Color.Parse(ativo ? "#166534" : "#991B1B")),
            },
        };
        Grid.SetRow(badge, row);
        Grid.SetColumn(badge, col);
        grid.Children.Add(badge);
    }
}
