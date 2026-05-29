using AutoCar.Shared.Results;

namespace AutoCar.Domain.Interfaces;

/// <summary>
/// Consulta dados públicos de um CNPJ em fonte externa (ex: Receita via BrasilAPI).
/// A implementação concreta mora na Infrastructure.
/// </summary>
public interface IConsultaCnpj
{
    /// <summary>
    /// Busca os dados cadastrais de um CNPJ (somente dígitos). Retorna falha quando
    /// o CNPJ não é encontrado ou a consulta indisponível (sem internet, fora do ar).
    /// </summary>
    Task<Result<DadosCnpj>> ConsultarAsync(string cnpj, CancellationToken ct = default);
}

/// <summary>Dados retornados da consulta de CNPJ. Campos ausentes vêm como null.</summary>
public sealed record DadosCnpj(
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
    string? Uf);
