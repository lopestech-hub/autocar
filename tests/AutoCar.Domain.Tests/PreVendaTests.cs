using System;
using System.Collections.Generic;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using Xunit;

namespace AutoCar.Domain.Tests;

/// <summary>
/// Testes do agregado PreVenda — cálculo de total (item, desconto por linha e geral) e
/// invariantes do ciclo de vida (só edita quando Aberta; não fatura sem itens). O cálculo
/// mora no domínio, então é aqui que ele é garantido — a UI só exibe.
/// </summary>
public class PreVendaTests
{
    private static readonly Guid Usuario = Guid.NewGuid();
    private static readonly Guid Produto = Guid.NewGuid();

    private static PreVenda NovaAberta() =>
        new(Usuario, idCliente: null, nomeClienteAvulso: "CONSUMIDOR",
            veiculoMontadora: null, veiculoModelo: null, veiculoAno: null, veiculoPlaca: null,
            observacao: null);

    private static PreVendaItem Item(decimal qtd, decimal unitario, decimal desconto = 0) =>
        new(Produto, "PASTILHA DE FREIO", qtd, unitario, desconto);

    [Fact]
    public void Total_do_item_e_qtd_vezes_unitario_menos_desconto()
    {
        var item = Item(qtd: 3, unitario: 50, desconto: 20);
        Assert.Equal(130m, item.VlrTotalItem); // 3*50 - 20
        Assert.Equal(150m, item.Subtotal);     // 3*50
    }

    [Fact]
    public void Total_da_pre_venda_soma_itens_menos_desconto_geral()
    {
        var pv = NovaAberta();
        pv.DefinirItens(new List<PreVendaItem> { Item(2, 100), Item(1, 50) }); // 200 + 50 = 250
        pv.AplicarDescontoGeral(30);

        Assert.Equal(250m, pv.SubtotalItens);
        Assert.Equal(220m, pv.VlrTotal); // 250 - 30
    }

    [Fact]
    public void Total_sem_desconto_e_a_soma_pura_dos_itens()
    {
        var pv = NovaAberta();
        pv.DefinirItens(new List<PreVendaItem> { Item(2, 100), Item(3, 10) }); // 200 + 30

        Assert.Equal(230m, pv.VlrTotal);
    }

    [Fact]
    public void Desconto_geral_maior_que_subtotal_e_rejeitado()
    {
        var pv = NovaAberta();
        pv.DefinirItens(new List<PreVendaItem> { Item(1, 100) });

        Assert.Throws<ArgumentException>(() => pv.AplicarDescontoGeral(150));
    }

    [Fact]
    public void Desconto_do_item_maior_que_subtotal_da_linha_e_rejeitado()
    {
        Assert.Throws<ArgumentException>(() => Item(qtd: 1, unitario: 100, desconto: 150));
    }

    [Fact]
    public void Quantidade_zero_ou_negativa_e_rejeitada()
    {
        Assert.Throws<ArgumentException>(() => Item(qtd: 0, unitario: 100));
        Assert.Throws<ArgumentException>(() => Item(qtd: -1, unitario: 100));
    }

    [Fact]
    public void Remover_item_reduz_o_desconto_geral_para_nao_exceder_o_subtotal()
    {
        var pv = NovaAberta();
        pv.DefinirItens(new List<PreVendaItem> { Item(1, 100), Item(1, 100) }); // 200
        pv.AplicarDescontoGeral(150);
        Assert.Equal(50m, pv.VlrTotal);

        // Some um item (subtotal cai para 100); o desconto de 150 é ajustado para 100.
        pv.DefinirItens(new List<PreVendaItem> { Item(1, 100) });
        Assert.Equal(100m, pv.VlrDesconto);
        Assert.Equal(0m, pv.VlrTotal);
    }

    [Fact]
    public void Nova_pre_venda_nasce_aberta()
    {
        Assert.Equal(SituacaoPreVenda.Aberta, NovaAberta().Situacao);
    }

    [Fact]
    public void Faturar_sem_itens_e_rejeitado()
    {
        var pv = NovaAberta();
        Assert.Throws<InvalidOperationException>(() => pv.Faturar());
    }

    [Fact]
    public void Faturar_com_itens_muda_situacao_para_faturada()
    {
        var pv = NovaAberta();
        pv.DefinirItens(new List<PreVendaItem> { Item(1, 100) });
        pv.Faturar();

        Assert.Equal(SituacaoPreVenda.Faturada, pv.Situacao);
    }

    [Fact]
    public void Pre_venda_faturada_nao_pode_ser_alterada()
    {
        var pv = NovaAberta();
        pv.DefinirItens(new List<PreVendaItem> { Item(1, 100) });
        pv.Faturar();

        // Qualquer alteração de cabeçalho/itens/desconto/cancelamento deve ser bloqueada.
        Assert.Throws<InvalidOperationException>(() => pv.AplicarDescontoGeral(10));
        Assert.Throws<InvalidOperationException>(() => pv.DefinirItens(new List<PreVendaItem> { Item(1, 50) }));
        Assert.Throws<InvalidOperationException>(() => pv.Cancelar());
    }

    [Fact]
    public void Cancelar_muda_situacao_e_bloqueia_novas_alteracoes()
    {
        var pv = NovaAberta();
        pv.DefinirItens(new List<PreVendaItem> { Item(1, 100) });
        pv.Cancelar();

        Assert.Equal(SituacaoPreVenda.Cancelada, pv.Situacao);
        Assert.Throws<InvalidOperationException>(() => pv.Faturar());
    }

    [Fact]
    public void Selecionar_cliente_cadastrado_limpa_o_nome_avulso()
    {
        var pv = NovaAberta(); // nasce com nome avulso "CONSUMIDOR"
        var idCliente = Guid.NewGuid();

        pv.AlterarCabecalho(idCliente, nomeClienteAvulso: "IGNORADO",
            null, null, null, null, observacao: null);

        Assert.Equal(idCliente, pv.IdCliente);
        Assert.Null(pv.NomeClienteAvulso); // com cliente cadastrado, não guarda nome avulso
    }

    [Fact]
    public void Veiculo_texto_livre_e_normalizado_em_caixa_alta()
    {
        var pv = NovaAberta();
        pv.AlterarCabecalho(idCliente: null, nomeClienteAvulso: null,
            veiculoMontadora: "vw", veiculoModelo: "gol", veiculoAno: "2018", veiculoPlaca: "abc1d23",
            observacao: null);

        Assert.Equal("VW", pv.VeiculoMontadora);
        Assert.Equal("GOL", pv.VeiculoModelo);
        Assert.Equal("2018", pv.VeiculoAno);   // ano preserva conteúdo (só Trim)
        Assert.Equal("ABC1D23", pv.VeiculoPlaca);
    }
}
