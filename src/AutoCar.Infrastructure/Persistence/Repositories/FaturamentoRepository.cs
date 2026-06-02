using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core do faturamento transacional (Pré-venda + baixa de estoque). Toda a operação
/// roda num ÚNICO DbContext com um único SaveChanges — logo, uma única transação no PostgreSQL: ou
/// fatura e baixa tudo, ou nada. Se um item não tem saldo, o domínio lança antes do save e nada é
/// persistido (rollback). O xmin de cada saldo é comparado no save; se outro terminal moveu um dos
/// produtos no meio, o UPDATE afeta 0 linhas → DbUpdateConcurrencyException → ConcorrenciaException.
/// </summary>
public class FaturamentoRepository : IFaturamentoRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public FaturamentoRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task FaturarComBaixaEstoqueAsync(
        Guid idPreVenda, Guid idUsuario, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Pré-venda rastreada com os itens (o change tracker detecta a mudança de situação).
        var preVenda = await db.PreVendas
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == idPreVenda, ct)
            ?? throw new NaoEncontradoException("Pré-venda não encontrada.");

        // Fatura (valida Aberta + ≥1 item — invariante do agregado). Lança se já faturada/cancelada.
        preVenda.Faturar();

        // Baixa o estoque de cada item. Mantém os saldos já carregados em cache local para que duas
        // linhas do mesmo produto vejam a baixa uma da outra (a segunda parte do saldo já reduzido).
        var saldosPorProduto = new Dictionary<Guid, EstoqueProduto>();

        foreach (var item in preVenda.Itens)
        {
            var estoque = await EstoquePersistencia.ObterOuCriarSaldoRastreadoAsync(
                db, saldosPorProduto, item.IdProduto, ct);

            // Qtd da pré-venda é decimal (modelo do documento); estoque é inteiro (autopeça não fraciona).
            var qtd = QuantidadeEstoque.DeDocumento(item.Qtd, item.DescricaoProduto);

            // Saída com origem rastreável (Venda + documento). O número e o id vêm da pré-venda — o
            // histórico de estoque mostra "Venda nº X" e permite chegar ao documento. A observação
            // livre fica vazia (origem não é observação). Lança se faltar saldo (invariante do estoque).
            var movimento = estoque.Movimentar(
                TipoMovimentoEstoque.Saida, qtd, idUsuario,
                observacao: null,
                origem: OrigemMovimento.Venda,
                idDocumentoOrigem: preVenda.Id,
                codDocumentoOrigem: preVenda.CodPreVenda);
            await db.MovimentosEstoque.AddAsync(movimento, ct);
        }

        // Único SaveChanges = única transação: UPDATE pre_venda + N×(UPDATE estoque_produto WHERE xmin
        // + INSERT movimento_estoque). Qualquer falha aborta tudo; conflito de xmin vira ConcorrenciaException.
        await EstoquePersistencia.SalvarTraduzindoConcorrenciaAsync(
            db, "O saldo de um dos produtos foi alterado por outro terminal durante o faturamento.", ct);
    }
}
