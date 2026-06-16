using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Grupo/família de peça dentro de uma categoria (Amortecedor, Mola, Pastilha, Disco...).
/// Nível intermediário do catálogo: Categoria → Grupo → Produto. Cadastro mestre auxiliar
/// editável (id + cod_grupo). Pertence a uma <see cref="CategoriaProduto"/> (FK obrigatória):
/// AMORTECEDOR vive dentro de SUSPENSÃO. Descrição única DENTRO da categoria.
/// </summary>
public class GrupoProduto : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected GrupoProduto() { }

    public GrupoProduto(string descricao, Guid idCategoria)
    {
        Descricao = descricao.Trim().ToUpperInvariant();
        IdCategoria = idCategoria;
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodGrupo { get; protected set; }

    public string Descricao { get; protected set; } = string.Empty;

    /// <summary>Categoria à qual o grupo pertence (obrigatória).</summary>
    public Guid IdCategoria { get; protected set; }

    /// <summary>Navegação para a categoria (carregada só quando necessário, ex: exibição com nome).</summary>
    public CategoriaProduto? Categoria { get; protected set; }

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(string descricao, Guid idCategoria)
    {
        Descricao = descricao.Trim().ToUpperInvariant();
        IdCategoria = idCategoria;
        MarcarAtualizada();
    }

    public void Ativar()
    {
        FlgAtivo = true;
        MarcarAtualizada();
    }

    public void Inativar()
    {
        FlgAtivo = false;
        MarcarAtualizada();
    }
}
