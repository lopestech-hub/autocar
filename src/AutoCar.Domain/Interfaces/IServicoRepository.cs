using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="Servico"/>.</summary>
public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista serviços ativos, opcionalmente filtrando por descrição.</summary>
    Task<IReadOnlyList<Servico>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task AdicionarAsync(Servico servico, CancellationToken ct = default);

    void Atualizar(Servico servico);

    /// <summary>Verifica se já existe outro serviço com a mesma descrição (exceto o id informado, em edição).</summary>
    Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
