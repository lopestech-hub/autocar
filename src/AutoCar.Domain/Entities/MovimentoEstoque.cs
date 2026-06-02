using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Movimento de estoque: uma linha do livro-razão. Cada entrada, saída ou ajuste de um produto
/// gera um <see cref="MovimentoEstoque"/> <b>imutável</b> — nunca é alterado depois de criado, só
/// consultado. Dá rastreabilidade (quem movimentou, quando, quanto, e o saldo resultante).
///
/// É criado exclusivamente por <see cref="EstoqueProduto.Movimentar"/>, que garante a coerência
/// entre o saldo e o movimento. A quantidade é sempre positiva; o <see cref="Tipo"/> define a direção.
/// Sem `xmin` — registro imutável não sofre edição concorrente. Tem `cod_` (rastreável na tela).
/// </summary>
public class MovimentoEstoque : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected MovimentoEstoque() { }

    /// <summary>Cria um movimento. Uso interno do agregado (<see cref="EstoqueProduto.Movimentar"/>),
    /// que já validou quantidade e saldo. A quantidade deve ser positiva.</summary>
    internal MovimentoEstoque(
        Guid idProduto,
        TipoMovimentoEstoque tipo,
        int qtd,
        int qtdSaldoApos,
        Guid idUsuario,
        string? observacao,
        OrigemMovimento origem,
        Guid? idDocumentoOrigem,
        int? codDocumentoOrigem)
    {
        IdProduto = idProduto;
        Tipo = tipo;
        Qtd = qtd;
        QtdSaldoApos = qtdSaldoApos;
        IdUsuario = idUsuario;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        Origem = origem;
        IdDocumentoOrigem = idDocumentoOrigem;
        CodDocumentoOrigem = codDocumentoOrigem;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodMovimento { get; protected set; }

    /// <summary>FK para o produto movimentado (Restrict — não se apaga produto com histórico).</summary>
    public Guid IdProduto { get; protected set; }

    /// <summary>Tipo do movimento (define a direção — entrada/saída/ajuste).</summary>
    public TipoMovimentoEstoque Tipo { get; protected set; }

    /// <summary>Quantidade movimentada, sempre positiva. Inteira — autopeça não fraciona.</summary>
    public int Qtd { get; protected set; }

    /// <summary>Saldo do produto logo após este movimento (foto para auditoria).</summary>
    public int QtdSaldoApos { get; protected set; }

    /// <summary>Usuário que registrou o movimento (rastreabilidade).</summary>
    public Guid IdUsuario { get; protected set; }

    /// <summary>Observação livre (motivo do ajuste, nº da nota, etc.). Opcional.</summary>
    public string? Observacao { get; protected set; }

    /// <summary>Origem do movimento (Manual, Venda, Compra...). Dado estruturado, não texto livre.</summary>
    public OrigemMovimento Origem { get; protected set; }

    /// <summary>Id do documento que originou o movimento (ex: a pré-venda faturada). Null se Manual —
    /// permite rastrear a venda/compra a partir do estoque.</summary>
    public Guid? IdDocumentoOrigem { get; protected set; }

    /// <summary>Número legível do documento de origem (ex: nº da pré-venda) — para exibir "Venda nº 1"
    /// no histórico sem precisar de join cross-módulo. Null se Manual.</summary>
    public int? CodDocumentoOrigem { get; protected set; }
}
