using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core da compra transacional: salva o documento de compra e dá ENTRADA no estoque de
/// cada item comprado. Usa uma transação explícita com DOIS SaveChanges (o cod_compra, identity do banco,
/// só fica disponível após o 1º INSERT e os movimentos precisam dele para a origem "Compra nº X") — ou
/// registra tudo, ou nada. Reusa o EstoquePersistencia (obter-ou-criar saldo com cache + tradução de
/// concorrência) — mesma fonte de invariantes do estoque que o faturamento e a devolução.
/// </summary>
public class CompraRepository : ICompraRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public CompraRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task RegistrarComEntradaEstoqueAsync(Compra compra, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Transação explícita para manter a atomicidade COM dois SaveChanges: o cod_compra é gerado pelo
        // banco (identity) e só fica disponível após o INSERT do documento — e os movimentos de estoque
        // precisam desse número para registrar a origem ("Compra nº X"). Então: salva o documento (popula
        // o cod) → cria os movimentos com o cod real → salva. Tudo na mesma transação: qualquer falha
        // (ex: saldo, xmin) faz rollback de tudo.
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // 1) Insere o documento + itens (Cascade). Após o save, compra.CodCompra está populado.
        await db.Compras.AddAsync(compra, ct);
        await db.SaveChangesAsync(ct);

        // 2) Dá entrada no estoque de cada item, já com o número do documento de origem correto.
        var saldosPorProduto = new Dictionary<Guid, EstoqueProduto>();
        foreach (var item in compra.Itens)
        {
            var estoque = await EstoquePersistencia.ObterOuCriarSaldoRastreadoAsync(
                db, saldosPorProduto, item.IdProduto, ct);
            var movimento = estoque.Movimentar(
                TipoMovimentoEstoque.Entrada, item.Qtd, compra.IdUsuario,
                observacao: null,
                origem: OrigemMovimento.Compra,
                idDocumentoOrigem: compra.Id,
                codDocumentoOrigem: compra.CodCompra);
            await db.MovimentosEstoque.AddAsync(movimento, ct);
        }

        // Conflito de xmin (saldo alterado por outro terminal) vira ConcorrenciaException.
        await EstoquePersistencia.SalvarTraduzindoConcorrenciaAsync(
            db, "O saldo de um dos produtos foi alterado por outro terminal durante a compra.", ct);

        await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<Compra>> ListarAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Carrega cabeçalho + fornecedor (para o nome) + itens (para a contagem), mais recentes primeiro.
        return await db.Compras
            .AsNoTracking()
            .Include(c => c.Fornecedor)
            .Include(c => c.Itens)
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync(ct);
    }
}
