using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>Casos de uso do cadastro de Produto (CRUD + opções para os combos).</summary>
public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoListaDto>> ListarAsync(string? filtro, CancellationToken ct = default);
    Task<Result<ProdutoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ProdutoDto>> CriarAsync(SalvarProdutoDto dto, CancellationToken ct = default);
    Task<Result<ProdutoDto>> AtualizarAsync(Guid id, SalvarProdutoDto dto, CancellationToken ct = default);
    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    /// <summary>Categorias ativas para o combo (obrigatório no produto).</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarCategoriasAsync(CancellationToken ct = default);

    /// <summary>Marcas ativas para o combo (opcional no produto).</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarMarcasAsync(CancellationToken ct = default);

    /// <summary>Fornecedores ativos para o combo (opcional no produto).</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarFornecedoresAsync(CancellationToken ct = default);
}
