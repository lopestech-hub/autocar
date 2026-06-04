using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de mecânicos.</summary>
public class MecanicoRepository : IMecanicoRepository
{
    private readonly AppDbContext _db;

    public MecanicoRepository(AppDbContext db) => _db = db;

    public Task<Mecanico?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Mecanicos.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<Mecanico>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.Mecanicos.AsNoTracking().Where(m => m.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(m => EF.Functions.ILike(m.Nome, $"%{termo}%"));
        }

        return await query.OrderBy(m => m.Nome).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Mecanico mecanico, CancellationToken ct = default) =>
        await _db.Mecanicos.AddAsync(mecanico, ct);

    public void Atualizar(Mecanico mecanico) => _db.Mecanicos.Update(mecanico);

    // Unicidade case-insensitive do nome.
    public Task<bool> ExisteNomeAsync(string nome, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.Mecanicos.AnyAsync(m => EF.Functions.ILike(m.Nome, nome) && (excetoId == null || m.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
