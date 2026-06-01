namespace AutoCar.Domain.Enums;

/// <summary>
/// Combustível do veículo em que a peça se aplica. Lista fixa (combo na tela) — algumas peças
/// só servem em certa versão de motor (ex: filtro/sonda específicos de flex vs diesel). Muitas
/// aplicações não dependem do combustível: para essas, <see cref="NaoAplica"/>. Persistido como int.
/// </summary>
public enum Combustivel
{
    /// <summary>Não se aplica / qualquer combustível. Padrão.</summary>
    NaoAplica = 0,

    /// <summary>Flex (álcool + gasolina).</summary>
    Flex = 1,

    /// <summary>Gasolina.</summary>
    Gasolina = 2,

    /// <summary>Diesel.</summary>
    Diesel = 3,

    /// <summary>Etanol (álcool).</summary>
    Etanol = 4,

    /// <summary>GNV (gás natural veicular).</summary>
    GNV = 5,
}
