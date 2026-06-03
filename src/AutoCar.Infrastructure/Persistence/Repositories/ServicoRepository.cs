using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de serviços.</summary>
public class ServicoRepository : IServicoRepository
{
    private readonly AppDbContext _db;

    public ServicoRepository(AppDbContext db) => _db = db;

    public Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Servicos.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Servico>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.Servicos.AsNoTracking().Where(s => s.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(s => EF.Functions.ILike(s.Descricao, $"%{termo}%"));
        }

        return await query.OrderBy(s => s.Descricao).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Servico servico, CancellationToken ct = default) =>
        await _db.Servicos.AddAsync(servico, ct);

    public void Atualizar(Servico servico) => _db.Servicos.Update(servico);

    // Unicidade case-insensitive: "Alinhamento" colide com "ALINHAMENTO".
    public Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.Servicos.AnyAsync(s => EF.Functions.ILike(s.Descricao, descricao) && (excetoId == null || s.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
