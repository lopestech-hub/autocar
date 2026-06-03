using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Item (linha) de uma Ordem de Serviço. Tabela filho de <see cref="OrdemServico"/> (1:N) — apaga
/// junto com o documento (Cascade). Uma única tabela serve aos dois tipos de linha (peça e serviço),
/// distinguidos por <see cref="Tipo"/>: linha de <b>Peça</b> aponta para um produto (e baixa do estoque
/// ao faturar); linha de <b>Serviço</b> aponta para um serviço do catálogo (mão de obra, sem estoque).
///
/// Guarda um <b>snapshot</b> da descrição e do valor unitário no momento da inclusão. A invariante
/// "exatamente uma FK por tipo" (Peça ⇒ id_produto; Serviço ⇒ id_servico) é garantida pelos factory
/// methods <see cref="DePeca"/> e <see cref="DeServico"/> — não há construtor público que a viole.
/// Sem `cod_` (registro filho). Editado junto com a OS pai.
/// </summary>
public class OrdemServicoItem : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected OrdemServicoItem() { }

    private OrdemServicoItem(
        TipoItemOrdemServico tipo,
        Guid? idProduto,
        Guid? idServico,
        string descricaoItem,
        int qtd,
        decimal vlrUnitario,
        decimal vlrDesconto)
    {
        Tipo = tipo;
        IdProduto = idProduto;
        IdServico = idServico;
        DescricaoItem = descricaoItem.Trim();
        DefinirValores(qtd, vlrUnitario, vlrDesconto);
    }

    /// <summary>Cria uma linha de PEÇA (aponta para um produto; baixa do estoque ao faturar).</summary>
    public static OrdemServicoItem DePeca(Guid idProduto, string descricaoProduto, int qtd, decimal vlrUnitario, decimal vlrDesconto) =>
        new(TipoItemOrdemServico.Peca, idProduto, idServico: null, descricaoProduto, qtd, vlrUnitario, vlrDesconto);

    /// <summary>Cria uma linha de SERVIÇO / mão de obra (aponta para um serviço; não toca o estoque).</summary>
    public static OrdemServicoItem DeServico(Guid idServico, string descricaoServico, int qtd, decimal vlrUnitario, decimal vlrDesconto) =>
        new(TipoItemOrdemServico.Servico, idProduto: null, idServico, descricaoServico, qtd, vlrUnitario, vlrDesconto);

    /// <summary>FK para a OS pai (preenchida pelo EF via a coleção da OrdemServico).</summary>
    public Guid IdOrdemServico { get; protected set; }

    /// <summary>Tipo da linha: Peça (estoque) ou Serviço (mão de obra).</summary>
    public TipoItemOrdemServico Tipo { get; protected set; }

    /// <summary>FK para o produto — preenchida só quando <see cref="Tipo"/> é Peça (Restrict).</summary>
    public Guid? IdProduto { get; protected set; }

    /// <summary>FK para o serviço — preenchida só quando <see cref="Tipo"/> é Serviço (Restrict).</summary>
    public Guid? IdServico { get; protected set; }

    /// <summary>Snapshot da descrição (do produto ou do serviço) no momento da inclusão.</summary>
    public string DescricaoItem { get; protected set; } = string.Empty;

    /// <summary>Quantidade. Inteira — peça é unidade fechada e mão de obra normalmente é 1.</summary>
    public int Qtd { get; protected set; }

    /// <summary>Valor unitário praticado (snapshot, editável na linha).</summary>
    public decimal VlrUnitario { get; protected set; }

    /// <summary>Desconto da linha em valor (R$). Padrão 0.</summary>
    public decimal VlrDesconto { get; protected set; }

    /// <summary>Total da linha: (qtd × unitário) − desconto. Calculado no domínio e persistido.</summary>
    public decimal VlrTotalItem { get; protected set; }

    /// <summary>True quando a linha é uma peça (baixa do estoque ao faturar).</summary>
    public bool EhPeca => Tipo == TipoItemOrdemServico.Peca;

    /// <summary>Redefine quantidade, unitário e desconto, recalculando o total da linha.</summary>
    public void DefinirValores(int qtd, decimal vlrUnitario, decimal vlrDesconto)
    {
        if (qtd <= 0)
            throw new ArgumentException("A quantidade do item deve ser maior que zero.", nameof(qtd));
        if (vlrUnitario < 0)
            throw new ArgumentException("O valor unitário não pode ser negativo.", nameof(vlrUnitario));
        if (vlrDesconto < 0)
            throw new ArgumentException("O desconto do item não pode ser negativo.", nameof(vlrDesconto));

        var subtotal = qtd * vlrUnitario;
        if (vlrDesconto > subtotal)
            throw new ArgumentException("O desconto do item não pode ser maior que o subtotal da linha.", nameof(vlrDesconto));

        Qtd = qtd;
        VlrUnitario = vlrUnitario;
        VlrDesconto = vlrDesconto;
        VlrTotalItem = subtotal - vlrDesconto;
        MarcarAtualizada();
    }
}
