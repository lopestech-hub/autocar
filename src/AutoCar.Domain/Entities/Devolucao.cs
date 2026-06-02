using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Devolução de itens de uma venda (pré-venda Faturada) ao estoque. É a raiz do agregado e um
/// documento à parte que aponta para a venda de origem — não "desfatura" a venda (o histórico de que
/// houve venda + devolução é preservado). Cada item devolvido gera uma ENTRADA no estoque (origem
/// Devolução), na mesma transação em que o documento é salvo (garantido pelo repositório).
///
/// Devolução parcial: o usuário escolhe quais itens e que quantidade devolver. A regra de "não
/// devolver mais do que o saldo devolvível" (vendido − já devolvido) é validada na camada de
/// aplicação, que conhece o total vendido e as devoluções anteriores — aqui o agregado garante as
/// invariantes locais (≥1 item, quantidades positivas, total coerente).
/// </summary>
public class Devolucao : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected Devolucao() { }

    public Devolucao(Guid idPreVenda, Guid idUsuario, string? motivo)
    {
        IdPreVenda = idPreVenda;
        IdUsuario = idUsuario;
        Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
    }

    /// <summary>Código legível autoincrement (número do documento de devolução), gerado pelo banco.</summary>
    public int CodDevolucao { get; protected set; }

    /// <summary>FK para a pré-venda de origem (Restrict — a venda permanece para histórico).</summary>
    public Guid IdPreVenda { get; protected set; }

    /// <summary>Usuário que registrou a devolução (rastreabilidade).</summary>
    public Guid IdUsuario { get; protected set; }

    /// <summary>Motivo da devolução (texto livre, opcional).</summary>
    public string? Motivo { get; protected set; }

    /// <summary>Valor total devolvido (soma dos itens). Calculado no domínio e persistido.</summary>
    public decimal VlrTotal { get; protected set; }

    // Itens (1:N). Backing field exposto como somente leitura — definir via DefinirItens.
    private readonly List<DevolucaoItem> _itens = new();

    public IReadOnlyList<DevolucaoItem> Itens => _itens;

    /// <summary>Define os itens devolvidos (≥1) e calcula o total. Exige ao menos um item.</summary>
    public void DefinirItens(IEnumerable<DevolucaoItem> itens)
    {
        _itens.Clear();
        _itens.AddRange(itens);

        if (_itens.Count == 0)
            throw new InvalidOperationException("A devolução precisa de ao menos um item.");

        VlrTotal = _itens.Sum(i => i.VlrTotalItem);
        MarcarAtualizada();
    }
}
