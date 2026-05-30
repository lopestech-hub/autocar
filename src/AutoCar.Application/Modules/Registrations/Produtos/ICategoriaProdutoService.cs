using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>Casos de uso do cadastro de Categoria de produto (CRUD).</summary>
public interface ICategoriaProdutoService
{
    Task<Result<CategoriaProdutoDto>> CriarAsync(SalvarCategoriaProdutoDto dto, CancellationToken ct = default);

    Task<Result<CategoriaProdutoDto>> AtualizarAsync(Guid id, SalvarCategoriaProdutoDto dto, CancellationToken ct = default);

    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    Task<Result> ReativarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CategoriaProdutoDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<CategoriaProdutoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
