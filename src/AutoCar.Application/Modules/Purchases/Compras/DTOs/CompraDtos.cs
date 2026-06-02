namespace AutoCar.Application.Modules.Purchases.Compras.DTOs;

/// <summary>Item a comprar (vindo da tela): produto + quantidade + custo unitário pago.</summary>
public sealed record CompraItemDto(Guid IdProduto, int Qtd, decimal VlrCustoUnitario);

/// <summary>Dados para registrar uma compra: o fornecedor, o nº do documento, observação e os itens.</summary>
public sealed record CriarCompraDto(
    Guid IdFornecedor,
    string? NumDocumento,
    string? Observacao,
    IReadOnlyList<CompraItemDto> Itens);

/// <summary>Compra criada (retorno), com o número do documento e o total.</summary>
public sealed record CompraDto(
    Guid Id,
    int CodCompra,
    Guid IdFornecedor,
    string? NumDocumento,
    decimal VlrTotal);

/// <summary>Compra na listagem: cabeçalho + contagem de itens (mais recentes primeiro).</summary>
public sealed record CompraListaDto(
    Guid Id,
    int CodCompra,
    string NomeFornecedor,
    int QtdItens,
    decimal VlrTotal,
    DateTime CriadoEm);
