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

    /// <summary>Busca de peças por veículo (Catálogo): produtos ativos que casam com o termo na
    /// descrição E têm aplicação no veículo informado. Filtros nulos são ignorados. As navegações
    /// (categoria, marca, aplicações) vêm carregadas para montar o resultado.</summary>
    Task<IReadOnlyList<Produto>> BuscarPorVeiculoAsync(
        string? termo, string? montadora, string? modelo, int? ano, CancellationToken ct = default);

    /// <summary>Montadoras distintas cadastradas em aplicações de produtos ativos.</summary>
    Task<IReadOnlyList<string>> ListarMontadorasAsync(CancellationToken ct = default);

    /// <summary>Modelos distintos cadastrados; filtra por montadora se informada.</summary>
    Task<IReadOnlyList<string>> ListarModelosAsync(string? montadora, CancellationToken ct = default);
}
