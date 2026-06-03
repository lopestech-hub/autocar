namespace AutoCar.Application.Modules.Registrations.Servicos.DTOs;

/// <summary>Dados de entrada para criar/atualizar um serviço (vindo da tela).</summary>
public sealed record SalvarServicoDto(string Descricao, decimal VlrPadrao);

/// <summary>Serviço completo para o formulário, a listagem e o seletor da OS.</summary>
public sealed record ServicoDto(Guid Id, int CodServico, string Descricao, decimal VlrPadrao, bool FlgAtivo);
