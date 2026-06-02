using AutoCar.Application.Modules.Sales.Devolucoes.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Sales.Devolucoes;

/// <summary>Casos de uso da Devolução de venda: listar o que é devolvível de uma venda faturada e
/// registrar a devolução (repondo o estoque).</summary>
public interface IDevolucaoService
{
    /// <summary>Lista os itens de uma pré-venda faturada com o saldo devolvível de cada um (vendido −
    /// já devolvido). Falha se a venda não existir ou não estiver Faturada.</summary>
    Task<Result<IReadOnlyList<ItemDevolvivelDto>>> ListarItensDevolviveisAsync(Guid idPreVenda, CancellationToken ct = default);

    /// <summary>Registra a devolução e repõe o estoque dos itens (transação única). Valida que cada
    /// item devolvido não excede o saldo devolvível. O <paramref name="idUsuario"/> é quem devolve.</summary>
    Task<Result<DevolucaoDto>> CriarAsync(Guid idUsuario, CriarDevolucaoDto dto, CancellationToken ct = default);
}
