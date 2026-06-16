using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>Casos de uso do cadastro de Grupo de produto (CRUD). Grupo pertence a uma categoria.</summary>
public interface IGrupoProdutoService
{
    Task<Result<GrupoProdutoDto>> CriarAsync(SalvarGrupoProdutoDto dto, CancellationToken ct = default);

    Task<Result<GrupoProdutoDto>> AtualizarAsync(Guid id, SalvarGrupoProdutoDto dto, CancellationToken ct = default);

    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    Task<Result> ReativarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<GrupoProdutoDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<GrupoProdutoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Categorias ativas para o combo do formulário de grupo.</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarCategoriasAsync(CancellationToken ct = default);
}
