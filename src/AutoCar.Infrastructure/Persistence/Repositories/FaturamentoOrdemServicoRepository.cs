using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core do faturamento transacional da Ordem de Serviço (OS + baixa de estoque das
/// peças). Espelha o <see cref="FaturamentoRepository"/> da Pré-venda: toda a operação roda num ÚNICO
/// DbContext com um único SaveChanges — logo, uma única transação no PostgreSQL: ou fatura e baixa tudo,
/// ou nada. A diferença é que só as linhas de PEÇA baixam estoque; as de serviço (mão de obra) não têm
/// saldo a movimentar. Se uma peça não tem saldo, o domínio lança antes do save e nada é persistido
/// (rollback). O xmin de cada saldo é comparado no save; se outro terminal moveu um dos produtos no
/// meio, o UPDATE afeta 0 linhas → DbUpdateConcurrencyException → ConcorrenciaException.
/// </summary>
public class FaturamentoOrdemServicoRepository : IFaturamentoOrdemServicoRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public FaturamentoOrdemServicoRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task FaturarComBaixaEstoqueAsync(
        Guid idOrdemServico, Guid idUsuario, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // OS rastreada com os itens (o change tracker detecta a mudança de situação).
        var os = await db.OrdensServico
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == idOrdemServico, ct)
            ?? throw new NaoEncontradoException("Ordem de serviço não encontrada.");

        // Fatura (valida Concluída + ≥1 item — invariante do agregado). Lança se não estiver Concluída.
        os.Faturar();

        // Baixa o estoque só das PEÇAS. Mantém os saldos já carregados em cache local para que duas
        // linhas do mesmo produto vejam a baixa uma da outra (a segunda parte do saldo já reduzido).
        var saldosPorProduto = new Dictionary<Guid, EstoqueProduto>();

        foreach (var item in os.Itens.Where(i => i.EhPeca))
        {
            // EhPeca garante IdProduto preenchido (invariante do OrdemServicoItem).
            var idProduto = item.IdProduto!.Value;

            var estoque = await EstoquePersistencia.ObterOuCriarSaldoRastreadoAsync(
                db, saldosPorProduto, idProduto, ct);

            // Qtd da OS já é inteira (não há fração no agregado) — vai direto ao estoque.
            // Saída com origem rastreável (OrdemServico + documento): o histórico mostra "OS nº X".
            // Lança se faltar saldo (invariante do estoque).
            var movimento = estoque.Movimentar(
                TipoMovimentoEstoque.Saida, item.Qtd, idUsuario,
                observacao: null,
                origem: OrigemMovimento.OrdemServico,
                idDocumentoOrigem: os.Id,
                codDocumentoOrigem: os.CodOrdemServico);
            await db.MovimentosEstoque.AddAsync(movimento, ct);
        }

        // Único SaveChanges = única transação: UPDATE ordem_servico + N×(UPDATE estoque_produto WHERE xmin
        // + INSERT movimento_estoque). Qualquer falha aborta tudo; conflito de xmin vira ConcorrenciaException.
        await EstoquePersistencia.SalvarTraduzindoConcorrenciaAsync(
            db, "O saldo de um dos produtos foi alterado por outro terminal durante o faturamento.", ct);
    }
}
