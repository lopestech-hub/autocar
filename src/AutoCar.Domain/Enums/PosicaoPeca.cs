namespace AutoCar.Domain.Enums;

/// <summary>
/// Posição (eixo) em que a peça se aplica no veículo. Distingue itens que existem em
/// versão dianteira e traseira (freio, suspensão, etc.). Muitas peças não têm posição
/// (óleo, filtro, correia) — para essas, <see cref="NaoAplica"/>. Persistida como int.
/// </summary>
public enum PosicaoPeca
{
    /// <summary>Posição não se aplica (peça sem distinção de eixo). Padrão.</summary>
    NaoAplica = 0,

    /// <summary>Eixo dianteiro.</summary>
    Dianteira = 1,

    /// <summary>Eixo traseiro.</summary>
    Traseira = 2,
}
