using System.Linq;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.ValueObjects;

/// <summary>
/// Documento de identificação (CPF para pessoa física, CNPJ para jurídica).
/// Armazena apenas dígitos, sem máscara. Garante o dígito verificador válido —
/// uma entidade só existe com um Documento válido (invariante de domínio).
/// Reutilizável por Cliente e Fornecedor.
/// </summary>
public sealed record Documento
{
    private Documento(string numero, TipoPessoa tipo)
    {
        Numero = numero;
        Tipo = tipo;
    }

    /// <summary>Somente dígitos (11 para CPF, 14 para CNPJ).</summary>
    public string Numero { get; }

    public TipoPessoa Tipo { get; }

    /// <summary>
    /// Cria um Documento validando o dígito verificador conforme o tipo.
    /// Retorna null quando inválido (o chamador decide como reportar).
    /// </summary>
    public static Documento? Criar(string? entrada, TipoPessoa tipo)
    {
        var digitos = SomenteDigitos(entrada);

        var valido = tipo switch
        {
            TipoPessoa.Fisica => CpfValido(digitos),
            TipoPessoa.Juridica => CnpjValido(digitos),
            _ => false,
        };

        return valido ? new Documento(digitos, tipo) : null;
    }

    public static string SomenteDigitos(string? entrada) =>
        new(string.IsNullOrEmpty(entrada) ? [] : entrada.Where(char.IsDigit).ToArray());

    private static bool CpfValido(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
            return false;

        var digitos = cpf.Select(c => c - '0').ToArray();

        var dv1 = CalcularDigito(digitos, 9, 10);
        var dv2 = CalcularDigito(digitos, 10, 11);

        return digitos[9] == dv1 && digitos[10] == dv2;
    }

    private static bool CnpjValido(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
            return false;

        var digitos = cnpj.Select(c => c - '0').ToArray();
        int[] pesos1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesos2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var dv1 = CalcularDigitoCnpj(digitos, pesos1);
        var dv2 = CalcularDigitoCnpj(digitos, pesos2);

        return digitos[12] == dv1 && digitos[13] == dv2;
    }

    // CPF: soma ponderada decrescente (pesoInicial..2), resto < 2 => 0.
    private static int CalcularDigito(int[] digitos, int quantidade, int pesoInicial)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += digitos[i] * (pesoInicial - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static int CalcularDigitoCnpj(int[] digitos, int[] pesos)
    {
        var soma = 0;
        for (var i = 0; i < pesos.Length; i++)
            soma += digitos[i] * pesos[i];

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
