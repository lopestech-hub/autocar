namespace AutoCar.Application.Modules.Registrations.Produtos.DTOs;

/// <summary>Dados de entrada para criar/atualizar uma marca (vindo da tela).</summary>
public sealed record SalvarMarcaDto(string Descricao);

/// <summary>Marca completa para o formulário e a listagem.</summary>
public sealed record MarcaDto(Guid Id, int CodMarca, string Descricao, bool FlgAtivo);
