namespace AutoCar.Domain.ValueObjects;

/// <summary>
/// Endereço de um cliente/fornecedor. Value Object — mapeado como owned type
/// nas colunas da própria tabela (um endereço por entidade no MVP). Todos os
/// campos são opcionais; <see cref="Vazio"/> representa endereço não informado.
/// </summary>
public sealed record Endereco
{
    public Endereco(
        string? cep,
        string? logradouro,
        string? numero,
        string? complemento,
        string? bairro,
        string? cidade,
        string? uf)
    {
        Cep = Limpar(cep);
        Logradouro = MaiusculaOuNulo(logradouro);
        Numero = Limpar(numero);
        Complemento = MaiusculaOuNulo(complemento);
        Bairro = MaiusculaOuNulo(bairro);
        Cidade = MaiusculaOuNulo(cidade);
        Uf = string.IsNullOrWhiteSpace(uf) ? null : uf.Trim().ToUpperInvariant();
    }

    public string? Cep { get; }
    public string? Logradouro { get; }
    public string? Numero { get; }
    public string? Complemento { get; }
    public string? Bairro { get; }
    public string? Cidade { get; }
    public string? Uf { get; }

    public static Endereco Vazio() => new(null, null, null, null, null, null, null);

    private static string? Limpar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    // Campos de endereço textual padronizados em CAIXA ALTA (padrão ERP).
    private static string? MaiusculaOuNulo(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim().ToUpperInvariant();
}
