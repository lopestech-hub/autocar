using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>Implementação EF Core do repositório de usuários.</summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;

    public UsuarioRepository(AppDbContext db) => _db = db;

    public Task<Usuario?> ObterPorLoginAsync(string login, CancellationToken ct = default) =>
        _db.Usuarios.FirstOrDefaultAsync(u => u.Login == login, ct);

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default) =>
        await _db.Usuarios.AsNoTracking().OrderBy(u => u.Nome).ToListAsync(ct);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default) =>
        await _db.Usuarios.AddAsync(usuario, ct);

    public void Atualizar(Usuario usuario) => _db.Usuarios.Update(usuario);

    public Task<bool> ExisteLoginAsync(string login, CancellationToken ct = default) =>
        _db.Usuarios.AnyAsync(u => u.Login == login, ct);

    public Task<int> SalvarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
