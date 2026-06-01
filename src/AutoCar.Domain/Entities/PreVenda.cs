using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Pré-venda: documento provisório de balcão (cabeçalho + itens). É a raiz do agregado e
/// estabelece o <b>padrão de documento</b> reusado depois em Orçamento/OS/Venda. Nasce
/// <see cref="SituacaoPreVenda.Aberta"/> e é editável; ao ser <see cref="Faturar"/> vira venda
/// (a baixa de estoque ocorre nessa transição, na Fase de Estoque) e torna-se imutável.
///
/// Regras de negócio centralizadas aqui (a UI nunca calcula total): o total é
/// (soma dos itens) − desconto geral, sempre recalculado a cada mudança.
///
/// Cliente é opcional (venda de balcão avulso): ou referencia um <see cref="Cliente"/>
/// cadastrado (<see cref="IdCliente"/>), ou guarda o nome digitado (<see cref="NomeClienteAvulso"/>).
/// Veículo é texto livre no MVP.
/// </summary>
public class PreVenda : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected PreVenda() { }

    public PreVenda(
        Guid idUsuario,
        Guid? idCliente,
        string? nomeClienteAvulso,
        string? veiculoMontadora,
        string? veiculoModelo,
        string? veiculoAno,
        string? veiculoPlaca,
        string? observacao)
    {
        IdUsuario = idUsuario;
        Situacao = SituacaoPreVenda.Aberta;
        FlgAtivo = true;
        AlterarCabecalho(idCliente, nomeClienteAvulso, veiculoMontadora, veiculoModelo,
            veiculoAno, veiculoPlaca, observacao);
        Recalcular();
    }

    /// <summary>Código legível autoincrement (número do documento), gerado pelo banco.</summary>
    public int CodPreVenda { get; protected set; }

    public SituacaoPreVenda Situacao { get; protected set; }

    // --- Cliente (opcional) ---

    /// <summary>FK para cliente cadastrado (opcional — balcão avulso não tem).</summary>
    public Guid? IdCliente { get; protected set; }

    /// <summary>Nome digitado quando a venda é avulsa (sem cliente cadastrado).</summary>
    public string? NomeClienteAvulso { get; protected set; }

    public Cliente? Cliente { get; protected set; }

    // --- Veículo (texto livre no MVP) ---

    public string? VeiculoMontadora { get; protected set; }
    public string? VeiculoModelo { get; protected set; }
    public string? VeiculoAno { get; protected set; }
    public string? VeiculoPlaca { get; protected set; }

    /// <summary>Desconto geral do documento, em valor (R$). Aplicado sobre a soma dos itens.</summary>
    public decimal VlrDesconto { get; protected set; }

    /// <summary>Total final do documento: (soma dos itens) − desconto geral. Calculado e persistido.</summary>
    public decimal VlrTotal { get; protected set; }

    public string? Observacao { get; protected set; }

    /// <summary>Vendedor que abriu o documento (rastreabilidade).</summary>
    public Guid IdUsuario { get; protected set; }

    public bool FlgAtivo { get; protected set; }

    // Itens (1:N). Backing field exposto como somente leitura — alterar via os métodos do agregado.
    private readonly List<PreVendaItem> _itens = new();

    public IReadOnlyList<PreVendaItem> Itens => _itens;

    /// <summary>Soma dos totais de cada item, antes do desconto geral.</summary>
    public decimal SubtotalItens => _itens.Sum(i => i.VlrTotalItem);

    // --- Comportamento ---

    /// <summary>Altera os dados do cabeçalho (cliente/veículo/observação). Só quando Aberta.</summary>
    public void AlterarCabecalho(
        Guid? idCliente,
        string? nomeClienteAvulso,
        string? veiculoMontadora,
        string? veiculoModelo,
        string? veiculoAno,
        string? veiculoPlaca,
        string? observacao)
    {
        GarantirEditavel();
        IdCliente = idCliente;
        // Nome avulso só faz sentido sem cliente cadastrado.
        NomeClienteAvulso = idCliente.HasValue ? null : NormalizarNome(nomeClienteAvulso);
        VeiculoMontadora = NormalizarTexto(veiculoMontadora);
        VeiculoModelo = NormalizarTexto(veiculoModelo);
        VeiculoAno = NormalizarCodigo(veiculoAno);
        VeiculoPlaca = NormalizarTexto(veiculoPlaca);
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        MarcarAtualizada();
    }

    /// <summary>Substitui todos os itens (padrão "salva junto" — o form envia a lista completa a cada
    /// gravação). Recalcula o total. Só quando Aberta.</summary>
    public void DefinirItens(IEnumerable<PreVendaItem> itens)
    {
        GarantirEditavel();
        _itens.Clear();
        _itens.AddRange(itens);
        Recalcular();
    }

    /// <summary>Define o desconto geral do documento (R$) e recalcula o total. Só quando Aberta.</summary>
    public void AplicarDescontoGeral(decimal vlrDesconto)
    {
        GarantirEditavel();
        if (vlrDesconto < 0)
            throw new ArgumentException("O desconto geral não pode ser negativo.", nameof(vlrDesconto));
        if (vlrDesconto > SubtotalItens)
            throw new ArgumentException("O desconto geral não pode ser maior que a soma dos itens.", nameof(vlrDesconto));

        VlrDesconto = vlrDesconto;
        Recalcular();
    }

    /// <summary>Fatura a pré-venda (vira venda) — torna o documento imutável. A baixa de estoque é
    /// disparada nessa transição na Fase de Estoque. Exige ao menos um item.</summary>
    public void Faturar()
    {
        GarantirEditavel();
        if (_itens.Count == 0)
            throw new InvalidOperationException("Não é possível faturar uma pré-venda sem itens.");
        Situacao = SituacaoPreVenda.Faturada;
        MarcarAtualizada();
    }

    /// <summary>Cancela a pré-venda. Só quando Aberta.</summary>
    public void Cancelar()
    {
        GarantirEditavel();
        Situacao = SituacaoPreVenda.Cancelada;
        MarcarAtualizada();
    }

    public void Inativar()
    {
        FlgAtivo = false;
        MarcarAtualizada();
    }

    /// <summary>Recalcula o total a partir dos itens e do desconto geral. Garante que o desconto
    /// geral nunca exceda a soma dos itens (ajusta para baixo se itens foram removidos).</summary>
    private void Recalcular()
    {
        var subtotal = SubtotalItens;
        if (VlrDesconto > subtotal)
            VlrDesconto = subtotal;
        VlrTotal = subtotal - VlrDesconto;
        MarcarAtualizada();
    }

    /// <summary>Bloqueia alterações em documento Faturado ou Cancelado (imutável fora de Aberta).</summary>
    private void GarantirEditavel()
    {
        if (Situacao != SituacaoPreVenda.Aberta)
            throw new InvalidOperationException(
                $"A pré-venda está {Situacao} e não pode mais ser alterada.");
    }

    // Nome do cliente avulso em CAIXA ALTA (padrão do projeto); nulo se vazio.
    private static string? NormalizarNome(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim().ToUpperInvariant();

    // Campos de veículo livres em CAIXA ALTA (montadora/modelo/placa).
    private static string? NormalizarTexto(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim().ToUpperInvariant();

    // Ano: preserva o conteúdo digitado, só apara espaços ("2018" ou "2018/2019").
    private static string? NormalizarCodigo(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
