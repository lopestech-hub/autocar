using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Ordem de Serviço da oficina: documento de cabeçalho + itens (peças e mão de obra). É a raiz do
/// agregado e uma variação enriquecida do padrão de documento da <see cref="PreVenda"/>: herda o total
/// no domínio (a UI nunca calcula) e o snapshot de preço dos itens, e acrescenta o que é próprio da
/// oficina — dois tipos de linha (peça/serviço), mecânico responsável e um ciclo de status mais rico.
///
/// Ciclo: nasce <see cref="SituacaoOrdemServico.Aberta"/> (editável). <see cref="Iniciar"/> →
/// EmAndamento (editável). <see cref="Concluir"/> → Concluida (exige mecânico responsável e ≥1 item).
/// <see cref="Faturar"/> → Faturada (imutável; a baixa de estoque das peças ocorre nessa transição,
/// disparada pelo repositório de faturamento). <see cref="Cancelar"/> encerra (só antes de faturar;
/// não mexe em estoque).
///
/// Cliente é opcional (balcão avulso): ou referencia um <see cref="Cliente"/> cadastrado, ou guarda o
/// nome digitado. Veículo é texto livre no MVP. Mecânico é opcional ao abrir, obrigatório para concluir.
/// </summary>
public class OrdemServico : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected OrdemServico() { }

    public OrdemServico(
        Guid idUsuario,
        Guid? idCliente,
        string? nomeClienteAvulso,
        Guid? idMecanico,
        string? veiculoMontadora,
        string? veiculoModelo,
        string? veiculoAno,
        string? veiculoPlaca,
        string? observacao)
    {
        IdUsuario = idUsuario;
        Situacao = SituacaoOrdemServico.Aberta;
        FlgAtivo = true;
        AlterarCabecalho(idCliente, nomeClienteAvulso, idMecanico, veiculoMontadora,
            veiculoModelo, veiculoAno, veiculoPlaca, observacao);
        Recalcular();
    }

    /// <summary>Código legível autoincrement (número da OS), gerado pelo banco.</summary>
    public int CodOrdemServico { get; protected set; }

    public SituacaoOrdemServico Situacao { get; protected set; }

    // --- Cliente (opcional) ---

    /// <summary>FK para cliente cadastrado (opcional — balcão avulso não tem).</summary>
    public Guid? IdCliente { get; protected set; }

    /// <summary>Nome digitado quando a OS é avulsa (sem cliente cadastrado).</summary>
    public string? NomeClienteAvulso { get; protected set; }

    public Cliente? Cliente { get; protected set; }

    // --- Mecânico responsável (opcional ao abrir, obrigatório para concluir) ---

    /// <summary>FK para o mecânico responsável pela OS (cadastro próprio, não usuário do sistema —
    /// o mecânico não loga). Opcional até a conclusão.</summary>
    public Guid? IdMecanico { get; protected set; }

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

    /// <summary>Atendente/vendedor que abriu a OS (rastreabilidade).</summary>
    public Guid IdUsuario { get; protected set; }

    public bool FlgAtivo { get; protected set; }

    // Itens (1:N). Backing field exposto como somente leitura — alterar via os métodos do agregado.
    private readonly List<OrdemServicoItem> _itens = new();

    public IReadOnlyList<OrdemServicoItem> Itens => _itens;

    /// <summary>Soma dos totais de cada item (peças + serviços), antes do desconto geral.</summary>
    public decimal SubtotalItens => _itens.Sum(i => i.VlrTotalItem);

    /// <summary>Subtotal só das peças (linhas que baixam estoque).</summary>
    public decimal SubtotalPecas => _itens.Where(i => i.EhPeca).Sum(i => i.VlrTotalItem);

    /// <summary>Subtotal só dos serviços (mão de obra).</summary>
    public decimal SubtotalServicos => _itens.Where(i => !i.EhPeca).Sum(i => i.VlrTotalItem);

    // --- Comportamento ---

    /// <summary>Altera os dados do cabeçalho (cliente/mecânico/veículo/observação). Só enquanto editável.</summary>
    public void AlterarCabecalho(
        Guid? idCliente,
        string? nomeClienteAvulso,
        Guid? idMecanico,
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
        IdMecanico = idMecanico;
        VeiculoMontadora = NormalizarTexto(veiculoMontadora);
        VeiculoModelo = NormalizarTexto(veiculoModelo);
        VeiculoAno = NormalizarCodigo(veiculoAno);
        VeiculoPlaca = NormalizarTexto(veiculoPlaca);
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        MarcarAtualizada();
    }

    /// <summary>Substitui todos os itens (padrão "salva junto" — o form envia a lista completa a cada
    /// gravação). Recalcula o total. Só enquanto editável.</summary>
    public void DefinirItens(IEnumerable<OrdemServicoItem> itens)
    {
        GarantirEditavel();
        _itens.Clear();
        _itens.AddRange(itens);
        Recalcular();
    }

    /// <summary>Define o desconto geral do documento (R$) e recalcula o total. Só enquanto editável.</summary>
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

    /// <summary>Marca a OS como EmAndamento (mecânico começou). Só a partir de Aberta.</summary>
    public void Iniciar()
    {
        if (Situacao != SituacaoOrdemServico.Aberta)
            throw new InvalidOperationException(
                $"Só é possível iniciar uma OS Aberta (situação atual: {Situacao}).");
        Situacao = SituacaoOrdemServico.EmAndamento;
        MarcarAtualizada();
    }

    /// <summary>Conclui o serviço. Exige mecânico responsável e ao menos um item. A partir de Aberta
    /// ou EmAndamento (a recepção pode concluir direto uma OS simples).</summary>
    public void Concluir()
    {
        if (Situacao != SituacaoOrdemServico.Aberta && Situacao != SituacaoOrdemServico.EmAndamento)
            throw new InvalidOperationException(
                $"Só é possível concluir uma OS Aberta ou Em andamento (situação atual: {Situacao}).");
        if (IdMecanico is null)
            throw new InvalidOperationException("Defina o mecânico responsável antes de concluir a OS.");
        if (_itens.Count == 0)
            throw new InvalidOperationException("Não é possível concluir uma OS sem itens.");

        Situacao = SituacaoOrdemServico.Concluida;
        MarcarAtualizada();
    }

    /// <summary>Fatura a OS (vira cobrança) — torna o documento imutável. A baixa de estoque das peças
    /// é disparada nessa transição pelo repositório de faturamento. Exige a OS Concluida e ≥1 item.</summary>
    public void Faturar()
    {
        if (Situacao != SituacaoOrdemServico.Concluida)
            throw new InvalidOperationException(
                $"Só é possível faturar uma OS Concluída (situação atual: {Situacao}).");
        if (_itens.Count == 0)
            throw new InvalidOperationException("Não é possível faturar uma OS sem itens.");

        Situacao = SituacaoOrdemServico.Faturada;
        MarcarAtualizada();
    }

    /// <summary>Cancela a OS. Permitido enquanto não faturada (Faturada/Cancelada não mudam).</summary>
    public void Cancelar()
    {
        if (Situacao == SituacaoOrdemServico.Faturada)
            throw new InvalidOperationException("Não é possível cancelar uma OS já faturada.");
        if (Situacao == SituacaoOrdemServico.Cancelada)
            throw new InvalidOperationException("A OS já está cancelada.");

        Situacao = SituacaoOrdemServico.Cancelada;
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

    /// <summary>Bloqueia alterações fora dos estados editáveis (Aberta/EmAndamento). Concluída,
    /// Faturada e Cancelada não aceitam edição de cabeçalho/itens/desconto.</summary>
    private void GarantirEditavel()
    {
        if (Situacao != SituacaoOrdemServico.Aberta && Situacao != SituacaoOrdemServico.EmAndamento)
            throw new InvalidOperationException(
                $"A OS está {Situacao} e não pode mais ser alterada.");
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
