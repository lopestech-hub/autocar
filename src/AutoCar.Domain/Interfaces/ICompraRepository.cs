using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>
/// Persistência de compras. O registro de uma compra é transacional: salvar o documento e dar ENTRADA
/// no estoque de cada item comprado acontecem no mesmo DbContext, com um único commit (atômico — ou
/// registra a compra e repõe o estoque, ou nada). Espelha o padrão da devolução (entrada por documento).
/// </summary>
public interface ICompraRepository
{
    /// <summary>
    /// Registra a compra e dá entrada no estoque de todos os seus itens numa única transação. Para cada
    /// item, gera uma entrada no estoque com origem Compra (apontando para o nº da compra).
    /// </summary>
    /// <param name="compra">Documento de compra com os itens já definidos.</param>
    Task RegistrarComEntradaEstoqueAsync(Compra compra, CancellationToken ct = default);

    /// <summary>Lista as compras (cabeçalho + contagem de itens) para a tela de listagem, mais recentes primeiro.</summary>
    Task<IReadOnlyList<Compra>> ListarAsync(CancellationToken ct = default);
}
