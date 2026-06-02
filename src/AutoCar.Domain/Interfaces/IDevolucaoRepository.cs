using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>
/// Persistência de devoluções. O registro de uma devolução é transacional: salvar o documento e dar
/// ENTRADA no estoque de cada item devolvido acontecem no mesmo DbContext, com um único commit
/// (atômico — ou registra a devolução e repõe o estoque, ou nada). Espelha o faturamento, ao contrário.
/// </summary>
public interface IDevolucaoRepository
{
    /// <summary>
    /// Registra a devolução e repõe o estoque de todos os seus itens numa única transação. Para cada
    /// item, gera uma entrada no estoque com origem Devolução (apontando para o nº da devolução).
    /// </summary>
    /// <param name="devolucao">Documento de devolução com os itens já definidos.</param>
    Task RegistrarComEntradaEstoqueAsync(Devolucao devolucao, CancellationToken ct = default);

    /// <summary>
    /// Soma, por produto, a quantidade já devolvida de uma pré-venda (todas as devoluções anteriores).
    /// Usado para validar o saldo devolvível (vendido − já devolvido) antes de registrar uma nova.
    /// Devolve um mapa idProduto → quantidade total já devolvida.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> TotalDevolvidoPorProdutoAsync(Guid idPreVenda, CancellationToken ct = default);
}
