namespace AutoCar.Domain.Common;

/// <summary>
/// Converte quantidades de documentos (decimal, ex: itens de pré-venda) para a quantidade inteira que
/// o estoque usa. No setor automotivo a peça é unidade fechada — fração não deveria existir; se existir,
/// falha explicitamente em vez de truncar silenciosamente (evita divergência entre o documento e o
/// estoque/devolução). Fonte única usada pelo faturamento e pela devolução.
/// </summary>
public static class QuantidadeEstoque
{
    /// <summary>Converte a quantidade do documento para inteiro; lança se houver fração.</summary>
    /// <param name="qtd">Quantidade (decimal) vinda de um item de documento.</param>
    /// <param name="descricaoItem">Descrição do item, para a mensagem de erro.</param>
    public static int DeDocumento(decimal qtd, string descricaoItem)
    {
        if (qtd != Math.Truncate(qtd))
            throw new InvalidOperationException(
                $"O item \"{descricaoItem}\" tem quantidade fracionada ({qtd}); o estoque trabalha com unidades inteiras.");
        return (int)qtd;
    }
}
