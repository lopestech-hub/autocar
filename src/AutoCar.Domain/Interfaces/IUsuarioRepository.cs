using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>
/// Contrato de persistência de <see cref="Usuario"/>. A implementação EF Core
/// mora na Infrastructure (Dependency Inversion).
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorLoginAsync(string login, CancellationToken ct = default);

    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default);

    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);

    void Atualizar(Usuario usuario);

    Task<bool> ExisteLoginAsync(string login, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
