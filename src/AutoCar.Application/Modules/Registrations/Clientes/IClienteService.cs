using AutoCar.Application.Modules.Registrations.Clientes.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Clientes;

/// <summary>Casos de uso do cadastro de Cliente (CRUD).</summary>
public interface IClienteService
{
    Task<Result<ClienteDto>> CriarAsync(SalvarClienteDto dto, CancellationToken ct = default);

    Task<Result<ClienteDto>> AtualizarAsync(Guid id, SalvarClienteDto dto, CancellationToken ct = default);

    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    Task<Result> ReativarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ClienteListaDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<ClienteDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
