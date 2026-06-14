using System;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Threading;
using AutoCar.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoCar.Desktop.ViewModels.Service;

/// <summary>
/// Linha editável do grid de itens da OS. Carrega o <see cref="Tipo"/> (peça/serviço) e a FK
/// correspondente (produto OU serviço) como snapshot read-only — vêm da escolha no seletor (F2/F4).
/// Quantidade (inteira), unitário e desconto são editáveis como texto e recalculam o total da linha.
/// </summary>
public partial class OrdemServicoItemViewModel : ViewModelBase
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    public OrdemServicoItemViewModel(
        TipoItemOrdemServico tipo, Guid? idProduto, Guid? idServico,
        string descricaoItem, int qtd, decimal vlrUnitario, decimal vlrDesconto,
        int? codProduto = null, string? codFabricante = null)
    {
        Tipo = tipo;
        IdProduto = idProduto;
        IdServico = idServico;
        DescricaoItem = descricaoItem;
        CodProduto = codProduto;
        CodFabricante = codFabricante;
        _qtd = qtd;
        _vlrUnitario = vlrUnitario;
        _vlrDesconto = vlrDesconto;
    }

    /// <summary>Tipo da linha: Peça (estoque) ou Serviço (mão de obra). Snapshot — não muda na linha.</summary>
    public TipoItemOrdemServico Tipo { get; }

    /// <summary>FK do produto — preenchida só nas linhas de peça (snapshot).</summary>
    public Guid? IdProduto { get; }

    /// <summary>FK do serviço — preenchida só nas linhas de serviço (snapshot).</summary>
    public Guid? IdServico { get; }

    /// <summary>Descrição (do produto ou serviço) no momento da inclusão (snapshot, read-only).</summary>
    public string DescricaoItem { get; }

    /// <summary>Código legível do produto (cod_produto) — só nas linhas de peça. Null em serviço.</summary>
    public int? CodProduto { get; }

    /// <summary>Referência do fabricante (cod_fabricante) — só nas linhas de peça. Null em serviço.</summary>
    public string? CodFabricante { get; }

    /// <summary>Código a exibir na grade: o número do produto nas peças, vazio nas linhas de serviço.</summary>
    public string CodProdutoTexto => CodProduto?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

    /// <summary>Referência a exibir na grade: o cod. fabricante das peças, vazio se não houver/for serviço.</summary>
    public string CodFabricanteTexto => CodFabricante ?? string.Empty;

    /// <summary>True quando a linha é uma peça (rótulo/realce de tipo na tela).</summary>
    public bool EhPeca => Tipo == TipoItemOrdemServico.Peca;

    /// <summary>Rótulo curto do tipo, exibido na coluna TIPO do grid.</summary>
    public string TipoRotulo => EhPeca ? "PEÇA" : "SERVIÇO";

    [ObservableProperty] private int _qtd;
    [ObservableProperty] private decimal _vlrUnitario;
    [ObservableProperty] private decimal _vlrDesconto;

    /// <summary>Quantidade como texto editável. Inteira (peça/serviço não fracionam).</summary>
    public string QtdTexto
    {
        get => Qtd.ToString(CultureInfo.InvariantCulture);
        set { Qtd = ParseInteiro(value); OnPropertyChanged(); }
    }

    /// <summary>Valor unitário como texto editável (snapshot, ajustável).</summary>
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

    /// <summary>True quando a linha tem desconto — realça a célula de desconto (âmbar).</summary>
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

    partial void OnQtdChanged(int value) => NotificarTotal();
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

    private static int ParseInteiro(string? texto)
    {
        var limpo = (texto ?? string.Empty).Trim();
        return int.TryParse(limpo, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;
    }

    private static decimal ParseDecimal(string? texto)
    {
        var limpo = (texto ?? string.Empty).Trim().Replace(".", "").Replace(",", ".");
        return decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;
    }
}
