using System.Net.Http;
using AutoCar.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AutoCar.Infrastructure.Tests.Services;

/// <summary>
/// Teste de integração real contra a BrasilAPI. Requer internet.
/// Marcado com Trait para poder ser pulado em CI offline.
/// </summary>
[Trait("Categoria", "Integracao")]
public class ConsultaCnpjBrasilApiTests
{
    private static ConsultaCnpjBrasilApi Criar()
    {
        var http = new HttpClient { BaseAddress = new System.Uri("https://brasilapi.com.br/") };
        http.DefaultRequestHeaders.Add("User-Agent", "AutoCar-ERP/1.0");
        return new ConsultaCnpjBrasilApi(http, NullLogger<ConsultaCnpjBrasilApi>.Instance);
    }

    [Fact]
    public async Task ConsultarAsync_CnpjValido_RetornaRazaoSocialEEndereco()
    {
        var servico = Criar();

        var resultado = await servico.ConsultarAsync("47960950000121"); // Magazine Luiza

        resultado.Sucesso.Should().BeTrue(
            because: resultado.Falha ? $"erro retornado: {resultado.Erro.Codigo} / {resultado.Erro.Mensagem}" : "");
        resultado.Valor.RazaoSocial.Should().Contain("MAGAZINE LUIZA");
        resultado.Valor.Uf.Should().Be("SP");
        resultado.Valor.Cidade.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ConsultarAsync_CnpjInexistente_RetornaFalha()
    {
        var servico = Criar();

        var resultado = await servico.ConsultarAsync("00000000000000");

        resultado.Falha.Should().BeTrue();
    }

    [Fact]
    public async Task ConsultarAsync_FormatoInvalido_RetornaFalhaSemChamarApi()
    {
        var servico = Criar();

        var resultado = await servico.ConsultarAsync("123");

        resultado.Falha.Should().BeTrue();
    }
}
