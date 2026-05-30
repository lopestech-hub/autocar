using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="CategoriaProduto"/>.</summary>
public interface ICategoriaProdutoRepository
{
    Task<CategoriaProduto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista categorias ativas, opcionalmente filtrando por descrição.</summary>
    Task<IReadOnlyList<CategoriaProduto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task AdicionarAsync(CategoriaProduto categoria, CancellationToken ct = default);

    void Atualizar(CategoriaProduto categoria);

    /// <summary>Verifica se já existe outra categoria com a mesma descrição (exceto o id informado, em edição).</summary>
    Task<bool> ExisteDescricaoAsync(string descricao, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
