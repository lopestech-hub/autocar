using System.Text.Json;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using Microsoft.Extensions.Logging;

namespace AutoCar.Infrastructure.Services;

/// <summary>
/// Consulta CNPJ na BrasilAPI (https://brasilapi.com.br/api/cnpj/v1/{cnpj}).
/// Pública e gratuita, sem chave. Exige internet no momento da consulta.
/// </summary>
public sealed class ConsultaCnpjBrasilApi : IConsultaCnpj
{
    private readonly HttpClient _http;
    private readonly ILogger<ConsultaCnpjBrasilApi> _logger;

    public ConsultaCnpjBrasilApi(HttpClient http, ILogger<ConsultaCnpjBrasilApi> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<Result<DadosCnpj>> ConsultarAsync(string cnpj, CancellationToken ct = default)
    {
        var digitos = new string((cnpj ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digitos.Length != 14)
            return Result.Falhar<DadosCnpj>(Error.Validacao("CNPJ inválido para consulta."));

        try
        {
            var resposta = await _http.GetAsync($"api/cnpj/v1/{digitos}", ct);

            if (resposta.StatusCode == System.Net.HttpStatusCode.NotFound)
                return Result.Falhar<DadosCnpj>(Error.NaoEncontrado("CNPJ não encontrado na Receita."));

            if (!resposta.IsSuccessStatusCode)
                return Result.Falhar<DadosCnpj>(Error.Conflito("Consulta de CNPJ indisponível no momento."));

            // A BrasilAPI mistura tipos (alguns campos vêm como número, outros como string).
            // Lemos via JsonDocument com tolerância a tipo, em vez de mapear para tipos rígidos.
            var json = await resposta.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            var raiz = doc.RootElement;

            var razao = Texto(raiz, "razao_social");
            if (string.IsNullOrWhiteSpace(razao))
                return Result.Falhar<DadosCnpj>(Error.NaoEncontrado("CNPJ sem dados cadastrais."));

            var dados = new DadosCnpj(
                RazaoSocial: razao!,
                NomeFantasia: Texto(raiz, "nome_fantasia"),
                Telefone: Texto(raiz, "ddd_telefone_1"),
                Email: Texto(raiz, "email"),
                Cep: Texto(raiz, "cep"),
                Logradouro: Texto(raiz, "logradouro"),
                Numero: Texto(raiz, "numero"),
                Complemento: Texto(raiz, "complemento"),
                Bairro: Texto(raiz, "bairro"),
                Cidade: Texto(raiz, "municipio"),
                Uf: Texto(raiz, "uf"));

            return Result.Ok(dados);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Resposta de CNPJ em formato inesperado.");
            return Result.Falhar<DadosCnpj>(Error.Conflito("Resposta de CNPJ em formato inesperado."));
        }
        catch (TaskCanceledException)
        {
            return Result.Falhar<DadosCnpj>(Error.Conflito("A consulta de CNPJ demorou demais. Tente novamente."));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Falha de rede ao consultar CNPJ {Cnpj}.", digitos);
            return Result.Falhar<DadosCnpj>(Error.Conflito("Sem conexão para consultar o CNPJ."));
        }
    }

    // Lê um campo como texto, tolerante a tipo (string ou número), retornando null se vazio.
    private static string? Texto(JsonElement raiz, string campo)
    {
        if (!raiz.TryGetProperty(campo, out var el))
            return null;

        var valor = el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.ToString(),
            _ => null,
        };

        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }
}
