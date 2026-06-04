namespace AutoCar.Application.Modules.Registrations.Mecanicos.DTOs;

/// <summary>Dados de entrada para criar/atualizar um mecânico (vindo da tela).</summary>
public sealed record SalvarMecanicoDto(string Nome, string? Telefone);

/// <summary>Mecânico completo para o formulário, a listagem e o seletor da OS.</summary>
public sealed record MecanicoDto(Guid Id, int CodMecanico, string Nome, string? Telefone, bool FlgAtivo);
