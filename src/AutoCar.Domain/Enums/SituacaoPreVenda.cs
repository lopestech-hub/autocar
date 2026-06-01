namespace AutoCar.Domain.Enums;

/// <summary>
/// Situação (ciclo de vida) de uma pré-venda. A pré-venda nasce Aberta e pode ser
/// editada; ao ser Faturada vira venda (a baixa de estoque acontece nesse momento,
/// na Fase de Estoque) e torna-se imutável; Cancelada encerra sem efeito. Persistida como int.
/// </summary>
public enum SituacaoPreVenda
{
    /// <summary>Documento provisório, editável. Estado inicial.</summary>
    Aberta = 1,

    /// <summary>Faturada (virou venda). Imutável — baixa de estoque ocorre aqui.</summary>
    Faturada = 2,

    /// <summary>Cancelada. Encerrada sem efeito; não pode ser reaberta.</summary>
    Cancelada = 3,
}
