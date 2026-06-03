using AutoCar.Application.Modules.Registrations.Servicos.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Servicos;

/// <summary>Casos de uso do cadastro de Serviço (CRUD + listagem para o seletor da OS).</summary>
public interface IServicoService
{
    Task<Result<ServicoDto>> CriarAsync(SalvarServicoDto dto, CancellationToken ct = default);

    Task<Result<ServicoDto>> AtualizarAsync(Guid id, SalvarServicoDto dto, CancellationToken ct = default);

    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    Task<Result> ReativarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ServicoDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<ServicoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
