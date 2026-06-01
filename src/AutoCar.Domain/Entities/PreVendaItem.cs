using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Item (linha) de uma pré-venda. Tabela filho de <see cref="PreVenda"/> (1:N) — apaga junto
/// com o documento (Cascade). Guarda um <b>snapshot</b> da descrição e do preço do produto no
/// momento em que foi adicionado: o documento registra o que foi praticado na hora, mesmo que o
/// produto mude de preço depois. Sem `cod_` (registro filho). Editado junto com a pré-venda pai.
/// </summary>
public class PreVendaItem : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected PreVendaItem() { }

    public PreVendaItem(
        Guid idProduto,
        string descricaoProduto,
        decimal qtd,
        decimal vlrUnitario,
        decimal vlrDesconto)
    {
        IdProduto = idProduto;
        DescricaoProduto = descricaoProduto.Trim();
        DefinirValores(qtd, vlrUnitario, vlrDesconto);
    }

    /// <summary>FK para a pré-venda pai (preenchida pelo EF via a coleção da PreVenda).</summary>
    public Guid IdPreVenda { get; protected set; }

    /// <summary>FK para o produto (Restrict — não se apaga produto usado em documento).</summary>
    public Guid IdProduto { get; protected set; }

    /// <summary>Snapshot da descrição do produto no momento da inclusão.</summary>
    public string DescricaoProduto { get; protected set; } = string.Empty;

    /// <summary>Quantidade. Decimal para permitir fração (KG, L, M).</summary>
    public decimal Qtd { get; protected set; }

    /// <summary>Preço unitário praticado (snapshot do produto, editável pelo vendedor).</summary>
    public decimal VlrUnitario { get; protected set; }

    /// <summary>Desconto da linha em valor (R$). Padrão 0.</summary>
    public decimal VlrDesconto { get; protected set; }

    /// <summary>Total da linha: (qtd × unitário) − desconto. Calculado no domínio e persistido.</summary>
    public decimal VlrTotalItem { get; protected set; }

    /// <summary>Subtotal antes do desconto da linha (qtd × unitário).</summary>
    public decimal Subtotal => Qtd * VlrUnitario;

    /// <summary>Redefine quantidade, unitário e desconto, recalculando o total da linha.</summary>
    public void DefinirValores(decimal qtd, decimal vlrUnitario, decimal vlrDesconto)
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
