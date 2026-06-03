namespace AutoCar.Domain.Enums;

/// <summary>
/// Discriminador da linha de uma Ordem de Serviço. Uma OS mistura, no mesmo documento, peças
/// (que baixam do estoque, como uma venda) e mão de obra / serviços (valor cobrado, sem estoque).
/// O tipo define qual FK da linha está preenchida (produto ou serviço) e se ela toca o estoque
/// ao faturar. Persistido como int.
/// </summary>
public enum TipoItemOrdemServico
{
    /// <summary>Peça/produto — FK id_produto. Baixa do estoque ao faturar a OS.</summary>
    Peca = 1,

    /// <summary>Mão de obra / serviço — FK id_servico. Não toca o estoque.</summary>
    Servico = 2,
}
