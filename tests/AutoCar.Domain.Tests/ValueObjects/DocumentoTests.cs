using AutoCar.Domain.Enums;
using AutoCar.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AutoCar.Domain.Tests.ValueObjects;

public class DocumentoTests
{
    [Theory]
    [InlineData("111.444.777-35")]   // CPF válido com máscara
    [InlineData("11144477735")]       // mesmo CPF só dígitos
    public void Criar_CpfValido_RetornaDocumento(string entrada)
    {
        var doc = Documento.Criar(entrada, TipoPessoa.Fisica);

        doc.Should().NotBeNull();
        doc!.Numero.Should().Be("11144477735");
        doc.Tipo.Should().Be(TipoPessoa.Fisica);
    }

    [Theory]
    [InlineData("123.456.789-00")]   // dígito verificador errado
    [InlineData("111.111.111-11")]   // todos iguais
    [InlineData("123")]               // curto demais
    [InlineData("")]                  // vazio
    [InlineData(null)]                // nulo
    public void Criar_CpfInvalido_RetornaNull(string? entrada)
    {
        Documento.Criar(entrada, TipoPessoa.Fisica).Should().BeNull();
    }

    [Theory]
    [InlineData("11.222.333/0001-81")] // CNPJ válido com máscara
    [InlineData("11222333000181")]      // mesmo CNPJ só dígitos
    public void Criar_CnpjValido_RetornaDocumento(string entrada)
    {
        var doc = Documento.Criar(entrada, TipoPessoa.Juridica);

        doc.Should().NotBeNull();
        doc!.Numero.Should().Be("11222333000181");
        doc.Tipo.Should().Be(TipoPessoa.Juridica);
    }

    [Theory]
    [InlineData("11.222.333/0001-00")] // dígito verificador errado
    [InlineData("00.000.000/0000-00")] // todos iguais
    [InlineData("11222333")]            // curto demais
    public void Criar_CnpjInvalido_RetornaNull(string entrada)
    {
        Documento.Criar(entrada, TipoPessoa.Juridica).Should().BeNull();
    }

    [Fact]
    public void Criar_CpfValidoMasTipoJuridica_RetornaNull()
    {
        // 11 dígitos não passam como CNPJ (espera 14).
        Documento.Criar("11144477735", TipoPessoa.Juridica).Should().BeNull();
    }
}
