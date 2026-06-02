using AutoCar.Application.Modules.Purchases.Compras.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Purchases.Compras;

/// <summary>Casos de uso da Compra: registrar uma compra (dando entrada no estoque) e listar as compras.</summary>
public interface ICompraService
{
    /// <summary>Registra a compra e dá entrada no estoque dos itens numa transação única. Valida o
    /// fornecedor e os produtos. O <paramref name="idUsuario"/> é quem registrou a compra.</summary>
    Task<Result<CompraDto>> CriarAsync(Guid idUsuario, CriarCompraDto dto, CancellationToken ct = default);

    /// <summary>Lista as compras registradas (cabeçalho + contagem de itens), mais recentes primeiro.</summary>
    Task<Result<IReadOnlyList<CompraListaDto>>> ListarAsync(CancellationToken ct = default);

    /// <summary>Obtém uma compra registrada para reabrir em visualização (cabeçalho + fornecedor + itens).</summary>
    Task<Result<CompraDetalheDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
}
