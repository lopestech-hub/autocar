namespace AutoCar.Application.Modules.Registrations.Produtos.DTOs;

/// <summary>Dados de entrada para criar/atualizar uma categoria (vindo da tela).</summary>
public sealed record SalvarCategoriaProdutoDto(string Descricao);

/// <summary>Categoria completa para o formulário e a listagem.</summary>
public sealed record CategoriaProdutoDto(Guid Id, int CodCategoria, string Descricao, bool FlgAtivo);
