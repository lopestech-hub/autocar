using AutoCar.Domain.Enums;

namespace AutoCar.Application.Modules.Estoque.DTOs;

/// <summary>Dados para registrar um movimento de estoque (vindo da tela). Quantidade inteira positiva.</summary>
public sealed record MovimentarEstoqueDto(
    Guid IdProduto,
    TipoMovimentoEstoque Tipo,
    int Qtd,
    string? Observacao);

/// <summary>Linha da listagem de saldos: produto + saldo atual (saldo zero quando ainda sem movimento).</summary>
public sealed record SaldoEstoqueListaDto(
    Guid IdProduto,
    int CodProduto,
    string Descricao,
    UnidadeMedida Unidade,
    int QtdSaldo,
    int QtdReservada,
    int QtdDisponivel);

/// <summary>Movimento do histórico de um produto (livro-razão), com o tipo e o saldo resultante.</summary>
public sealed record MovimentoEstoqueDto(
    Guid Id,
    int CodMovimento,
    TipoMovimentoEstoque Tipo,
    int Qtd,
    int QtdSaldoApos,
    string? Observacao,
    DateTime DataCriacao);
