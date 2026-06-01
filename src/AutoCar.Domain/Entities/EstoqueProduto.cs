using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Saldo de estoque de um produto (1:1 com <see cref="Produto"/>). É a raiz do agregado de estoque:
/// o saldo só muda por <see cref="Movimentar"/>, que valida as regras (quantidade positiva, saldo
/// nunca negativo) e produz o <see cref="MovimentoEstoque"/> correspondente no livro-razão.
///
/// Usa concorrência otimista via <c>xmin</c> (coluna de sistema do PostgreSQL): dois terminais que
/// disputam o mesmo último item — um vence, o outro recebe conflito e refaz a leitura. Diferente de
/// Produto/PreVenda, este agregado NÃO tem coleção filha editada no mesmo SaveChanges, então o xmin
/// funciona limpo aqui (o movimento gerado é um INSERT independente, não um UPDATE do mesmo registro).
///
/// Quantidades são <b>inteiras</b> — no setor automotivo a peça é sempre unidade fechada
/// (UN, PC, PAR, JG, KIT, CX); óleo/mangueira vendem em embalagem fechada, também inteiro.
/// </summary>
public class EstoqueProduto : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected EstoqueProduto() { }

    /// <summary>Cria o registro de saldo de um produto, começando zerado.</summary>
    public EstoqueProduto(Guid idProduto)
    {
        IdProduto = idProduto;
        QtdSaldo = 0;
        QtdReservada = 0;
    }

    /// <summary>FK para o produto (única — um registro de saldo por produto).</summary>
    public Guid IdProduto { get; protected set; }

    /// <summary>Saldo atual em estoque. Nunca negativo (garantido por <see cref="Movimentar"/>).</summary>
    public int QtdSaldo { get; protected set; }

    /// <summary>Quantidade reservada (preparado para reserva de venda futura; não usado no MVP).</summary>
    public int QtdReservada { get; protected set; }

    /// <summary>Saldo livre para venda (saldo − reservado). No MVP, igual ao saldo (reservado = 0).</summary>
    public int QtdDisponivel => QtdSaldo - QtdReservada;

    /// <summary>
    /// Aplica um movimento ao saldo e devolve o registro de livro-razão correspondente. Valida:
    /// quantidade estritamente positiva e saldo resultante não-negativo (não vende o que não tem).
    /// Saldo e movimento devem ser persistidos na MESMA transação (o repositório garante).
    /// </summary>
    /// <param name="tipo">Direção do movimento (entrada eleva; saída/ajuste negativo abaixam).</param>
    /// <param name="qtd">Quantidade positiva a movimentar.</param>
    /// <param name="idUsuario">Usuário que registra o movimento.</param>
    /// <param name="observacao">Observação opcional (motivo, nº da nota).</param>
    public MovimentoEstoque Movimentar(
        TipoMovimentoEstoque tipo,
        int qtd,
        Guid idUsuario,
        string? observacao = null)
    {
        if (qtd <= 0)
            throw new ArgumentException("A quantidade do movimento deve ser maior que zero.", nameof(qtd));

        var delta = tipo switch
        {
            TipoMovimentoEstoque.Entrada or TipoMovimentoEstoque.AjustePositivo => qtd,
            TipoMovimentoEstoque.Saida or TipoMovimentoEstoque.AjusteNegativo => -qtd,
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de movimento inválido."),
        };

        var saldoResultante = QtdSaldo + delta;
        if (saldoResultante < 0)
            throw new InvalidOperationException(
                $"Saldo insuficiente: há {QtdSaldo} em estoque e a operação retiraria {qtd}.");

        QtdSaldo = saldoResultante;
        MarcarAtualizada();

        return new MovimentoEstoque(IdProduto, tipo, qtd, QtdSaldo, idUsuario, observacao);
    }
}
