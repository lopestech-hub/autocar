using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Categoria/grupo de uma peça (Filtros, Suspensão, Elétrica...). Cadastro mestre auxiliar:
/// id + cod_categoria. Referenciada por Produto via FK para organizar o catálogo.
/// </summary>
public class CategoriaProduto : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected CategoriaProduto() { }

    public CategoriaProduto(string descricao)
    {
        Descricao = descricao.Trim().ToUpperInvariant();
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodCategoria { get; protected set; }

    public string Descricao { get; protected set; } = string.Empty;

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(string descricao)
    {
        Descricao = descricao.Trim().ToUpperInvariant();
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
