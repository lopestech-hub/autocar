using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.Interfaces;

/// <summary>Saldo de um produto já com os dados de exibição (descrição/código/unidade) resolvidos
/// na mesma consulta — projeção de leitura que evita N+1 ao listar os saldos.</summary>
public sealed record SaldoProdutoLeitura(
    Guid IdProduto,
    int CodProduto,
    string Descricao,
    UnidadeMedida Unidade,
    int QtdSaldo,
    int QtdReservada);

/// <summary>
/// Contrato de persistência do estoque (saldo + livro-razão de movimentos). O saldo usa
/// concorrência otimista (xmin): operações de movimentação carregam o saldo rastreado, aplicam a
/// mudança e salvam saldo + movimento na MESMA transação. Se outro terminal alterou o saldo nesse
/// meio-tempo, o SaveChanges lança <c>DbUpdateConcurrencyException</c> (tratada na Application).
/// </summary>
public interface IEstoqueRepository
{
    /// <summary>Lista os saldos dos produtos ativos (com descrição/código/unidade resolvidos numa
    /// única consulta; saldo zero quando o produto ainda não tem movimento), opcionalmente filtrando
    /// por descrição, código de barras, código de fabricante ou número do produto.</summary>
    Task<IReadOnlyList<SaldoProdutoLeitura>> ListarSaldosAsync(string? filtro, CancellationToken ct = default);

    /// <summary>Lista o histórico de movimentos de um produto (mais recentes primeiro).</summary>
    Task<IReadOnlyList<MovimentoEstoque>> ListarMovimentosAsync(Guid idProduto, CancellationToken ct = default);

    /// <summary>
    /// Aplica uma movimentação ao saldo do produto numa única transação: carrega o saldo rastreado
    /// (cria zerado se ainda não existir), executa <paramref name="movimentar"/> sobre ele, persiste o
    /// saldo e o movimento gerado juntos. O xmin do saldo detecta edição concorrente.
    /// </summary>
    /// <param name="idProduto">Produto a movimentar.</param>
    /// <param name="movimentar">Função que aplica o movimento e devolve o registro de livro-razão.</param>
    Task MovimentarAsync(
        Guid idProduto,
        Func<EstoqueProduto, MovimentoEstoque> movimentar,
        CancellationToken ct = default);
}
