using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core da devolução transacional: salva o documento de devolução e dá ENTRADA no
/// estoque de cada item devolvido. Usa uma transação explícita com DOIS SaveChanges (o cod_devolucao,
/// identity do banco, só fica disponível após o 1º INSERT e os movimentos precisam dele) — ou registra
/// tudo, ou nada. Reusa o EstoquePersistencia (obter-ou-criar saldo com cache + tradução de concorrência)
/// — mesma fonte de invariantes do estoque que o faturamento.
/// </summary>
public class DevolucaoRepository : IDevolucaoRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public DevolucaoRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task RegistrarComEntradaEstoqueAsync(Devolucao devolucao, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Transação explícita para manter a atomicidade COM dois SaveChanges: o cod_devolucao é gerado
        // pelo banco (identity) e só fica disponível após o INSERT do documento — e os movimentos de
        // estoque precisam desse número para registrar a origem ("Devolução nº X"). Então: salva o
        // documento (popula o cod) → cria os movimentos com o cod real → salva. Tudo na mesma transação:
        // qualquer falha (ex: saldo, xmin) faz rollback de tudo.
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // 1) Insere o documento + itens (Cascade). Após o save, devolucao.CodDevolucao está populado.
        await db.Devolucoes.AddAsync(devolucao, ct);
        await db.SaveChangesAsync(ct);

        // 2) Repõe o estoque de cada item, já com o número do documento de origem correto.
        var saldosPorProduto = new Dictionary<Guid, EstoqueProduto>();
        foreach (var item in devolucao.Itens)
        {
            var estoque = await EstoquePersistencia.ObterOuCriarSaldoRastreadoAsync(
                db, saldosPorProduto, item.IdProduto, ct);
            var movimento = estoque.Movimentar(
                TipoMovimentoEstoque.Entrada, item.Qtd, devolucao.IdUsuario,
                observacao: null,
                origem: OrigemMovimento.Devolucao,
                idDocumentoOrigem: devolucao.Id,
                codDocumentoOrigem: devolucao.CodDevolucao);
            await db.MovimentosEstoque.AddAsync(movimento, ct);
        }

        // Conflito de xmin (saldo alterado por outro terminal) vira ConcorrenciaException.
        await EstoquePersistencia.SalvarTraduzindoConcorrenciaAsync(
            db, "O saldo de um dos produtos foi alterado por outro terminal durante a devolução.", ct);

        await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> TotalDevolvidoPorProdutoAsync(
        Guid idPreVenda, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Soma a quantidade já devolvida por produto, somando os itens de todas as devoluções da venda.
        var devolucoesDaVenda = db.Devolucoes
            .AsNoTracking()
            .Where(d => d.IdPreVenda == idPreVenda)
            .Select(d => d.Id);

        var totais = await db.Set<DevolucaoItem>()
            .AsNoTracking()
            .Where(i => devolucoesDaVenda.Contains(i.IdDevolucao))
            .GroupBy(i => i.IdProduto)
            .Select(g => new { IdProduto = g.Key, Total = g.Sum(i => i.Qtd) })
            .ToListAsync(ct);

        return totais.ToDictionary(x => x.IdProduto, x => x.Total);
    }
}
