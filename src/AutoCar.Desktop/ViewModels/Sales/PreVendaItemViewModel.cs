using System;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoCar.Desktop.ViewModels.Sales;

/// <summary>
/// Linha editável do grid de itens da pré-venda. Descrição e produto são snapshot
/// (read-only na linha; vêm da escolha no Catálogo). Quantidade, unitário e desconto
/// são editáveis como texto (parse tolerante BR) e recalculam o total da linha.
/// </summary>
public partial class PreVendaItemViewModel : ViewModelBase
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public PreVendaItemViewModel(Guid idProduto, string descricaoProduto, decimal qtd, decimal vlrUnitario, decimal vlrDesconto,
        int? codProduto = null, string? codFabricante = null)
    {
        IdProduto = idProduto;
        DescricaoProduto = descricaoProduto;
        CodProduto = codProduto;
        CodFabricante = codFabricante;
        _qtd = qtd;
        _vlrUnitario = vlrUnitario;
        _vlrDesconto = vlrDesconto;
    }

    /// <summary>FK do produto (snapshot — não muda na linha).</summary>
    public Guid IdProduto { get; }

    /// <summary>Descrição do produto no momento da inclusão (snapshot, read-only).</summary>
    public string DescricaoProduto { get; }

    /// <summary>Código legível do produto (cod_produto). Exibido na grade.</summary>
    public int? CodProduto { get; }

    /// <summary>Referência do fabricante (cod_fabricante). Exibido na grade.</summary>
    public string? CodFabricante { get; }

    /// <summary>Código a exibir na grade (vazio se não houver).</summary>
    public string CodProdutoTexto => CodProduto?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>Referência a exibir na grade (vazio se não houver).</summary>
    public string CodFabricanteTexto => CodFabricante ?? string.Empty;

    [ObservableProperty] private decimal _qtd;
    [ObservableProperty] private decimal _vlrUnitario;
    [ObservableProperty] private decimal _vlrDesconto;

    /// <summary>Quantidade como texto editável.</summary>
    public string QtdTexto
    {
        get => Qtd.ToString("N2", PtBr);
        set { Qtd = ParseDecimal(value); OnPropertyChanged(); }
    }

    /// <summary>Valor unitário como texto editável (snapshot do preço, ajustável pelo vendedor).</summary>
    public string VlrUnitarioTexto
    {
        get => VlrUnitario.ToString("N2", PtBr);
        set { VlrUnitario = ParseDecimal(value); OnPropertyChanged(); }
    }

    /// <summary>Desconto da linha como texto editável.</summary>
    public string VlrDescontoTexto
    {
        get => VlrDesconto.ToString("N2", PtBr);
        set { VlrDesconto = ParseDecimal(value); OnPropertyChanged(); }
    }

    /// <summary>Total da linha: (qtd × unitário) − desconto, nunca negativo. Read-only na tela.</summary>
    public decimal VlrTotalItem => Math.Max(0, Qtd * VlrUnitario - VlrDesconto);

    public string VlrTotalItemTexto => VlrTotalItem.ToString("N2", PtBr);

    /// <summary>True quando a linha tem desconto — usado para realçar a célula de desconto (âmbar).</summary>
    public bool TemDesconto => VlrDesconto > 0;

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

    partial void OnQtdChanged(decimal value) => NotificarTotal();
    partial void OnVlrUnitarioChanged(decimal value) => NotificarTotal();
    partial void OnVlrDescontoChanged(decimal value)
    {
        OnPropertyChanged(nameof(TemDesconto));
        NotificarTotal();
    }

    private void NotificarTotal()
    {
        OnPropertyChanged(nameof(VlrTotalItem));
        OnPropertyChanged(nameof(VlrTotalItemTexto));
        TotalAlterado?.Invoke();
    }

    private static decimal ParseDecimal(string? texto)
    {
        var limpo = (texto ?? string.Empty).Trim().Replace(".", "").Replace(",", ".");
        return decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;
    }
}
