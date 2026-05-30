using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de marcas.</summary>
public class MarcaRepository : IMarcaRepository
{
    private readonly AppDbContext _db;

    public MarcaRepository(AppDbContext db) => _db = db;

    public Task<Marca?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Marcas.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Marca>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.Marcas.AsNoTracking().Where(m => m.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(m => EF.Functions.ILike(m.Descricao, $"%{termo}%"));
        }

        return await query.OrderBy(m => m.Descricao).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Marca marca, CancellationToken ct = default) =>
        await _db.Marcas.AddAsync(marca, ct);

    public void Atualizar(Marca marca) => _db.Marcas.Update(marca);

    // Unicidade case-insensitive: "Bosch" colide com "BOSCH".
    public Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.Marcas.AnyAsync(m => EF.Functions.ILike(m.Descricao, descricao) && (excetoId == null || m.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
