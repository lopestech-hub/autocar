using System;
using System.Collections.Generic;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using Xunit;

namespace AutoCar.Domain.Tests;

/// <summary>
/// Testes do agregado OrdemServico — cálculo de total (peças + serviços, descontos), subtotais por
/// tipo, invariante peça×serviço (via factories do item) e o ciclo de vida completo
/// (Aberta→EmAndamento→Concluída→Faturada/Cancelada) com suas regras: mecânico obrigatório para
/// concluir, faturar só a partir de Concluída, não cancelar faturada, imutabilidade fora dos estados
/// editáveis. O cálculo e as transições moram no domínio — é aqui que são garantidos.
/// </summary>
public class OrdemServicoTests
{
    private static readonly Guid Usuario = Guid.NewGuid();
    private static readonly Guid Mecanico = Guid.NewGuid();
    private static readonly Guid Produto = Guid.NewGuid();
    private static readonly Guid ServicoId = Guid.NewGuid();

    private static OrdemServico NovaAberta(Guid? mecanico = null) =>
        new(Usuario, idCliente: null, nomeClienteAvulso: "CONSUMIDOR", idUsuarioMecanico: mecanico,
            veiculoMontadora: null, veiculoModelo: null, veiculoAno: null, veiculoPlaca: null,
            observacao: null);

    private static OrdemServicoItem Peca(int qtd, decimal unitario, decimal desconto = 0) =>
        OrdemServicoItem.DePeca(Produto, "PASTILHA DE FREIO", qtd, unitario, desconto);

    private static OrdemServicoItem ServicoLinha(int qtd, decimal unitario, decimal desconto = 0) =>
        OrdemServicoItem.DeServico(ServicoId, "ALINHAMENTO", qtd, unitario, desconto);

    // --- Itens e cálculo ---

    [Fact]
    public void Item_de_peca_aponta_para_produto_e_e_peca()
    {
        var item = Peca(qtd: 2, unitario: 50);
        Assert.Equal(TipoItemOrdemServico.Peca, item.Tipo);
        Assert.Equal(Produto, item.IdProduto);
        Assert.Null(item.IdServico);
        Assert.True(item.EhPeca);
    }

    [Fact]
    public void Item_de_servico_aponta_para_servico_e_nao_e_peca()
    {
        var item = ServicoLinha(qtd: 1, unitario: 80);
        Assert.Equal(TipoItemOrdemServico.Servico, item.Tipo);
        Assert.Equal(ServicoId, item.IdServico);
        Assert.Null(item.IdProduto);
        Assert.False(item.EhPeca);
    }

    [Fact]
    public void Total_do_item_e_qtd_vezes_unitario_menos_desconto()
    {
        var item = Peca(qtd: 3, unitario: 50, desconto: 20);
        Assert.Equal(130m, item.VlrTotalItem); // 3*50 - 20
    }

    [Fact]
    public void Total_da_os_soma_pecas_e_servicos_menos_desconto_geral()
    {
        var os = NovaAberta();
        os.DefinirItens(new List<OrdemServicoItem> { Peca(2, 100), ServicoLinha(1, 80) }); // 200 + 80 = 280
        os.AplicarDescontoGeral(30);

        Assert.Equal(280m, os.SubtotalItens);
        Assert.Equal(250m, os.VlrTotal); // 280 - 30
    }

    [Fact]
    public void Subtotais_por_tipo_separam_pecas_de_servicos()
    {
        var os = NovaAberta();
        os.DefinirItens(new List<OrdemServicoItem> { Peca(2, 100), ServicoLinha(1, 80), ServicoLinha(1, 20) });

        Assert.Equal(200m, os.SubtotalPecas);    // 2*100
        Assert.Equal(100m, os.SubtotalServicos); // 80 + 20
        Assert.Equal(300m, os.SubtotalItens);
    }

    [Fact]
    public void Desconto_geral_maior_que_subtotal_e_rejeitado()
    {
        var os = NovaAberta();
        os.DefinirItens(new List<OrdemServicoItem> { Peca(1, 100) });

        Assert.Throws<ArgumentException>(() => os.AplicarDescontoGeral(150));
    }

    [Fact]
    public void Quantidade_zero_ou_negativa_e_rejeitada()
    {
        Assert.Throws<ArgumentException>(() => Peca(qtd: 0, unitario: 100));
        Assert.Throws<ArgumentException>(() => ServicoLinha(qtd: -1, unitario: 100));
    }

    // --- Ciclo de vida ---

    [Fact]
    public void Nova_os_nasce_aberta()
    {
        Assert.Equal(SituacaoOrdemServico.Aberta, NovaAberta().Situacao);
    }

    [Fact]
    public void Iniciar_muda_de_aberta_para_em_andamento()
    {
        var os = NovaAberta();
        os.Iniciar();
        Assert.Equal(SituacaoOrdemServico.EmAndamento, os.Situacao);
    }

    [Fact]
    public void Concluir_exige_mecanico_responsavel()
    {
        var os = NovaAberta(mecanico: null); // sem mecânico
        os.DefinirItens(new List<OrdemServicoItem> { Peca(1, 100) });

        Assert.Throws<InvalidOperationException>(() => os.Concluir());
    }

    [Fact]
    public void Concluir_exige_ao_menos_um_item()
    {
        var os = NovaAberta(mecanico: Mecanico); // com mecânico, sem itens
        Assert.Throws<InvalidOperationException>(() => os.Concluir());
    }

    [Fact]
    public void Concluir_com_mecanico_e_itens_muda_para_concluida()
    {
        var os = NovaAberta(mecanico: Mecanico);
        os.DefinirItens(new List<OrdemServicoItem> { Peca(1, 100) });
        os.Concluir();

        Assert.Equal(SituacaoOrdemServico.Concluida, os.Situacao);
    }

    [Fact]
    public void Faturar_so_e_permitido_a_partir_de_concluida()
    {
        var os = NovaAberta(mecanico: Mecanico);
        os.DefinirItens(new List<OrdemServicoItem> { Peca(1, 100) });

        // Aberta não fatura direto.
        Assert.Throws<InvalidOperationException>(() => os.Faturar());

        os.Concluir();
        os.Faturar();
        Assert.Equal(SituacaoOrdemServico.Faturada, os.Situacao);
    }

    [Fact]
    public void Os_concluida_nao_pode_mais_ser_editada()
    {
        var os = NovaAberta(mecanico: Mecanico);
        os.DefinirItens(new List<OrdemServicoItem> { Peca(1, 100) });
        os.Concluir();

        // Concluída é imutável (a edição vale só em Aberta/EmAndamento).
        Assert.Throws<InvalidOperationException>(() => os.AplicarDescontoGeral(10));
        Assert.Throws<InvalidOperationException>(() => os.DefinirItens(new List<OrdemServicoItem> { Peca(1, 50) }));
    }

    [Fact]
    public void Os_faturada_e_imutavel_e_nao_cancela()
    {
        var os = NovaAberta(mecanico: Mecanico);
        os.DefinirItens(new List<OrdemServicoItem> { Peca(1, 100) });
        os.Concluir();
        os.Faturar();

        Assert.Throws<InvalidOperationException>(() => os.AplicarDescontoGeral(10));
        Assert.Throws<InvalidOperationException>(() => os.Cancelar());
    }

    [Fact]
    public void Cancelar_antes_de_faturar_muda_situacao_e_bloqueia_faturamento()
    {
        var os = NovaAberta(mecanico: Mecanico);
        os.DefinirItens(new List<OrdemServicoItem> { Peca(1, 100) });
        os.Cancelar();

        Assert.Equal(SituacaoOrdemServico.Cancelada, os.Situacao);
        Assert.Throws<InvalidOperationException>(() => os.Faturar());
    }

    [Fact]
    public void Editar_em_andamento_e_permitido()
    {
        var os = NovaAberta(mecanico: Mecanico);
        os.Iniciar();
        // Em andamento ainda aceita itens e desconto (o trabalho está em curso).
        os.DefinirItens(new List<OrdemServicoItem> { Peca(2, 100), ServicoLinha(1, 50) });
        os.AplicarDescontoGeral(50);

        Assert.Equal(200m, os.VlrTotal); // 250 - 50
        Assert.Equal(SituacaoOrdemServico.EmAndamento, os.Situacao);
    }

    [Fact]
    public void Concluir_pode_acontecer_direto_de_aberta()
    {
        // OS simples de balcão: recepção registra e conclui sem passar por "em andamento".
        var os = NovaAberta(mecanico: Mecanico);
        os.DefinirItens(new List<OrdemServicoItem> { ServicoLinha(1, 80) });
        os.Concluir();

        Assert.Equal(SituacaoOrdemServico.Concluida, os.Situacao);
    }

    // --- Cabeçalho ---

    [Fact]
    public void Selecionar_cliente_cadastrado_limpa_o_nome_avulso()
    {
        var os = NovaAberta();
        var idCliente = Guid.NewGuid();

        os.AlterarCabecalho(idCliente, nomeClienteAvulso: "IGNORADO", idUsuarioMecanico: null,
            null, null, null, null, observacao: null);

        Assert.Equal(idCliente, os.IdCliente);
        Assert.Null(os.NomeClienteAvulso);
    }

    [Fact]
    public void Veiculo_texto_livre_e_normalizado_em_caixa_alta()
    {
        var os = NovaAberta();
        os.AlterarCabecalho(idCliente: null, nomeClienteAvulso: null, idUsuarioMecanico: null,
            veiculoMontadora: "vw", veiculoModelo: "gol", veiculoAno: "2018", veiculoPlaca: "abc1d23",
            observacao: null);

        Assert.Equal("VW", os.VeiculoMontadora);
        Assert.Equal("GOL", os.VeiculoModelo);
        Assert.Equal("2018", os.VeiculoAno);
        Assert.Equal("ABC1D23", os.VeiculoPlaca);
    }
}
