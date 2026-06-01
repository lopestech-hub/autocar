namespace AutoCar.Domain.Enums;

/// <summary>
/// Tipo de movimento no livro-razão de estoque. A quantidade do movimento é sempre positiva;
/// o tipo é que define a direção (entrada eleva o saldo, saída/ajuste negativo abaixam).
/// Persistido como int.
/// </summary>
public enum TipoMovimentoEstoque
{
    /// <summary>Entrada de mercadoria (compra, devolução de cliente). Eleva o saldo.</summary>
    Entrada = 1,

    /// <summary>Saída de mercadoria (venda faturada, baixa). Abaixa o saldo.</summary>
    Saida = 2,

    /// <summary>Ajuste de inventário que eleva o saldo (sobra encontrada na contagem).</summary>
    AjustePositivo = 3,

    /// <summary>Ajuste de inventário que abaixa o saldo (perda, quebra, falta na contagem).</summary>
    AjusteNegativo = 4,
}
