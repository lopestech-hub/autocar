namespace AutoCar.Domain.Interfaces;

/// <summary>
/// Operação transacional de faturamento da Ordem de Serviço, cruzando dois agregados (OS + Estoque).
/// Existe como repositório próprio porque a baixa de estoque das peças ao faturar precisa ser ATÔMICA:
/// faturar o documento e dar saída de todas as PEÇAS acontecem no mesmo DbContext, com um único commit.
/// Se alguma peça não tiver saldo, nada é persistido (rollback total) — não existe "OS faturada com
/// estoque pela metade". Linhas de serviço (mão de obra) não tocam o estoque. A concorrência otimista
/// (xmin) de cada saldo continua valendo dentro dessa transação. Espelha o <see cref="IFaturamentoRepository"/>.
/// </summary>
public interface IFaturamentoOrdemServicoRepository
{
    /// <summary>
    /// Fatura a OS e baixa o estoque de todas as suas PEÇAS numa única transação. Carrega a OS rastreada,
    /// aplica <c>Faturar()</c> (valida Concluída + ≥1 item — invariante do domínio), e para cada linha do
    /// tipo Peça gera uma saída de estoque (saldo nunca negativo — invariante do estoque). Linhas de
    /// serviço são ignoradas (não há estoque). Tudo num único SaveChanges. Lança a exceção do domínio se
    /// faltar saldo (rollback) e <c>ConcorrenciaException</c> se outro terminal alterar um saldo no meio.
    /// </summary>
    /// <param name="idOrdemServico">Ordem de serviço a faturar.</param>
    /// <param name="idUsuario">Usuário que fatura (registrado em cada movimento de estoque).</param>
    Task FaturarComBaixaEstoqueAsync(Guid idOrdemServico, Guid idUsuario, CancellationToken ct = default);
}
