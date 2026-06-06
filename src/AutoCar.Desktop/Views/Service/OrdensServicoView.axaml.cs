using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AutoCar.Application.Modules.Service.OrdensServico.DTOs;
using AutoCar.Desktop.ViewModels.Service;
using AutoCar.Domain.Enums;

namespace AutoCar.Desktop.Views.Service;

/// <summary>
/// Listagem de Ordens de Serviço. Tabela renderizada como Grid único via code-behind (padrão do
/// projeto — nunca DataGrid). Duplo-clique numa linha abre o formulário numa janela separada.
/// </summary>
public partial class OrdensServicoView : UserControl
{
    // Nº · DATA · CLIENTE · VEÍCULO · ITENS · TOTAL · SITUAÇÃO
    private const string ColDefs = "80,110,*,140,70,110,130";
    private static readonly string[] Headers = { "Nº", "DATA", "CLIENTE", "VEÍCULO", "ITENS", "TOTAL", "SITUAÇÃO" };

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

    private readonly System.Collections.Generic.List<Border> _linhas = new();
    private readonly System.Collections.Generic.List<Border> _barras = new();
    private int _indiceSelecionado = -1;

    private OrdensServicoViewModel? _vm;

    public OrdensServicoView()
    {
        InitializeComponent();
        DataContextChanged += AoTrocarDataContext;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoTrocarDataContext(object? sender, System.EventArgs e)
    {
        if (DataContext is not OrdensServicoViewModel vm)
            return;

        _vm = vm;
        vm.Ordens.CollectionChanged += AoMudarColecao;
        vm.AbrirJanelaSolicitado += AbrirJanelaOrdemServico;

        if (vm.CarregarCommand.CanExecute(null))
            vm.CarregarCommand.Execute(null);
    }

    /// <summary>
    /// Abre o formulário da OS numa janela separada maximizada (não-modal): o shell principal
    /// continua acessível. Ao salvar (Salvo), recarrega a listagem.
    /// </summary>
    private void AbrirJanelaOrdemServico(OrdemServicoFormViewModel form)
    {
        form.Salvo += () => _vm?.RecarregarAsync();

        var janela = new OrdemServicoWindow(form);
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
        Control corpo = _vm.Ordens.Count > 0 ? CriarCorpo() : CriarMensagemVazia();

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
                // ITENS (4) e TOTAL (5) alinham à direita, como os dados numéricos.
                HorizontalAlignment = col is 4 or 5 ? HorizontalAlignment.Right : HorizontalAlignment.Left,
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
        for (var i = 0; i < _vm!.Ordens.Count; i++)
            grid.RowDefinitions.Add(new RowDefinition(28, GridUnitType.Pixel));

        for (var i = 0; i < _vm.Ordens.Count; i++)
        {
            var os = _vm.Ordens[i];
            var corLinha = i % 2 == 1 ? CorZebra : CorNormal;
            var indiceLinha = i;

            var fundo = new Border
            {
                Background = corLinha,
                BorderBrush = CorBordaSuave,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Cursor = new Cursor(StandardCursorType.Hand),
                Tag = os,
            };
            Grid.SetRow(fundo, i);
            Grid.SetColumnSpan(fundo, Headers.Length);

            fundo.PointerEntered += (_, _) => { if (indiceLinha != _indiceSelecionado) fundo.Background = CorHover; };
            fundo.PointerExited += (_, _) => { if (indiceLinha != _indiceSelecionado) fundo.Background = corLinha; };
            fundo.Tapped += (_, _) => { grid.Focus(); DestacarLinha(indiceLinha); };
            fundo.DoubleTapped += (_, _) =>
            {
                if (fundo.Tag is OrdemServicoListaDto dto && _vm.AbrirCommand.CanExecute(dto))
                    _vm.AbrirCommand.Execute(dto);
            };
            grid.Children.Add(fundo);
            _linhas.Add(fundo);

            var barra = new Border { Width = 3, Background = CorBarraAtiva, HorizontalAlignment = HorizontalAlignment.Left, IsVisible = false };
            Grid.SetRow(barra, i);
            Grid.SetColumnSpan(barra, Headers.Length);
            _barras.Add(barra);
            grid.Children.Add(barra);

            // A data do banco vem em UTC; converter para Brasília (UTC-3) na exibição.
            var dataLocal = os.DataCriacao.ToLocalTime();

            Celula(grid, i, 0, os.CodOrdemServico.ToString(), mono: true);
            Celula(grid, i, 1, dataLocal.ToString("dd/MM/yyyy", PtBr), mono: true);
            Celula(grid, i, 2, os.Cliente);
            // Veículo: modelo + placa (quando houver). Sem placa, só o modelo. Sem nada, vazio (sem traço).
            Celula(grid, i, 3, TextoVeiculo(os.VeiculoModelo, os.VeiculoPlaca));
            Celula(grid, i, 4, os.QtdItens.ToString(), mono: true, alinharDireita: true);
            Celula(grid, i, 5, os.VlrTotal.ToString("N2", PtBr), mono: true, alinharDireita: true);
            BadgeSituacao(grid, i, 6, os.Situacao);
        }

        return new ScrollViewer
        {
            Content = grid,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
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
                if (_indiceSelecionado >= 0 && _linhas[_indiceSelecionado].Tag is OrdemServicoListaDto dto
                    && _vm?.AbrirCommand.CanExecute(dto) == true)
                    _vm.AbrirCommand.Execute(dto);
                e.Handled = true;
                break;
        }
    }

    private static Border CriarMensagemVazia() => new()
    {
        Padding = new Thickness(0, 40, 0, 0),
        Child = new TextBlock
        {
            Text = "Nenhuma ordem de serviço. Clique em \"+ Nova OS\" para começar.",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#94A3B8")),
            HorizontalAlignment = HorizontalAlignment.Center,
        },
    };

    // Texto do veículo na listagem: "MODELO · PLACA" quando há placa; só o modelo quando não há;
    // vazio quando nenhum dos dois (sem traço longo, regra do projeto).
    private static string TextoVeiculo(string? modelo, string? placa)
    {
        var temModelo = !string.IsNullOrWhiteSpace(modelo);
        var temPlaca = !string.IsNullOrWhiteSpace(placa);
        if (temModelo && temPlaca) return $"{modelo} · {placa}";
        if (temModelo) return modelo!;
        if (temPlaca) return placa!;
        return string.Empty;
    }

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

    // Badge colorido por situação (5 estados do ciclo da OS). Fundo claro + borda 1px da cor forte
    // da família = etiqueta sólida que salta da linha.
    private static void BadgeSituacao(Grid grid, int row, int col, SituacaoOrdemServico situacao)
    {
        var (fundo, borda, texto, rotulo) = situacao switch
        {
            SituacaoOrdemServico.EmAndamento => ("#FEF3C7", "#F59E0B", "#92400E", "Em andamento"),
            SituacaoOrdemServico.Concluida => ("#E0E7FF", "#6366F1", "#3730A3", "Concluída"),
            SituacaoOrdemServico.Faturada => ("#DCFCE7", "#22C55E", "#166534", "Faturada"),
            SituacaoOrdemServico.Cancelada => ("#FEE2E2", "#EF4444", "#991B1B", "Cancelada"),
            _ => ("#DBEAFE", "#3B82F6", "#1E40AF", "Aberta"),
        };

        var badge = new Border
        {
            Background = new SolidColorBrush(Color.Parse(fundo)),
            BorderBrush = new SolidColorBrush(Color.Parse(borda)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Padding = new Thickness(6, 1, 6, 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 8, 0),
            Child = new TextBlock
            {
                Text = rotulo,
                FontSize = 10,
                FontWeight = FontWeight.SemiBold,
                Foreground = new SolidColorBrush(Color.Parse(texto)),
            },
        };
        Grid.SetRow(badge, row);
        Grid.SetColumn(badge, col);
        grid.Children.Add(badge);
    }
}
