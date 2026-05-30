using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="Marca"/>.</summary>
public interface IMarcaRepository
{
    Task<Marca?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista marcas ativas, opcionalmente filtrando por descrição.</summary>
    Task<IReadOnlyList<Marca>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task AdicionarAsync(Marca marca, CancellationToken ct = default);

    void Atualizar(Marca marca);

    /// <summary>Verifica se já existe outra marca com a mesma descrição (exceto o id informado, em edição).</summary>
    Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
