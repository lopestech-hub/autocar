using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de grupos de produto.</summary>
public class GrupoProdutoRepository : IGrupoProdutoRepository
{
    private readonly AppDbContext _db;

    public GrupoProdutoRepository(AppDbContext db) => _db = db;

    public Task<GrupoProduto?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.GruposProduto.Include(g => g.Categoria).FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<IReadOnlyList<GrupoProduto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.GruposProduto.AsNoTracking().Include(g => g.Categoria).Where(g => g.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(g => EF.Functions.ILike(g.Descricao, $"%{termo}%"));
        }

        return await query.OrderBy(g => g.Descricao).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GrupoProduto>> ListarPorCategoriaAsync(Guid idCategoria, CancellationToken ct = default) =>
        await _db.GruposProduto.AsNoTracking()
            .Where(g => g.FlgAtivo && g.IdCategoria == idCategoria)
            .OrderBy(g => g.Descricao)
            .ToListAsync(ct);

    public async Task AdicionarAsync(GrupoProduto grupo, CancellationToken ct = default) =>
        await _db.GruposProduto.AddAsync(grupo, ct);

    public void Atualizar(GrupoProduto grupo) => _db.GruposProduto.Update(grupo);

    // Unicidade case-insensitive DENTRO da categoria: "Amortecedor" colide com "AMORTECEDOR" na mesma categoria.
    public Task<bool> ExisteDescricaoAsync(string descricao, Guid idCategoria, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.GruposProduto.AnyAsync(g =>
            g.IdCategoria == idCategoria &&
            EF.Functions.ILike(g.Descricao, descricao) &&
            (excetoId == null || g.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
