namespace AutoCar.Application.Modules.Registrations.Produtos.DTOs;

/// <summary>Dados de entrada para criar/atualizar uma posição (vindo da tela).</summary>
public sealed record SalvarPosicaoPecaDto(string Descricao);

/// <summary>Posição completa para o formulário e a listagem.</summary>
public sealed record PosicaoPecaDto(Guid Id, int CodPosicao, string Descricao, bool FlgAtivo);
