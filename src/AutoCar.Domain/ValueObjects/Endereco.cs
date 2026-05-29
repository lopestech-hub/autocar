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
        Logradouro = Limpar(logradouro);
        Numero = Limpar(numero);
        Complemento = Limpar(complemento);
        Bairro = Limpar(bairro);
        Cidade = Limpar(cidade);
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
}
