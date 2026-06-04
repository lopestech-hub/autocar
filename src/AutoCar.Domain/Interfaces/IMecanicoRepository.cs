using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="Mecanico"/>.</summary>
public interface IMecanicoRepository
{
    Task<Mecanico?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista mecânicos ativos, opcionalmente filtrando por nome.</summary>
    Task<IReadOnlyList<Mecanico>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task AdicionarAsync(Mecanico mecanico, CancellationToken ct = default);

    void Atualizar(Mecanico mecanico);

    /// <summary>Verifica se já existe outro mecânico com o mesmo nome (exceto o id informado, em edição).</summary>
    Task<bool> ExisteNomeAsync(string nome, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
