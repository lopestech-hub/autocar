using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Equivalência (cruzamento / cross-reference) de um produto com a peça de outra marca.
/// Tabela filho de <see cref="Produto"/> (1:N) — um produto pode equivaler a várias referências
/// de outras marcas. A referência é registrada como marca + código (texto): a peça equivalente NÃO
/// precisa existir no cadastro. Quando ela passa a ser um produto da loja, o vínculo opcional
/// <see cref="IdProdutoEquivalente"/> é preenchido (automaticamente, pelo casamento do código) e
/// a equivalência passa a cruzar com estoque/preço reais. Sem `cod_` (registro filho). Editada
/// junto com o produto pai.
/// </summary>
public class ProdutoSimilar : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected ProdutoSimilar() { }

    public ProdutoSimilar(
        string marca,
        string codReferencia,
        Guid? idProdutoEquivalente,
        string? observacao)
    {
        Marca = marca.Trim().ToUpperInvariant();
        CodReferencia = NormalizarReferencia(codReferencia);
        IdProdutoEquivalente = idProdutoEquivalente;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim().ToUpperInvariant();
    }

    /// <summary>FK para o produto pai (preenchida pelo EF via a coleção do Produto).</summary>
    public Guid IdProduto { get; protected set; }

    /// <summary>Marca da peça equivalente (ex: NAKATA, MONROE). CAIXA ALTA.</summary>
    public string Marca { get; protected set; } = string.Empty;

    /// <summary>Código/referência da peça equivalente nessa marca. Normalizado (Trim + CAIXA ALTA),
    /// pois é usado no casamento com o <c>cod_fabricante</c> de um produto.</summary>
    public string CodReferencia { get; protected set; } = string.Empty;

    /// <summary>Vínculo opcional ao produto equivalente, quando ele existe no cadastro. Null = só
    /// referência externa (a loja ainda não compra essa marca).</summary>
    public Guid? IdProdutoEquivalente { get; protected set; }

    /// <summary>Detalhe livre (opcional).</summary>
    public string? Observacao { get; protected set; }

    /// <summary>Liga (ou desliga) a equivalência a um produto do cadastro. Usado pelo vínculo
    /// automático por referência, quando a marca equivalente passa a ser um produto da loja.</summary>
    public void VincularProduto(Guid? idProdutoEquivalente)
    {
        IdProdutoEquivalente = idProdutoEquivalente;
        MarcarAtualizada();
    }

    // Referência normaliza espaços e CAIXA ALTA — para casar com cod_fabricante de forma estável.
    private static string NormalizarReferencia(string valor) => valor.Trim().ToUpperInvariant();
}
