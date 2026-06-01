using AutoCar.Application.Modules.Estoque.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Estoque;

/// <summary>Casos de uso do Estoque: consulta de saldos, histórico de movimentos e registro de
/// entrada/saída/ajuste com concorrência otimista (saldo nunca negativo; conflito entre terminais).</summary>
public interface IEstoqueService
{
    /// <summary>Lista os saldos dos produtos ativos (saldo zero quando o produto ainda não tem movimento),
    /// opcionalmente filtrando por descrição, código de barras, código de fabricante ou número do produto.</summary>
    Task<IReadOnlyList<SaldoEstoqueListaDto>> ListarSaldosAsync(string? filtro, CancellationToken ct = default);

    /// <summary>Lista o histórico de movimentos de um produto (mais recentes primeiro).</summary>
    Task<IReadOnlyList<MovimentoEstoqueDto>> ListarMovimentosAsync(Guid idProduto, CancellationToken ct = default);

    /// <summary>
    /// Registra um movimento (entrada/saída/ajuste) no estoque do produto. O <paramref name="idUsuario"/>
    /// é quem realiza a operação. Falha com erro de validação se a quantidade não respeitar as regras
    /// (saldo insuficiente numa saída) e com erro de conflito se outro terminal alterar o saldo ao mesmo tempo.
    /// </summary>
    Task<Result> MovimentarAsync(Guid idUsuario, MovimentarEstoqueDto dto, CancellationToken ct = default);
}
