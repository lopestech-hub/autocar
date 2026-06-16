using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>Casos de uso do cadastro de Lado da peça (CRUD).</summary>
public interface ILadoPecaService
{
    Task<Result<LadoPecaDto>> CriarAsync(SalvarLadoPecaDto dto, CancellationToken ct = default);

    Task<Result<LadoPecaDto>> AtualizarAsync(Guid id, SalvarLadoPecaDto dto, CancellationToken ct = default);

    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    Task<Result> ReativarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<LadoPecaDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<LadoPecaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
