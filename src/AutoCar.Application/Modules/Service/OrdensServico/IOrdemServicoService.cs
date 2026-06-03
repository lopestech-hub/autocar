using AutoCar.Application.Modules.Service.OrdensServico.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Service.OrdensServico;

/// <summary>Casos de uso da Ordem de Serviço: CRUD + transições do ciclo de vida. O faturamento
/// (que baixa estoque) entra na sub-fase 4.4 via repositório transacional dedicado.</summary>
public interface IOrdemServicoService
{
    Task<Result<OrdemServicoDto>> CriarAsync(Guid idUsuario, SalvarOrdemServicoDto dto, CancellationToken ct = default);

    Task<Result<OrdemServicoDto>> AtualizarAsync(Guid id, SalvarOrdemServicoDto dto, CancellationToken ct = default);

    /// <summary>Marca a OS como Em andamento (mecânico começou). Só a partir de Aberta.</summary>
    Task<Result> IniciarAsync(Guid id, CancellationToken ct = default);

    /// <summary>Conclui o serviço (exige mecânico responsável e ≥1 item). De Aberta ou Em andamento.</summary>
    Task<Result> ConcluirAsync(Guid id, CancellationToken ct = default);

    /// <summary>Cancela a OS (só antes de faturar; não mexe em estoque).</summary>
    Task<Result> CancelarAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<OrdemServicoListaDto>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task<Result<OrdemServicoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
