using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core do repositório de pré-vendas. Usa <see cref="IDbContextFactory{TContext}"/>
/// para criar um DbContext novo por operação — mesmo padrão do ProdutoRepository, pelo mesmo motivo
/// (agregado com coleção filha 1:N editada no mesmo SaveChanges).
/// </summary>
public class PreVendaRepository : IPreVendaRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public PreVendaRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<PreVenda?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.PreVendas
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<PreVenda>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var query = db.PreVendas
            .AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Itens)
            .Where(p => p.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            // Filtra por nome do cliente avulso, razão social do cadastrado, ou número do documento.
            query = query.Where(p =>
                (p.NomeClienteAvulso != null && EF.Functions.ILike(p.NomeClienteAvulso, $"%{termo}%")) ||
                (p.Cliente != null && EF.Functions.ILike(p.Cliente.RazaoSocial, $"%{termo}%")) ||
                p.CodPreVenda.ToString() == termo);
        }

        return await query.OrderByDescending(p => p.CodPreVenda).ToListAsync(ct);
    }

    public async Task AdicionarAsync(PreVenda preVenda, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        await db.PreVendas.AddAsync(preVenda, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Guid id, Action<PreVenda> alterar, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Carrega rastreado (sem AsNoTracking) para o change tracker detectar as alterações do cabeçalho.
        var preVenda = await db.PreVendas
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException($"Pré-venda {id} não encontrada para atualização.");

        // Snapshot dos itens que existiam ANTES da alteração (os rastreados, vindos do banco).
        var itensAntigos = preVenda.Itens.ToList();

        alterar(preVenda); // AlterarCabecalho + DefinirItens (limpa a coleção e adiciona NOVAS instâncias)

        // O padrão "substitui tudo" recria os itens com Id gerado no cliente (Guid.NewGuid no
        // construtor). O change tracker, ao ver Id preenchido, infere ESTADO ERRADO (Modified → UPDATE
        // numa linha inexistente → "0 rows affected"). Forçar: remover os antigos, inserir os novos
        // como Added. (Mesma lição do ProdutoRepository — coleção filha com PK no cliente.)
        db.RemoveRange(itensAntigos);
        foreach (var novo in preVenda.Itens)
            db.Entry(novo).State = EntityState.Added;

        await db.SaveChangesAsync(ct);
    }
}
