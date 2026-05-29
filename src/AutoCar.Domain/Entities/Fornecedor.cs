using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;
using AutoCar.Domain.ValueObjects;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Fornecedor do AutoCar (pessoa física ou jurídica). Tabela mestre: id + cod_fornecedor.
/// Só existe com um <see cref="Documento"/> válido (invariante garantida no construtor).
/// Mesmo padrão de Cliente, com Inscrição Estadual e Contato (vendedor/representante)
/// no lugar do limite de crédito.
/// </summary>
public class Fornecedor : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected Fornecedor() { }

    public Fornecedor(
        Documento documento,
        string razaoSocial,
        string? nomeFantasia,
        string? telefone,
        string? email,
        Endereco endereco,
        string? inscricaoEstadual,
        string? contato,
        string? observacao)
    {
        TipoPessoa = documento.Tipo;
        Documento = documento.Numero;
        RazaoSocial = razaoSocial.Trim();
        NomeFantasia = string.IsNullOrWhiteSpace(nomeFantasia) ? null : nomeFantasia.Trim();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Endereco = endereco;
        InscricaoEstadual = string.IsNullOrWhiteSpace(inscricaoEstadual) ? null : inscricaoEstadual.Trim();
        Contato = string.IsNullOrWhiteSpace(contato) ? null : contato.Trim();
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodFornecedor { get; protected set; }

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

    /// <summary>Inscrição estadual (relevante para entrada de NF futura). Texto livre.</summary>
    public string? InscricaoEstadual { get; protected set; }

    /// <summary>Nome do contato/vendedor do fornecedor com quem a loja negocia.</summary>
    public string? Contato { get; protected set; }

    public string? Observacao { get; protected set; }

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(
        Documento documento,
        string razaoSocial,
        string? nomeFantasia,
        string? telefone,
        string? email,
        Endereco endereco,
        string? inscricaoEstadual,
        string? contato,
        string? observacao)
    {
        TipoPessoa = documento.Tipo;
        Documento = documento.Numero;
        RazaoSocial = razaoSocial.Trim();
        NomeFantasia = string.IsNullOrWhiteSpace(nomeFantasia) ? null : nomeFantasia.Trim();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Endereco = endereco;
        InscricaoEstadual = string.IsNullOrWhiteSpace(inscricaoEstadual) ? null : inscricaoEstadual.Trim();
        Contato = string.IsNullOrWhiteSpace(contato) ? null : contato.Trim();
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
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
