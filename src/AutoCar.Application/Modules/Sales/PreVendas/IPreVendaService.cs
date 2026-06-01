using AutoCar.Application.Modules.Sales.PreVendas.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Sales.PreVendas;

/// <summary>Casos de uso da Pré-venda (documento provisório de balcão: cabeçalho + itens).</summary>
public interface IPreVendaService
{
    Task<IReadOnlyList<PreVendaListaDto>> ListarAsync(string? filtro, CancellationToken ct = default);
    Task<Result<PreVendaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Cria uma pré-venda Aberta. O <paramref name="idUsuario"/> é o vendedor logado.</summary>
    Task<Result<PreVendaDto>> CriarAsync(Guid idUsuario, SalvarPreVendaDto dto, CancellationToken ct = default);

    /// <summary>Atualiza uma pré-venda Aberta (cabeçalho + itens). Falha se já Faturada/Cancelada.</summary>
    Task<Result<PreVendaDto>> AtualizarAsync(Guid id, SalvarPreVendaDto dto, CancellationToken ct = default);

    /// <summary>Fatura a pré-venda (vira venda — torna o documento imutável). Falha se não Aberta ou sem itens.</summary>
    Task<Result> FaturarAsync(Guid id, CancellationToken ct = default);

    /// <summary>Cancela a pré-venda. Falha se não estiver Aberta.</summary>
    Task<Result> CancelarAsync(Guid id, CancellationToken ct = default);
}
