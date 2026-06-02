namespace AutoCar.Application.Modules.Sales.Devolucoes.DTOs;

/// <summary>Item a devolver (vindo da tela): produto da venda + quantidade devolvida.</summary>
public sealed record DevolucaoItemDto(Guid IdProduto, int Qtd);

/// <summary>Dados para criar uma devolução: a venda de origem, o motivo e os itens devolvidos.</summary>
public sealed record CriarDevolucaoDto(
    Guid IdPreVenda,
    string? Motivo,
    IReadOnlyList<DevolucaoItemDto> Itens);

/// <summary>Item devolvível na tela de devolução: o que foi vendido e quanto ainda pode ser devolvido.</summary>
public sealed record ItemDevolvivelDto(
    Guid IdProduto,
    string DescricaoProduto,
    decimal VlrUnitario,
    int QtdVendida,
    int QtdJaDevolvida,
    int QtdDevolvivel);

/// <summary>Devolução criada (retorno), com o número do documento e o total.</summary>
public sealed record DevolucaoDto(
    Guid Id,
    int CodDevolucao,
    Guid IdPreVenda,
    string? Motivo,
    decimal VlrTotal);
