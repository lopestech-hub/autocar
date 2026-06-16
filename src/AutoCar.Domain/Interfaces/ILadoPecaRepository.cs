using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="LadoPeca"/>.</summary>
public interface ILadoPecaRepository
{
    Task<LadoPeca?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista lados ativos, opcionalmente filtrando por descrição.</summary>
    Task<IReadOnlyList<LadoPeca>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task AdicionarAsync(LadoPeca lado, CancellationToken ct = default);

    void Atualizar(LadoPeca lado);

    /// <summary>Verifica se já existe outro lado com a mesma descrição (exceto o id informado, em edição).</summary>
    Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
