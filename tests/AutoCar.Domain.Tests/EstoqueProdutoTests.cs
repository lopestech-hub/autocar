using System;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using Xunit;

namespace AutoCar.Domain.Tests;

/// <summary>
/// Testes do agregado EstoqueProduto — as regras de movimentação moram no domínio: quantidade
/// sempre positiva, saldo nunca negativo (não vende o que não tem), e cada movimento registra o
/// saldo resultante. Quantidades inteiras (autopeça não fraciona).
/// </summary>
public class EstoqueProdutoTests
{
    private static readonly Guid Produto = Guid.NewGuid();
    private static readonly Guid Usuario = Guid.NewGuid();

    private static EstoqueProduto NovoSaldo() => new(Produto);

    [Fact]
    public void Novo_saldo_nasce_zerado()
    {
        var estoque = NovoSaldo();
        Assert.Equal(0, estoque.QtdSaldo);
        Assert.Equal(0, estoque.QtdReservada);
        Assert.Equal(0, estoque.QtdDisponivel);
    }

    [Fact]
    public void Entrada_eleva_o_saldo()
    {
        var estoque = NovoSaldo();
        var mov = estoque.Movimentar(TipoMovimentoEstoque.Entrada, 10, Usuario);

        Assert.Equal(10, estoque.QtdSaldo);
        Assert.Equal(TipoMovimentoEstoque.Entrada, mov.Tipo);
        Assert.Equal(10, mov.Qtd);
        Assert.Equal(10, mov.QtdSaldoApos); // foto do saldo após o movimento
        Assert.Equal(Produto, mov.IdProduto);
        Assert.Equal(Usuario, mov.IdUsuario);
    }

    [Fact]
    public void Saida_abaixa_o_saldo()
    {
        var estoque = NovoSaldo();
        estoque.Movimentar(TipoMovimentoEstoque.Entrada, 10, Usuario);
        var mov = estoque.Movimentar(TipoMovimentoEstoque.Saida, 4, Usuario);

        Assert.Equal(6, estoque.QtdSaldo);
        Assert.Equal(4, mov.Qtd);          // a quantidade do movimento é sempre positiva
        Assert.Equal(6, mov.QtdSaldoApos);
    }

    [Fact]
    public void Saida_maior_que_o_saldo_e_rejeitada()
    {
        var estoque = NovoSaldo();
        estoque.Movimentar(TipoMovimentoEstoque.Entrada, 3, Usuario);

        Assert.Throws<InvalidOperationException>(
            () => estoque.Movimentar(TipoMovimentoEstoque.Saida, 4, Usuario));
        Assert.Equal(3, estoque.QtdSaldo); // saldo não muda quando a operação é rejeitada
    }

    [Fact]
    public void Saida_que_zera_o_saldo_e_permitida()
    {
        var estoque = NovoSaldo();
        estoque.Movimentar(TipoMovimentoEstoque.Entrada, 5, Usuario);
        estoque.Movimentar(TipoMovimentoEstoque.Saida, 5, Usuario);

        Assert.Equal(0, estoque.QtdSaldo);
    }

    [Fact]
    public void Ajuste_positivo_eleva_e_negativo_abaixa()
    {
        var estoque = NovoSaldo();
        estoque.Movimentar(TipoMovimentoEstoque.Entrada, 10, Usuario);
        estoque.Movimentar(TipoMovimentoEstoque.AjustePositivo, 2, Usuario);
        Assert.Equal(12, estoque.QtdSaldo);

        estoque.Movimentar(TipoMovimentoEstoque.AjusteNegativo, 5, Usuario);
        Assert.Equal(7, estoque.QtdSaldo);
    }

    [Fact]
    public void Ajuste_negativo_maior_que_o_saldo_e_rejeitado()
    {
        var estoque = NovoSaldo();
        estoque.Movimentar(TipoMovimentoEstoque.Entrada, 2, Usuario);

        Assert.Throws<InvalidOperationException>(
            () => estoque.Movimentar(TipoMovimentoEstoque.AjusteNegativo, 3, Usuario));
    }

    [Fact]
    public void Quantidade_zero_ou_negativa_e_rejeitada()
    {
        var estoque = NovoSaldo();
        Assert.Throws<ArgumentException>(() => estoque.Movimentar(TipoMovimentoEstoque.Entrada, 0, Usuario));
        Assert.Throws<ArgumentException>(() => estoque.Movimentar(TipoMovimentoEstoque.Entrada, -1, Usuario));
    }

    [Fact]
    public void Disponivel_desconta_o_reservado()
    {
        var estoque = NovoSaldo();
        estoque.Movimentar(TipoMovimentoEstoque.Entrada, 10, Usuario);
        // Reservado é 0 no MVP, então disponível = saldo.
        Assert.Equal(10, estoque.QtdDisponivel);
    }
}
