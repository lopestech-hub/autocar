using System;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoCar.Desktop.ViewModels.Purchases;

/// <summary>
/// Linha editável do grid de itens da compra. Descrição e produto são snapshot (read-only na linha;
/// vêm da escolha no Catálogo). Quantidade (inteira — autopeça não fraciona) e custo unitário são
/// editáveis como texto (parse tolerante BR) e recalculam o total da linha.
/// </summary>
public partial class CompraItemViewModel : ViewModelBase
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public CompraItemViewModel(Guid idProduto, string descricaoProduto, int qtd, decimal vlrCustoUnitario)
    {
        IdProduto = idProduto;
        DescricaoProduto = descricaoProduto;
        _qtd = qtd;
        _vlrCustoUnitario = vlrCustoUnitario;
    }

    /// <summary>FK do produto (snapshot — não muda na linha).</summary>
    public Guid IdProduto { get; }

    /// <summary>Descrição do produto no momento da inclusão (snapshot, read-only).</summary>
    public string DescricaoProduto { get; }

    [ObservableProperty] private int _qtd;
    [ObservableProperty] private decimal _vlrCustoUnitario;

    /// <summary>Quantidade como texto editável (inteira — autopeça não fraciona).</summary>
    public string QtdTexto
    {
        get => Qtd.ToString(PtBr);
        set { Qtd = ParseInteiro(value); OnPropertyChanged(); }
    }

    /// <summary>Custo unitário pago ao fornecedor, como texto editável.</summary>
    public string VlrCustoUnitarioTexto
    {
        get => VlrCustoUnitario.ToString("N2", PtBr);
        set { VlrCustoUnitario = ParseDecimal(value); OnPropertyChanged(); }
    }

    /// <summary>Total da linha: qtd × custo unitário. Read-only na tela.</summary>
    public decimal VlrTotalItem => Qtd * VlrCustoUnitario;

    public string VlrTotalItemTexto => VlrTotalItem.ToString("N2", PtBr);

    private static readonly IBrush FlashAmarelo = new SolidColorBrush(Color.Parse("#FEF9C3"));

    /// <summary>Cor de fundo da linha. Normalmente transparente; pisca amarelo ao ser adicionada.</summary>
    [ObservableProperty] private IBrush _brushFundoLinha = Brushes.Transparent;

    /// <summary>Pisca a linha em amarelo (#FEF9C3) por ~1.2s — feedback de "item adicionado agora".
    /// Usa DispatcherTimer (NUNCA Style.Animations — anima trava o Avalonia).</summary>
    public void Realcar()
    {
        BrushFundoLinha = FlashAmarelo;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1200) };
        timer.Tick += (s, _) =>
        {
            BrushFundoLinha = Brushes.Transparent;
            ((DispatcherTimer)s!).Stop();
        };
        timer.Start();
    }

    /// <summary>Disparado quando o total da linha muda — o form recalcula o total geral.</summary>
    public event Action? TotalAlterado;

    partial void OnQtdChanged(int value) => NotificarTotal();
    partial void OnVlrCustoUnitarioChanged(decimal value) => NotificarTotal();

    private void NotificarTotal()
    {
        OnPropertyChanged(nameof(VlrTotalItem));
        OnPropertyChanged(nameof(VlrTotalItemTexto));
        TotalAlterado?.Invoke();
    }

    private static int ParseInteiro(string? texto)
    {
        var limpo = (texto ?? string.Empty).Trim().Replace(".", "").Replace(",", "");
        return int.TryParse(limpo, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;
    }

    private static decimal ParseDecimal(string? texto)
    {
        var limpo = (texto ?? string.Empty).Trim().Replace(".", "").Replace(",", ".");
        return decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;
    }
}
