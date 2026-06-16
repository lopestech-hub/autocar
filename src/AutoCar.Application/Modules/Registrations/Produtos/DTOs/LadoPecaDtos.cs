namespace AutoCar.Application.Modules.Registrations.Produtos.DTOs;

/// <summary>Dados de entrada para criar/atualizar um lado (vindo da tela).</summary>
public sealed record SalvarLadoPecaDto(string Descricao);

/// <summary>Lado completo para o formulário e a listagem.</summary>
public sealed record LadoPecaDto(Guid Id, int CodLado, string Descricao, bool FlgAtivo);
