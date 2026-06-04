using AutoCar.Application.Modules.Registrations.Mecanicos.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Mecanicos;

/// <summary>Casos de uso do cadastro de Mecânico (CRUD + listagem para o seletor da OS).</summary>
public interface IMecanicoService
{
    Task<Result<MecanicoDto>> CriarAsync(SalvarMecanicoDto dto, CancellationToken ct = default);

    Task<Result<MecanicoDto>> AtualizarAsync(Guid id, SalvarMecanicoDto dto, CancellationToken ct = default);

    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    Task<Result> ReativarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<MecanicoDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<MecanicoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
