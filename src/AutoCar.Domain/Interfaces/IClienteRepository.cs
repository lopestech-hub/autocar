using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="Cliente"/>.</summary>
public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<Cliente?> ObterPorDocumentoAsync(string documento, CancellationToken ct = default);

    /// <summary>Lista clientes ativos, opcionalmente filtrando por nome/razão social ou documento.</summary>
    Task<IReadOnlyList<Cliente>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task AdicionarAsync(Cliente cliente, CancellationToken ct = default);

    void Atualizar(Cliente cliente);

    /// <summary>Verifica se já existe outro cliente com o documento (exceto o id informado, em edição).</summary>
    Task<bool> ExisteDocumentoAsync(string documento, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
