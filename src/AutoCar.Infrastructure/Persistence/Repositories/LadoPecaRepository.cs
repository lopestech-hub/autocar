using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de lados da peça.</summary>
public class LadoPecaRepository : ILadoPecaRepository
{
    private readonly AppDbContext _db;

    public LadoPecaRepository(AppDbContext db) => _db = db;

    public Task<LadoPeca?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.LadosPeca.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<LadoPeca>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.LadosPeca.AsNoTracking().Where(l => l.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(l => EF.Functions.ILike(l.Descricao, $"%{termo}%"));
        }

        return await query.OrderBy(l => l.Descricao).ToListAsync(ct);
    }

    public async Task AdicionarAsync(LadoPeca lado, CancellationToken ct = default) =>
        await _db.LadosPeca.AddAsync(lado, ct);

    public void Atualizar(LadoPeca lado) => _db.LadosPeca.Update(lado);

    // Unicidade case-insensitive: "Esquerdo" colide com "ESQUERDO".
    public Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.LadosPeca.AnyAsync(l => EF.Functions.ILike(l.Descricao, descricao) && (excetoId == null || l.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
