using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de clientes.</summary>
public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _db;

    public ClienteRepository(AppDbContext db) => _db = db;

    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Cliente?> ObterPorDocumentoAsync(string documento, CancellationToken ct = default) =>
        _db.Clientes.FirstOrDefaultAsync(c => c.Documento == documento, ct);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.Clientes.AsNoTracking().Where(c => c.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            var digitos = new string(termo.Where(char.IsDigit).ToArray());

            query = query.Where(c =>
                EF.Functions.ILike(c.RazaoSocial, $"%{termo}%")
                || (digitos.Length > 0 && c.Documento.Contains(digitos)));
        }

        return await query.OrderBy(c => c.RazaoSocial).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Cliente cliente, CancellationToken ct = default) =>
        await _db.Clientes.AddAsync(cliente, ct);

    public void Atualizar(Cliente cliente) => _db.Clientes.Update(cliente);

    public Task<bool> ExisteDocumentoAsync(string documento, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.Clientes.AnyAsync(c => c.Documento == documento && (excetoId == null || c.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
