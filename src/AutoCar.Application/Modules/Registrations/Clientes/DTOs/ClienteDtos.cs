using AutoCar.Domain.Enums;

namespace AutoCar.Application.Modules.Registrations.Clientes.DTOs;

/// <summary>Dados de entrada para criar/atualizar um cliente (vindo da tela).</summary>
public sealed record SalvarClienteDto(
    TipoPessoa TipoPessoa,
    string Documento,
    string RazaoSocial,
    string? NomeFantasia,
    string? Telefone,
    string? Email,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Observacao,
    decimal LimiteCredito);

/// <summary>Cliente completo para o formulário (visualização/edição).</summary>
public sealed record ClienteDto(
    Guid Id,
    int CodCliente,
    TipoPessoa TipoPessoa,
    string Documento,
    string RazaoSocial,
    string? NomeFantasia,
    string? Telefone,
    string? Email,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Uf,
    string? Observacao,
    decimal LimiteCredito,
    bool FlgAtivo);

/// <summary>Linha enxuta para a listagem de clientes.</summary>
public sealed record ClienteListaDto(
    Guid Id,
    int CodCliente,
    TipoPessoa TipoPessoa,
    string Documento,
    string RazaoSocial,
    string? Telefone,
    bool FlgAtivo);
