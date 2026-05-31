using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="Produto"/>.</summary>
public interface IProdutoRepository
{
    /// <summary>Obtém o produto com as navegações (categoria, marca, fornecedor, aplicações) carregadas.</summary>
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista produtos ativos (com navegações), opcionalmente filtrando por descrição,
    /// código de barras ou código de fabricante.</summary>
    Task<IReadOnlyList<Produto>> ListarAsync(string? filtro, CancellationToken ct = default);

    /// <summary>Persiste um novo produto (com suas aplicações). Contexto próprio por operação.</summary>
    Task AdicionarAsync(Produto produto, CancellationToken ct = default);

    /// <summary>Carrega o produto rastreado, aplica a alteração e salva — tudo num único contexto
    /// (evita xmin defasado de contexto de longa duração). Lança se o produto não existir.</summary>
    Task AtualizarAsync(Guid id, Action<Produto> alterar, CancellationToken ct = default);

    /// <summary>Verifica se já existe outro produto com o mesmo código de barras
    /// (exceto o id informado, em edição). Usado só quando o código de barras é informado.</summary>
    Task<bool> ExisteCodBarrasAsync(string codBarras, Guid? excetoId = null, CancellationToken ct = default);
}
