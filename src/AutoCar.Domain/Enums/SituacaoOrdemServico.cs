namespace AutoCar.Domain.Enums;

/// <summary>
/// Situação (ciclo de vida) de uma Ordem de Serviço da oficina. Mais rico que a pré-venda porque
/// reflete o trabalho da oficina: a OS nasce Aberta (orçamento/recepção), passa a EmAndamento quando
/// o mecânico começa, é Concluida quando o serviço termina (exige mecânico responsável), e Faturada
/// quando vira cobrança (a baixa de estoque das peças ocorre nesse momento). Cancelada encerra sem
/// efeito (só antes de faturar; não mexe em estoque). Persistida como int.
/// </summary>
public enum SituacaoOrdemServico
{
    /// <summary>Documento provisório, editável. Estado inicial (recepção/orçamento).</summary>
    Aberta = 1,

    /// <summary>Mecânico iniciou o trabalho. Ainda editável.</summary>
    EmAndamento = 2,

    /// <summary>Serviço terminado. Exige mecânico responsável definido. Ainda não cobrado.</summary>
    Concluida = 3,

    /// <summary>Faturada (virou cobrança). Imutável — baixa de estoque das peças ocorre aqui.</summary>
    Faturada = 4,

    /// <summary>Cancelada. Encerrada sem efeito; não pode ser reaberta. Não mexe em estoque.</summary>
    Cancelada = 5,
}
