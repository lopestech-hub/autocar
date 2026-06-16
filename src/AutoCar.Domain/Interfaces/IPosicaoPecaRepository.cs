using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="PosicaoPeca"/>.</summary>
public interface IPosicaoPecaRepository
{
    Task<PosicaoPeca?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista posições ativas, opcionalmente filtrando por descrição.</summary>
    Task<IReadOnlyList<PosicaoPeca>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task AdicionarAsync(PosicaoPeca posicao, CancellationToken ct = default);

    void Atualizar(PosicaoPeca posicao);

    /// <summary>Verifica se já existe outra posição com a mesma descrição (exceto o id informado, em edição).</summary>
    Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
