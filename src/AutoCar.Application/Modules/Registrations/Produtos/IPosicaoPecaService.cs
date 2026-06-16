using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>Casos de uso do cadastro de Posição da peça (CRUD).</summary>
public interface IPosicaoPecaService
{
    Task<Result<PosicaoPecaDto>> CriarAsync(SalvarPosicaoPecaDto dto, CancellationToken ct = default);

    Task<Result<PosicaoPecaDto>> AtualizarAsync(Guid id, SalvarPosicaoPecaDto dto, CancellationToken ct = default);

    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    Task<Result> ReativarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<PosicaoPecaDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<PosicaoPecaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
