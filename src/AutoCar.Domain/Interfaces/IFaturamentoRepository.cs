namespace AutoCar.Domain.Interfaces;

/// <summary>
/// Operação transacional de faturamento que cruza dois agregados (Pré-venda + Estoque). Existe como
/// repositório próprio porque a baixa de estoque ao faturar precisa ser ATÔMICA: faturar o documento
/// e dar saída de todos os itens acontecem no mesmo DbContext, com um único commit. Se qualquer item
/// não tiver saldo, nada é persistido (rollback total) — não existe "venda faturada com estoque pela
/// metade". A concorrência otimista (xmin) de cada saldo continua valendo dentro dessa transação.
/// </summary>
public interface IFaturamentoRepository
{
    /// <summary>
    /// Fatura a pré-venda e baixa o estoque de todos os seus itens numa única transação. Carrega a
    /// pré-venda rastreada, aplica <c>Faturar()</c> (valida Aberta + ≥1 item — invariante do domínio),
    /// e para cada item gera uma saída de estoque (saldo nunca negativo — invariante do estoque). Tudo
    /// num único SaveChanges. Lança a exceção do domínio se faltar saldo (rollback) e
    /// <c>ConcorrenciaException</c> se outro terminal alterar um saldo no meio.
    /// </summary>
    /// <param name="idPreVenda">Pré-venda a faturar.</param>
    /// <param name="idUsuario">Usuário que fatura (registrado em cada movimento de estoque).</param>
    Task FaturarComBaixaEstoqueAsync(Guid idPreVenda, Guid idUsuario, CancellationToken ct = default);
}
