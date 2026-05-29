using AutoCar.Domain.Enums;

namespace AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;

/// <summary>Dados de entrada para criar/atualizar um fornecedor (vindo da tela).</summary>
public sealed record SalvarFornecedorDto(
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
    string? InscricaoEstadual,
    string? Contato,
    string? Observacao);

/// <summary>Fornecedor completo para o formulário (visualização/edição).</summary>
public sealed record FornecedorDto(
    Guid Id,
    int CodFornecedor,
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
    string? InscricaoEstadual,
    string? Contato,
    string? Observacao,
    bool FlgAtivo);

/// <summary>Linha enxuta para a listagem de fornecedores.</summary>
public sealed record FornecedorListaDto(
    Guid Id,
    int CodFornecedor,
    TipoPessoa TipoPessoa,
    string Documento,
    string RazaoSocial,
    string? Telefone,
    bool FlgAtivo);
