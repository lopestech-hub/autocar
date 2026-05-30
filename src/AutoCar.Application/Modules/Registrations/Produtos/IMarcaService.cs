using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>Casos de uso do cadastro de Marca (CRUD).</summary>
public interface IMarcaService
{
    Task<Result<MarcaDto>> CriarAsync(SalvarMarcaDto dto, CancellationToken ct = default);

    Task<Result<MarcaDto>> AtualizarAsync(Guid id, SalvarMarcaDto dto, CancellationToken ct = default);

    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    Task<Result> ReativarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<MarcaDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<MarcaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
