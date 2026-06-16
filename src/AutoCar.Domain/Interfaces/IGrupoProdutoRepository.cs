using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="GrupoProduto"/>.</summary>
public interface IGrupoProdutoRepository
{
    Task<GrupoProduto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista grupos ativos (com a categoria carregada), opcionalmente filtrando por descrição.</summary>
    Task<IReadOnlyList<GrupoProduto>> ListarAsync(string? filtro, CancellationToken ct = default);

    /// <summary>Lista grupos ativos de UMA categoria (para o combo dependente do Produto).</summary>
    Task<IReadOnlyList<GrupoProduto>> ListarPorCategoriaAsync(Guid idCategoria, CancellationToken ct = default);

    Task AdicionarAsync(GrupoProduto grupo, CancellationToken ct = default);

    void Atualizar(GrupoProduto grupo);

    /// <summary>Verifica se já existe outro grupo com a mesma descrição NA MESMA categoria (exceto o id informado).</summary>
    Task<bool> ExisteDescricaoAsync(string descricao, Guid idCategoria, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
