using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de posições da peça.</summary>
public class PosicaoPecaRepository : IPosicaoPecaRepository
{
    private readonly AppDbContext _db;

    public PosicaoPecaRepository(AppDbContext db) => _db = db;

    public Task<PosicaoPeca?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.PosicoesPeca.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<PosicaoPeca>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.PosicoesPeca.AsNoTracking().Where(p => p.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(p => EF.Functions.ILike(p.Descricao, $"%{termo}%"));
        }

        return await query.OrderBy(p => p.Descricao).ToListAsync(ct);
    }

    public async Task AdicionarAsync(PosicaoPeca posicao, CancellationToken ct = default) =>
        await _db.PosicoesPeca.AddAsync(posicao, ct);

    public void Atualizar(PosicaoPeca posicao) => _db.PosicoesPeca.Update(posicao);

    // Unicidade case-insensitive: "Dianteira" colide com "DIANTEIRA".
    public Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.PosicoesPeca.AnyAsync(p => EF.Functions.ILike(p.Descricao, descricao) && (excetoId == null || p.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
