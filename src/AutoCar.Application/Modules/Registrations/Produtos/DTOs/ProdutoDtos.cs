using AutoCar.Domain.Enums;

namespace AutoCar.Application.Modules.Registrations.Produtos.DTOs;

/// <summary>Dados de entrada para criar/atualizar um produto (vindo da tela).</summary>
public sealed record SalvarProdutoDto(
    Guid IdCategoria,
    string Descricao,
    string? DescricaoComplementar,
    string? CodBarras,
    string? CodFabricante,
    UnidadeMedida Unidade,
    decimal VlrCusto,
    decimal VlrVenda,
    Guid? IdMarca,
    Guid? IdFornecedor);

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
    decimal VlrCusto,
    decimal VlrVenda,
    Guid? IdMarca,
    Guid? IdFornecedor,
    bool FlgAtivo);

/// <summary>Linha enxuta para a listagem de produtos (com nomes das FKs já resolvidos).</summary>
public sealed record ProdutoListaDto(
    Guid Id,
    int CodProduto,
    string Descricao,
    string? CodBarras,
    string? Categoria,
    string? Marca,
    UnidadeMedida Unidade,
    decimal VlrVenda,
    bool FlgAtivo);

/// <summary>Opção genérica para preencher combos (Categoria/Marca/Fornecedor).</summary>
public sealed record OpcaoDto(Guid Id, string Descricao);
