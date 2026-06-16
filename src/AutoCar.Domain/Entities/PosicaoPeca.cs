using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Posição/eixo em que a peça se aplica no veículo (Dianteira, Traseira, Superior...).
/// Cadastro mestre auxiliar editável (id + cod_posicao): substitui o enum fixo anterior
/// para permitir adicionar valores sem recompilar. Referenciada por Produto via FK opcional
/// (peça sem distinção de eixo — óleo, filtro — simplesmente não tem posição).
/// </summary>
public class PosicaoPeca : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected PosicaoPeca() { }

    public PosicaoPeca(string descricao)
    {
        Descricao = descricao.Trim().ToUpperInvariant();
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodPosicao { get; protected set; }

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
