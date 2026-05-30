using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Marca/fabricante de uma peça (Bosch, Cofap, NGK...). Cadastro mestre auxiliar:
/// id + cod_marca. Referenciada por Produto via FK para padronizar a nomenclatura
/// e evitar digitação divergente da mesma marca.
/// </summary>
public class Marca : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected Marca() { }

    public Marca(string descricao)
    {
        Descricao = descricao.Trim().ToUpperInvariant();
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodMarca { get; protected set; }

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
