namespace AutoCar.Application.Modules.Registrations.Produtos.DTOs;

/// <summary>Dados de entrada para criar/atualizar um grupo (vindo da tela). Grupo pertence a uma categoria.</summary>
public sealed record SalvarGrupoProdutoDto(string Descricao, Guid IdCategoria);

/// <summary>Grupo completo para o formulário e a listagem (com o nome da categoria já resolvido).</summary>
public sealed record GrupoProdutoDto(Guid Id, int CodGrupo, string Descricao, Guid IdCategoria, string? Categoria, bool FlgAtivo);
