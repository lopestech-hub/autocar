using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Compra de mercadoria de um fornecedor. É a raiz do agregado e um documento de entrada: cada item
/// comprado gera uma ENTRADA no estoque (origem Compra), na mesma transação em que o documento é salvo
/// (garantido pelo repositório). É a contrapartida da venda — fecha o ciclo do estoque (a saída sai
/// pelo faturamento da pré-venda; a entrada entra pela compra).
///
/// Entrada imediata: a compra representa mercadoria que já chegou, então não tem ciclo de vida
/// (Aberta/Recebida) — ao registrar, já dá entrada no estoque. O agregado garante as invariantes
/// locais (≥1 item, quantidades positivas, total coerente). Não atualiza o custo do produto no MVP.
/// </summary>
public class Compra : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected Compra() { }

    public Compra(Guid idFornecedor, Guid idUsuario, string? numDocumento, string? observacao)
    {
        IdFornecedor = idFornecedor;
        IdUsuario = idUsuario;
        NumDocumento = string.IsNullOrWhiteSpace(numDocumento) ? null : numDocumento.Trim();
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
    }

    /// <summary>Código legível autoincrement (número do documento de compra), gerado pelo banco.</summary>
    public int CodCompra { get; protected set; }

    /// <summary>FK para o fornecedor da compra (Restrict — obrigatório; sem fornecedor a compra não existe).</summary>
    public Guid IdFornecedor { get; protected set; }

    /// <summary>Usuário que registrou a compra (rastreabilidade).</summary>
    public Guid IdUsuario { get; protected set; }

    /// <summary>Número da nota/documento do fornecedor (texto livre, opcional no MVP).</summary>
    public string? NumDocumento { get; protected set; }

    /// <summary>Observação livre do documento (opcional).</summary>
    public string? Observacao { get; protected set; }

    /// <summary>Valor total da compra (soma dos itens). Calculado no domínio e persistido.</summary>
    public decimal VlrTotal { get; protected set; }

    /// <summary>Navegação para o fornecedor (somente leitura — carregada na listagem para exibir o nome).</summary>
    public Fornecedor? Fornecedor { get; protected set; }

    // Itens (1:N). Backing field exposto como somente leitura — definir via DefinirItens.
    private readonly List<CompraItem> _itens = new();

    public IReadOnlyList<CompraItem> Itens => _itens;

    /// <summary>Define os itens comprados (≥1) e calcula o total. Exige ao menos um item.</summary>
    public void DefinirItens(IEnumerable<CompraItem> itens)
    {
        _itens.Clear();
        _itens.AddRange(itens);

        if (_itens.Count == 0)
            throw new InvalidOperationException("A compra precisa de ao menos um item.");

        VlrTotal = _itens.Sum(i => i.VlrTotalItem);
        MarcarAtualizada();
    }
}
