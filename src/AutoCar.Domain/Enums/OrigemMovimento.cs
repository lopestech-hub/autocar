namespace AutoCar.Domain.Enums;

/// <summary>
/// Origem de um movimento de estoque — de onde a entrada/saída veio. Dado estruturado (não texto
/// livre): permite rastrear o documento que gerou a movimentação e distinguir movimento de sistema
/// (venda, compra) de ajuste manual de balcão. Persistido como int.
/// </summary>
public enum OrigemMovimento
{
    /// <summary>Movimento lançado manualmente na tela de estoque (entrada/saída/ajuste avulso).</summary>
    Manual = 1,

    /// <summary>Baixa gerada pelo faturamento de uma venda/pré-venda.</summary>
    Venda = 2,

    /// <summary>Entrada gerada por um documento de compra (Fase futura).</summary>
    Compra = 3,

    /// <summary>Devolução (entrada por devolução de cliente / Fase futura).</summary>
    Devolucao = 4,
}
