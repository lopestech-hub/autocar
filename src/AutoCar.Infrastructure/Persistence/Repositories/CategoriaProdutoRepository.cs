using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de categorias de produto.</summary>
public class CategoriaProdutoRepository : ICategoriaProdutoRepository
{
    private readonly AppDbContext _db;

    public CategoriaProdutoRepository(AppDbContext db) => _db = db;

    public Task<CategoriaProduto?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CategoriaProduto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.Categorias.AsNoTracking().Where(c => c.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(c => EF.Functions.ILike(c.Descricao, $"%{termo}%"));
        }

        return await query.OrderBy(c => c.Descricao).ToListAsync(ct);
    }

    public async Task AdicionarAsync(CategoriaProduto categoria, CancellationToken ct = default) =>
        await _db.Categorias.AddAsync(categoria, ct);

    public void Atualizar(CategoriaProduto categoria) => _db.Categorias.Update(categoria);

    // Unicidade case-insensitive.
    public Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.Categorias.AnyAsync(c => EF.Functions.ILike(c.Descricao, descricao) && (excetoId == null || c.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
