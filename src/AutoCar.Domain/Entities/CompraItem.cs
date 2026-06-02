using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Item (linha) de uma compra: um produto que entrou no estoque, com a quantidade e o custo unitário
/// pagos ao fornecedor. Tabela filho de <see cref="Compra"/> (1:N) — apaga junto (Cascade). Guarda um
/// snapshot da descrição do produto. Sem `cod_` (registro filho). Imutável após criação.
/// </summary>
public class CompraItem : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected CompraItem() { }

    public CompraItem(Guid idProduto, string descricaoProduto, int qtd, decimal vlrCustoUnitario)
    {
        if (qtd <= 0)
            throw new ArgumentException("A quantidade comprada deve ser maior que zero.", nameof(qtd));
        if (vlrCustoUnitario < 0)
            throw new ArgumentException("O custo unitário não pode ser negativo.", nameof(vlrCustoUnitario));

        IdProduto = idProduto;
        DescricaoProduto = descricaoProduto.Trim();
        Qtd = qtd;
        VlrCustoUnitario = vlrCustoUnitario;
        VlrTotalItem = qtd * vlrCustoUnitario;
    }

    /// <summary>FK para a compra pai (preenchida pelo EF via a coleção da Compra).</summary>
    public Guid IdCompra { get; protected set; }

    /// <summary>FK para o produto comprado (Restrict — não se apaga produto com histórico).</summary>
    public Guid IdProduto { get; protected set; }

    /// <summary>Snapshot da descrição do produto no momento da compra.</summary>
    public string DescricaoProduto { get; protected set; } = string.Empty;

    /// <summary>Quantidade comprada deste item. Inteira — autopeça não fraciona (igual ao estoque).</summary>
    public int Qtd { get; protected set; }

    /// <summary>Custo unitário pago ao fornecedor (snapshot — define o valor da compra).</summary>
    public decimal VlrCustoUnitario { get; protected set; }

    /// <summary>Total da linha: qtd × custo unitário.</summary>
    public decimal VlrTotalItem { get; protected set; }
}
