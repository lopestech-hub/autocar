namespace AutoCar.Domain.Enums;

/// <summary>
/// Lado em que a peça se aplica no veículo. Dimensão independente da <see cref="PosicaoPeca"/>
/// (eixo): uma peça pode ter eixo sem lado (pastilha dianteira serve nos dois lados), lado sem
/// eixo (farol direito), ambos (coxim dianteiro esquerdo) ou nenhum (óleo, filtro). Cada lado é
/// um produto separado no cadastro — aqui é só atributo descritivo/filtro. Persistido como int.
/// </summary>
public enum LadoPeca
{
    /// <summary>Lado não se aplica (peça sem distinção de lado). Padrão.</summary>
    NaoAplica = 0,

    /// <summary>Lado esquerdo do veículo.</summary>
    Esquerdo = 1,

    /// <summary>Lado direito do veículo.</summary>
    Direito = 2,
}
