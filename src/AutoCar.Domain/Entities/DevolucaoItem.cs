using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Item (linha) de uma devolução: um produto da venda original que voltou ao estoque, com a
/// quantidade devolvida. Tabela filho de <see cref="Devolucao"/> (1:N) — apaga junto (Cascade).
/// Guarda um snapshot da descrição e do preço unitário praticado na venda (o valor da devolução
/// reflete o que foi vendido). Sem `cod_` (registro filho). Imutável após criação.
/// </summary>
public class DevolucaoItem : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected DevolucaoItem() { }

    public DevolucaoItem(Guid idProduto, string descricaoProduto, int qtd, decimal vlrUnitario)
    {
        if (qtd <= 0)
            throw new ArgumentException("A quantidade devolvida deve ser maior que zero.", nameof(qtd));
        if (vlrUnitario < 0)
            throw new ArgumentException("O valor unitário não pode ser negativo.", nameof(vlrUnitario));

        IdProduto = idProduto;
        DescricaoProduto = descricaoProduto.Trim();
        Qtd = qtd;
        VlrUnitario = vlrUnitario;
        VlrTotalItem = qtd * vlrUnitario;
    }

    /// <summary>FK para a devolução pai (preenchida pelo EF via a coleção da Devolucao).</summary>
    public Guid IdDevolucao { get; protected set; }

    /// <summary>FK para o produto devolvido (Restrict — não se apaga produto com histórico).</summary>
    public Guid IdProduto { get; protected set; }

    /// <summary>Snapshot da descrição do produto no momento da devolução.</summary>
    public string DescricaoProduto { get; protected set; } = string.Empty;

    /// <summary>Quantidade devolvida deste item. Inteira — autopeça não fraciona (igual ao estoque).</summary>
    public int Qtd { get; protected set; }

    /// <summary>Preço unitário praticado na venda original (snapshot — define o valor da devolução).</summary>
    public decimal VlrUnitario { get; protected set; }

    /// <summary>Total da linha devolvida: qtd × unitário.</summary>
    public decimal VlrTotalItem { get; protected set; }
}
