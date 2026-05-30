using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;
using AutoCar.Domain.ValueObjects;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Cliente do AutoCar (pessoa física ou jurídica). Tabela mestre: id + cod_cliente.
/// Só existe com um <see cref="Documento"/> válido (invariante garantida no construtor).
/// Endereço fica na própria entidade (um endereço por cliente no MVP).
/// </summary>
public class Cliente : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected Cliente() { }

    public Cliente(
        Documento documento,
        string razaoSocial,
        string? nomeFantasia,
        string? telefone,
        string? email,
        Endereco endereco,
        string? observacao,
        decimal limiteCredito)
    {
        TipoPessoa = documento.Tipo;
        Documento = documento.Numero;
        RazaoSocial = razaoSocial.Trim().ToUpperInvariant();
        NomeFantasia = string.IsNullOrWhiteSpace(nomeFantasia) ? null : nomeFantasia.Trim().ToUpperInvariant();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Endereco = endereco;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        LimiteCredito = limiteCredito;
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodCliente { get; protected set; }

    public TipoPessoa TipoPessoa { get; protected set; }

    /// <summary>Documento (CPF ou CNPJ), apenas dígitos. Único.</summary>
    public string Documento { get; protected set; } = string.Empty;

    /// <summary>Nome (PF) ou razão social (PJ).</summary>
    public string RazaoSocial { get; protected set; } = string.Empty;

    /// <summary>Nome fantasia — só faz sentido para PJ.</summary>
    public string? NomeFantasia { get; protected set; }

    public string? Telefone { get; protected set; }

    public string? Email { get; protected set; }

    public Endereco Endereco { get; protected set; } = Endereco.Vazio();

    public string? Observacao { get; protected set; }

    public decimal LimiteCredito { get; protected set; }

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(
        Documento documento,
        string razaoSocial,
        string? nomeFantasia,
        string? telefone,
        string? email,
        Endereco endereco,
        string? observacao,
        decimal limiteCredito)
    {
        TipoPessoa = documento.Tipo;
        Documento = documento.Numero;
        RazaoSocial = razaoSocial.Trim().ToUpperInvariant();
        NomeFantasia = string.IsNullOrWhiteSpace(nomeFantasia) ? null : nomeFantasia.Trim().ToUpperInvariant();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Endereco = endereco;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        LimiteCredito = limiteCredito;
        MarcarAtualizada();
    }

    public void Ativar()
    {
        FlgAtivo = true;
        MarcarAtualizada();
    }

    public void Inativar()
    {
        FlgAtivo = false;
        MarcarAtualizada();
    }
}
