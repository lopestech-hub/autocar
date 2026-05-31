namespace AutoCar.Domain.Enums;

/// <summary>
/// Unidade de medida do produto. Lista fixa (combo na tela) para evitar digitação
/// divergente do mesmo valor ("UN" vs "un" vs "unid"). Persistida como int.
/// </summary>
public enum UnidadeMedida
{
    /// <summary>Unidade.</summary>
    UN = 1,

    /// <summary>Peça.</summary>
    PC = 2,

    /// <summary>Caixa.</summary>
    CX = 3,

    /// <summary>Jogo (conjunto de peças vendidas juntas).</summary>
    JG = 4,

    /// <summary>Par.</summary>
    PAR = 5,

    /// <summary>Kit.</summary>
    KIT = 6,

    /// <summary>Litro.</summary>
    L = 7,

    /// <summary>Quilograma.</summary>
    KG = 8,

    /// <summary>Metro.</summary>
    M = 9,
}
