using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="Produto"/>.</summary>
public interface IProdutoRepository
{
    /// <summary>Obtém o produto com as navegações (categoria, marca, fornecedor) carregadas.</summary>
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista produtos ativos (com navegações), opcionalmente filtrando por descrição,
    /// código de barras ou código de fabricante.</summary>
    Task<IReadOnlyList<Produto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task AdicionarAsync(Produto produto, CancellationToken ct = default);

    void Atualizar(Produto produto);

    /// <summary>Verifica se já existe outro produto com o mesmo código de barras
    /// (exceto o id informado, em edição). Usado só quando o código de barras é informado.</summary>
    Task<bool> ExisteCodBarrasAsync(string codBarras, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
