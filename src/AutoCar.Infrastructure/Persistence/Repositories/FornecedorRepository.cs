using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de fornecedores.</summary>
public class FornecedorRepository : IFornecedorRepository
{
    private readonly AppDbContext _db;

    public FornecedorRepository(AppDbContext db) => _db = db;

    public Task<Fornecedor?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Fornecedores.FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<Fornecedor?> ObterPorDocumentoAsync(string documento, CancellationToken ct = default) =>
        _db.Fornecedores.FirstOrDefaultAsync(f => f.Documento == documento, ct);

    public async Task<IReadOnlyList<Fornecedor>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var query = _db.Fornecedores.AsNoTracking().Where(f => f.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            var digitos = new string(termo.Where(char.IsDigit).ToArray());

            query = query.Where(f =>
                EF.Functions.ILike(f.RazaoSocial, $"%{termo}%")
                || (digitos.Length > 0 && f.Documento.Contains(digitos)));
        }

        return await query.OrderBy(f => f.RazaoSocial).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Fornecedor fornecedor, CancellationToken ct = default) =>
        await _db.Fornecedores.AddAsync(fornecedor, ct);

    public void Atualizar(Fornecedor fornecedor) => _db.Fornecedores.Update(fornecedor);

    public Task<bool> ExisteDocumentoAsync(string documento, Guid? excetoId = null, CancellationToken ct = default) =>
        _db.Fornecedores.AnyAsync(f => f.Documento == documento && (excetoId == null || f.Id != excetoId), ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
