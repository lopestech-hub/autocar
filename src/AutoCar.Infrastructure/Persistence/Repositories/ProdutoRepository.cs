using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de produtos.</summary>
public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _db;

    public ProdutoRepository(AppDbContext db) => _db = db;

    public Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Produtos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Produto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Where(p => p.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Descricao, $"%{termo}%") ||
                (p.CodBarras != null && EF.Functions.ILike(p.CodBarras, $"%{termo}%")) ||
                (p.CodFabricante != null && EF.Functions.ILike(p.CodFabricante, $"%{termo}%")));
        }

        return await query.OrderBy(p => p.Descricao).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Produto produto, CancellationToken ct = default) =>
        await _db.Produtos.AddAsync(produto, ct);

    public void Atualizar(Produto produto) => _db.Produtos.Update(produto);

    public Task<bool> ExisteCodBarrasAsync(string codBarras, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.Produtos.AnyAsync(
            p => p.CodBarras == codBarras && (excetoId == null || p.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
