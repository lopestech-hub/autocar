using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core do repositório de estoque. Usa <see cref="IDbContextFactory{TContext}"/>
/// (contexto novo por operação) — padrão dos repositórios que precisam de estado fresco no desktop.
/// O ponto crítico é <see cref="MovimentarAsync"/>: carrega o saldo rastreado, aplica o movimento e
/// salva saldo + movimento na MESMA transação. O xmin do saldo detecta concorrência (dois terminais
/// no último item) — o SaveChanges lança DbUpdateConcurrencyException, tratada na Application.
/// </summary>
public class EstoqueRepository : IEstoqueRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public EstoqueRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<IReadOnlyList<SaldoProdutoLeitura>> ListarSaldosAsync(
        string? filtro, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Parte dos produtos ativos (left join no saldo) — produto sem movimento ainda aparece com
        // saldo 0. Projeta o DTO de leitura DIRETO no banco (descrição/código/unidade + saldo numa
        // única consulta) — evita o N+1 de buscar cada produto depois.
        var query = from p in db.Produtos.AsNoTracking()
                    where p.FlgAtivo
                    join e in db.EstoqueProdutos.AsNoTracking()
                        on p.Id equals e.IdProduto into estoque
                    from e in estoque.DefaultIfEmpty()
                    select new { Produto = p, Estoque = e };

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Produto.Descricao, $"%{termo}%") ||
                (x.Produto.CodBarras != null && EF.Functions.ILike(x.Produto.CodBarras, $"%{termo}%")) ||
                (x.Produto.CodFabricante != null && EF.Functions.ILike(x.Produto.CodFabricante, $"%{termo}%")) ||
                x.Produto.CodProduto.ToString() == termo);
        }

        return await query
            .OrderBy(x => x.Produto.Descricao)
            .Select(x => new SaldoProdutoLeitura(
                x.Produto.Id,
                x.Produto.CodProduto,
                x.Produto.Descricao,
                x.Produto.Unidade,
                x.Estoque != null ? x.Estoque.QtdSaldo : 0,
                x.Estoque != null ? x.Estoque.QtdReservada : 0))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MovimentoEstoque>> ListarMovimentosAsync(
        Guid idProduto, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.MovimentosEstoque
            .AsNoTracking()
            .Where(m => m.IdProduto == idProduto)
            .OrderByDescending(m => m.CodMovimento)
            .ToListAsync(ct);
    }

    public async Task MovimentarAsync(
        Guid idProduto,
        Func<EstoqueProduto, MovimentoEstoque> movimentar,
        CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Carrega o saldo RASTREADO (sem AsNoTracking) — o xmin lido aqui é o que o UPDATE vai
        // comparar no save. Se ainda não existe registro de saldo, cria zerado e marca como Added.
        var estoque = await db.EstoqueProdutos
            .FirstOrDefaultAsync(e => e.IdProduto == idProduto, ct);

        if (estoque is null)
        {
            estoque = new EstoqueProduto(idProduto);
            await db.EstoqueProdutos.AddAsync(estoque, ct);
        }

        // Aplica a regra de domínio (valida saldo, ajusta a quantidade) e obtém o movimento gerado.
        var movimento = movimentar(estoque);
        await db.MovimentosEstoque.AddAsync(movimento, ct);

        // Salva saldo (UPDATE com WHERE xmin) + movimento (INSERT) atomicamente. Concorrência: se
        // outro terminal alterou o saldo desde a leitura, o UPDATE afeta 0 linhas → DbUpdate-
        // ConcurrencyException. Traduz para a exceção neutra de domínio (ConcorrenciaException) —
        // a Application trata sem conhecer o EF Core (Clean Architecture).
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcorrenciaException(
                "O saldo deste produto foi alterado por outro terminal.", ex);
        }
    }
}
