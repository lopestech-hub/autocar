using System;
using System.Collections.Generic;
using AutoCar.Domain.Entities;
using Xunit;

namespace AutoCar.Domain.Tests;

/// <summary>
/// Testes do agregado Compra — invariantes locais: total = soma dos itens, exige ≥1 item, e o item
/// comprado valida quantidade/custo. A existência de fornecedor/produto (FK) é da camada de aplicação,
/// não testada aqui.
/// </summary>
public class CompraTests
{
    private static readonly Guid Fornecedor = Guid.NewGuid();
    private static readonly Guid Usuario = Guid.NewGuid();
    private static readonly Guid Produto = Guid.NewGuid();

    private static CompraItem Item(int qtd, decimal custo = 100m) =>
        new(Produto, "PASTILHA DE FREIO", qtd, custo);

    [Fact]
    public void Total_da_compra_e_a_soma_dos_itens()
    {
        var compra = new Compra(Fornecedor, Usuario, numDocumento: "NF-1234", observacao: null);
        compra.DefinirItens(new List<CompraItem> { Item(2, 50), Item(1, 30) }); // 100 + 30

        Assert.Equal(130m, compra.VlrTotal);
        Assert.Equal(2, compra.Itens.Count);
    }

    [Fact]
    public void Total_do_item_e_qtd_vezes_custo()
    {
        var item = Item(qtd: 3, custo: 40);
        Assert.Equal(120m, item.VlrTotalItem);
    }

    [Fact]
    public void Compra_sem_itens_e_rejeitada()
    {
        var compra = new Compra(Fornecedor, Usuario, numDocumento: null, observacao: null);
        Assert.Throws<InvalidOperationException>(() => compra.DefinirItens(new List<CompraItem>()));
    }

    [Fact]
    public void Quantidade_zero_ou_negativa_no_item_e_rejeitada()
    {
        Assert.Throws<ArgumentException>(() => Item(qtd: 0));
        Assert.Throws<ArgumentException>(() => Item(qtd: -1));
    }

    [Fact]
    public void Custo_unitario_negativo_e_rejeitado()
    {
        Assert.Throws<ArgumentException>(() => new CompraItem(Produto, "X", 1, -10m));
    }

    [Fact]
    public void Numero_documento_e_observacao_vazios_viram_nulo()
    {
        var compra = new Compra(Fornecedor, Usuario, numDocumento: "   ", observacao: "  ");
        Assert.Null(compra.NumDocumento);
        Assert.Null(compra.Observacao);
    }

    [Fact]
    public void Guarda_o_fornecedor_e_o_usuario()
    {
        var compra = new Compra(Fornecedor, Usuario, numDocumento: null, observacao: null);
        Assert.Equal(Fornecedor, compra.IdFornecedor);
        Assert.Equal(Usuario, compra.IdUsuario);
    }
}
