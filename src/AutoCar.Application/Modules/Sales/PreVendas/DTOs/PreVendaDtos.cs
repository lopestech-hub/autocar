using AutoCar.Domain.Enums;

namespace AutoCar.Application.Modules.Sales.PreVendas.DTOs;

/// <summary>Item (linha) de uma pré-venda vindo da tela. O preço é snapshot editável.</summary>
public sealed record PreVendaItemDto(
    Guid IdProduto,
    string DescricaoProduto,
    decimal Qtd,
    decimal VlrUnitario,
    decimal VlrDesconto);

/// <summary>Dados de entrada para criar/atualizar uma pré-venda (vindo da tela).</summary>
public sealed record SalvarPreVendaDto(
    Guid? IdCliente,
    string? NomeClienteAvulso,
    string? VeiculoMontadora,
    string? VeiculoModelo,
    string? VeiculoAno,
    string? VeiculoPlaca,
    decimal VlrDesconto,
    string? Observacao,
    IReadOnlyList<PreVendaItemDto> Itens);

/// <summary>Item da pré-venda para o formulário (com o total da linha já calculado).</summary>
public sealed record PreVendaItemDetalheDto(
    Guid Id,
    Guid IdProduto,
    string DescricaoProduto,
    decimal Qtd,
    decimal VlrUnitario,
    decimal VlrDesconto,
    decimal VlrTotalItem);

/// <summary>Pré-venda completa para o formulário (visualização/edição).</summary>
public sealed record PreVendaDto(
    Guid Id,
    int CodPreVenda,
    SituacaoPreVenda Situacao,
    Guid? IdCliente,
    string? NomeClienteAvulso,
    string? VeiculoMontadora,
    string? VeiculoModelo,
    string? VeiculoAno,
    string? VeiculoPlaca,
    decimal SubtotalItens,
    decimal VlrDesconto,
    decimal VlrTotal,
    string? Observacao,
    bool FlgAtivo,
    IReadOnlyList<PreVendaItemDetalheDto> Itens);

/// <summary>Linha enxuta para a listagem de pré-vendas (com o nome do cliente já resolvido).</summary>
public sealed record PreVendaListaDto(
    Guid Id,
    int CodPreVenda,
    SituacaoPreVenda Situacao,
    string Cliente,
    int QtdItens,
    decimal VlrTotal,
    DateTime DataCriacao);
