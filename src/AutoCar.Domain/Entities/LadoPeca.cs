using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Lado em que a peça se aplica no veículo (Esquerdo, Direito...). Dimensão independente
/// da <see cref="PosicaoPeca"/> (eixo). Cadastro mestre auxiliar editável (id + cod_lado):
/// substitui o enum fixo anterior. Referenciada por Produto via FK opcional (peça sem
/// distinção de lado simplesmente não tem lado).
/// </summary>
public class LadoPeca : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected LadoPeca() { }

    public LadoPeca(string descricao)
    {
        Descricao = descricao.Trim().ToUpperInvariant();
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodLado { get; protected set; }

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
