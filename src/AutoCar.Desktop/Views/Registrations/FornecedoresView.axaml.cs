using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;
using AutoCar.Domain.Enums;
using AutoCar.Desktop.ViewModels.Registrations;

namespace AutoCar.Desktop.Views.Registrations;

/// <summary>
/// Listagem de fornecedores. Tabela renderizada como Grid único via code-behind
/// (padrão Luna — nunca DataGrid). Duplo-clique numa linha abre o formulário.
/// </summary>
public partial class FornecedoresView : UserControl
{
    // CÓDIGO · TIPO · DOCUMENTO · NOME/RAZÃO SOCIAL · TELEFONE · STATUS
    private const string ColDefs = "70,60,150,*,130,90";
    private static readonly string[] Headers =
        { "CÓDIGO", "TIPO", "DOCUMENTO", "NOME / RAZÃO SOCIAL", "TELEFONE", "STATUS" };

    private static readonly IBrush CorHover = new SolidColorBrush(Color.Parse("#EFF6FF"));
    private static readonly IBrush CorNormal = Brushes.Transparent;
    private static readonly IBrush CorZebra = new SolidColorBrush(Color.Parse("#FAFBFC"));
    private static readonly IBrush CorBordaSuave = new SolidColorBrush(Color.Parse("#E2E8F0"));
    private static readonly IBrush CorBordaHeader = new SolidColorBrush(Color.Parse("#CBD5E1"));
    private static readonly IBrush CorFundoHeader = new SolidColorBrush(Color.Parse("#F8FAFC"));
    private static readonly IBrush CorTextoHeader = new SolidColorBrush(Color.Parse("#64748B"));
    private static readonly IBrush CorTexto = new SolidColorBrush(Color.Parse("#1E293B"));

    private FornecedoresViewModel? _vm;

    public FornecedoresView()
    {
        InitializeComponent();
        DataContextChanged += AoTrocarDataContext;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoTrocarDataContext(object? sender, System.EventArgs e)
    {
        if (DataContext is not FornecedoresViewModel vm)
            return;

        _vm = vm;
        vm.Fornecedores.CollectionChanged += AoMudarColecao;

        // Carrega ao abrir a tela.
        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
    }

    private void AoMudarColecao(object? sender, NotifyCollectionChangedEventArgs e) => RegerarTabela();

    private void RegerarTabela()
    {
        var container = this.FindControl<Panel>("TabelaContainer");
        if (container is null || _vm is null)
            return;

        // Remove a tabela antiga (preserva a mensagem de erro, que é filha do XAML).
        for (var i = container.Children.Count - 1; i >= 0; i--)
            if (container.Children[i] is DockPanel)
                container.Children.RemoveAt(i);

        // Header sempre visível (dá estrutura à tabela mesmo vazia).
        var header = CriarHeader();

        // Corpo: linhas (com scroll) ou faixa de "nenhum registro".
        Control corpo = _vm.Fornecedores.Count > 0 ? CriarCorpo() : CriarMensagemVazia();

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
        for (var i = 0; i < _vm!.Fornecedores.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition(28, GridUnitType.Pixel));

        for (var i = 0; i < _vm.Fornecedores.Count; i++)
        {
            var fornecedor = _vm.Fornecedores[i];

            // Zebra: linhas ímpares recebem um fundo sutil para facilitar a leitura.
            var corLinha = i % 2 == 1 ? CorZebra : CorNormal;

            var fundo = new Border
            {
                Background = corLinha,
                BorderBrush = CorBordaSuave,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Cursor = new Cursor(StandardCursorType.Hand),
                Tag = fornecedor,
            };
            Grid.SetRow(fundo, i);
            Grid.SetColumnSpan(fundo, Headers.Length);

            fundo.PointerEntered += (_, _) => fundo.Background = CorHover;
            fundo.PointerExited += (_, _) => fundo.Background = corLinha;
            fundo.DoubleTapped += (_, _) =>
            {
                if (fundo.Tag is FornecedorListaDto dto && _vm.AbrirCommand.CanExecute(dto))
                    _vm.AbrirCommand.Execute(dto);
            };
            grid.Children.Add(fundo);

            Celula(grid, i, 0, fornecedor.CodFornecedor.ToString(), mono: true);
            BadgeTipo(grid, i, 1, fornecedor.TipoPessoa);
            Celula(grid, i, 2, FormatarDocumento(fornecedor.Documento, fornecedor.TipoPessoa), mono: true);
            Celula(grid, i, 3, fornecedor.RazaoSocial);
            Celula(grid, i, 4, fornecedor.Telefone ?? "", mono: true);
            BadgeStatus(grid, i, 5, fornecedor.FlgAtivo);
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
            Text = "Nenhum fornecedor cadastrado. Clique em \"+ Novo fornecedor\" para começar.",
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

    // Badge PF (azul) / PJ (roxo) — leitura rápida do tipo.
    private static void BadgeTipo(Grid grid, int row, int col, TipoPessoa tipo)
    {
        var pf = tipo == TipoPessoa.Fisica;
        var badge = new Border
        {
            Background = new SolidColorBrush(Color.Parse(pf ? "#E0F2FE" : "#EDE9FE")),
            CornerRadius = new CornerRadius(2),
            Padding = new Thickness(6, 2, 6, 2),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0),
            Child = new TextBlock
            {
                Text = pf ? "PF" : "PJ",
                FontSize = 10,
                FontWeight = FontWeight.Medium,
                Foreground = new SolidColorBrush(Color.Parse(pf ? "#075985" : "#5B21B6")),
            },
        };
        Grid.SetRow(badge, row);
        Grid.SetColumn(badge, col);
        grid.Children.Add(badge);
    }

    // Máscara só na exibição (o banco guarda só dígitos).
    private static string FormatarDocumento(string doc, TipoPessoa tipo)
    {
        if (tipo == TipoPessoa.Fisica && doc.Length == 11)
            return $"{doc[..3]}.{doc.Substring(3, 3)}.{doc.Substring(6, 3)}-{doc.Substring(9, 2)}";
        if (tipo == TipoPessoa.Juridica && doc.Length == 14)
            return $"{doc[..2]}.{doc.Substring(2, 3)}.{doc.Substring(5, 3)}/{doc.Substring(8, 4)}-{doc.Substring(12, 2)}";
        return doc;
    }
}
