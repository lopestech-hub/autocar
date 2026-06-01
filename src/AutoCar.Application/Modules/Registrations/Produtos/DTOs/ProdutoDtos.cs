using AutoCar.Domain.Enums;

namespace AutoCar.Application.Modules.Registrations.Produtos.DTOs;

/// <summary>Aplicação por veículo (montadora/modelo/ano + motorização/combustível). Texto livre no MVP.</summary>
public sealed record AplicacaoDto(
    string Montadora,
    string Modelo,
    int? AnoInicio,
    int? AnoFim,
    string? Motorizacao,
    Combustivel Combustivel,
    string? Observacao);

/// <summary>Dados de entrada para criar/atualizar um produto (vindo da tela).</summary>
public sealed record SalvarProdutoDto(
    Guid IdCategoria,
    string Descricao,
    string? DescricaoComplementar,
    string? CodBarras,
    string? CodFabricante,
    UnidadeMedida Unidade,
    PosicaoPeca Posicao,
    decimal VlrCusto,
    decimal VlrVenda,
    Guid? IdMarca,
    Guid? IdFornecedor,
    IReadOnlyList<AplicacaoDto> Aplicacoes);

/// <summary>Produto completo para o formulário (visualização/edição).</summary>
public sealed record ProdutoDto(
    Guid Id,
    int CodProduto,
    Guid IdCategoria,
    string Descricao,
    string? DescricaoComplementar,
    string? CodBarras,
    string? CodFabricante,
    UnidadeMedida Unidade,
    PosicaoPeca Posicao,
    decimal VlrCusto,
    decimal VlrVenda,
    Guid? IdMarca,
    Guid? IdFornecedor,
    bool FlgAtivo,
    IReadOnlyList<AplicacaoDto> Aplicacoes);

/// <summary>Linha enxuta para a listagem de produtos (com nomes das FKs já resolvidos).</summary>
public sealed record ProdutoListaDto(
    Guid Id,
    int CodProduto,
    string Descricao,
    string? CodBarras,
    string? Categoria,
    string? Marca,
    UnidadeMedida Unidade,
    PosicaoPeca Posicao,
    decimal VlrVenda,
    bool FlgAtivo);

/// <summary>Opção genérica para preencher combos (Categoria/Marca/Fornecedor).</summary>
public sealed record OpcaoDto(Guid Id, string Descricao);

/// <summary>Filtros da busca de peças por veículo (tela Catálogo). Todos opcionais.</summary>
public sealed record BuscaCatalogoDto(
    string? Termo,
    string? Montadora,
    string? Modelo,
    int? Ano);

/// <summary>Linha do resultado do Catálogo: a peça + em quais veículos ela aplica.</summary>
public sealed record CatalogoItemDto(
    Guid Id,
    int CodProduto,
    string Descricao,
    string? Categoria,
    string? Marca,
    UnidadeMedida Unidade,
    decimal VlrVenda,
    string Aplicacoes);
