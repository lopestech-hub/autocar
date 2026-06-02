using System;
using System.Collections.Generic;
using AutoCar.Domain.Entities;
using Xunit;

namespace AutoCar.Domain.Tests;

/// <summary>
/// Testes do agregado Devolucao — invariantes locais: total = soma dos itens, exige ≥1 item, e o
/// item devolvido valida quantidade/valor. A regra de "não devolver mais que o saldo devolvível"
/// (vendido − já devolvido) é da camada de aplicação, não testada aqui.
/// </summary>
public class DevolucaoTests
{
    private static readonly Guid PreVenda = Guid.NewGuid();
    private static readonly Guid Usuario = Guid.NewGuid();
    private static readonly Guid Produto = Guid.NewGuid();

    private static DevolucaoItem Item(int qtd, decimal unitario = 100m) =>
        new(Produto, "PASTILHA DE FREIO", qtd, unitario);

    [Fact]
    public void Total_da_devolucao_e_a_soma_dos_itens()
    {
        var dev = new Devolucao(PreVenda, Usuario, motivo: "Peça errada");
        dev.DefinirItens(new List<DevolucaoItem> { Item(2, 50), Item(1, 30) }); // 100 + 30

        Assert.Equal(130m, dev.VlrTotal);
        Assert.Equal(2, dev.Itens.Count);
    }

    [Fact]
    public void Total_do_item_e_qtd_vezes_unitario()
    {
        var item = Item(qtd: 3, unitario: 40);
        Assert.Equal(120m, item.VlrTotalItem);
    }

    [Fact]
    public void Devolucao_sem_itens_e_rejeitada()
    {
        var dev = new Devolucao(PreVenda, Usuario, motivo: null);
        Assert.Throws<InvalidOperationException>(() => dev.DefinirItens(new List<DevolucaoItem>()));
    }

    [Fact]
    public void Quantidade_zero_ou_negativa_no_item_e_rejeitada()
    {
        Assert.Throws<ArgumentException>(() => Item(qtd: 0));
        Assert.Throws<ArgumentException>(() => Item(qtd: -1));
    }

    [Fact]
    public void Valor_unitario_negativo_e_rejeitado()
    {
        Assert.Throws<ArgumentException>(() => new DevolucaoItem(Produto, "X", 1, -10m));
    }

    [Fact]
    public void Motivo_vazio_vira_nulo()
    {
        var dev = new Devolucao(PreVenda, Usuario, motivo: "   ");
        Assert.Null(dev.Motivo);
    }

    [Fact]
    public void Guarda_a_venda_de_origem_e_o_usuario()
    {
        var dev = new Devolucao(PreVenda, Usuario, motivo: null);
        Assert.Equal(PreVenda, dev.IdPreVenda);
        Assert.Equal(Usuario, dev.IdUsuario);
    }
}
