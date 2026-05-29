using AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Fornecedores;

/// <summary>Casos de uso do cadastro de Fornecedor (CRUD).</summary>
public interface IFornecedorService
{
    Task<Result<FornecedorDto>> CriarAsync(SalvarFornecedorDto dto, CancellationToken ct = default);

    Task<Result<FornecedorDto>> AtualizarAsync(Guid id, SalvarFornecedorDto dto, CancellationToken ct = default);

    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    Task<Result> ReativarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<FornecedorListaDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<FornecedorDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
