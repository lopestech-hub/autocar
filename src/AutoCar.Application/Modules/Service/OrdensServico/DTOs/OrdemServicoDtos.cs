using AutoCar.Domain.Enums;

namespace AutoCar.Application.Modules.Service.OrdensServico.DTOs;

/// <summary>
/// Item (linha) de uma OS vindo da tela. O <see cref="Tipo"/> diz se é peça (FK <see cref="IdProduto"/>)
/// ou serviço (FK <see cref="IdServico"/>) — exatamente uma das duas é preenchida. O valor é snapshot editável.
/// </summary>
public sealed record OrdemServicoItemDto(
    TipoItemOrdemServico Tipo,
    Guid? IdProduto,
    Guid? IdServico,
    string DescricaoItem,
    int Qtd,
    decimal VlrUnitario,
    decimal VlrDesconto);

/// <summary>Dados de entrada para criar/atualizar uma OS (vindo da tela).</summary>
public sealed record SalvarOrdemServicoDto(
    Guid? IdCliente,
    string? NomeClienteAvulso,
    Guid? IdUsuarioMecanico,
    string? VeiculoMontadora,
    string? VeiculoModelo,
    string? VeiculoAno,
    string? VeiculoPlaca,
    decimal VlrDesconto,
    string? Observacao,
    IReadOnlyList<OrdemServicoItemDto> Itens);

/// <summary>Item da OS para o formulário (com o total da linha já calculado).</summary>
public sealed record OrdemServicoItemDetalheDto(
    Guid Id,
    TipoItemOrdemServico Tipo,
    Guid? IdProduto,
    Guid? IdServico,
    string DescricaoItem,
    int Qtd,
    decimal VlrUnitario,
    decimal VlrDesconto,
    decimal VlrTotalItem);

/// <summary>OS completa para o formulário (visualização/edição).</summary>
public sealed record OrdemServicoDto(
    Guid Id,
    int CodOrdemServico,
    SituacaoOrdemServico Situacao,
    Guid? IdCliente,
    string? NomeClienteAvulso,
    Guid? IdUsuarioMecanico,
    string? VeiculoMontadora,
    string? VeiculoModelo,
    string? VeiculoAno,
    string? VeiculoPlaca,
    decimal SubtotalItens,
    decimal SubtotalPecas,
    decimal SubtotalServicos,
    decimal VlrDesconto,
    decimal VlrTotal,
    string? Observacao,
    bool FlgAtivo,
    IReadOnlyList<OrdemServicoItemDetalheDto> Itens);

/// <summary>Linha enxuta para a listagem de OS (com o nome do cliente já resolvido).</summary>
public sealed record OrdemServicoListaDto(
    Guid Id,
    int CodOrdemServico,
    SituacaoOrdemServico Situacao,
    string Cliente,
    string? VeiculoPlaca,
    Guid? IdUsuarioMecanico,
    int QtdItens,
    decimal VlrTotal,
    DateTime DataCriacao);
