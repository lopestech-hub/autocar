using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Aplicação de um produto em um veículo (montadora/modelo/ano + motorização/combustível).
/// Tabela filho de <see cref="Produto"/> (1:N) — um produto serve em vários veículos. Texto livre
/// no MVP (sem cadastro normalizado de montadora/modelo). Sem `cod_` (registro filho, nunca
/// referenciado diretamente pelo usuário). Editada junto com o produto pai.
/// </summary>
public class ProdutoAplicacao : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected ProdutoAplicacao() { }

    public ProdutoAplicacao(
        string montadora,
        string modelo,
        int? anoInicio,
        int? anoFim,
        string? motorizacao,
        Combustivel combustivel,
        string? observacao)
    {
        Montadora = montadora.Trim().ToUpperInvariant();
        Modelo = modelo.Trim().ToUpperInvariant();
        AnoInicio = anoInicio;
        AnoFim = anoFim;
        Motorizacao = string.IsNullOrWhiteSpace(motorizacao) ? null : motorizacao.Trim().ToUpperInvariant();
        Combustivel = combustivel;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim().ToUpperInvariant();
    }

    /// <summary>FK para o produto pai (preenchida pelo EF via a coleção do Produto).</summary>
    public Guid IdProduto { get; protected set; }

    public string Montadora { get; protected set; } = string.Empty;

    public string Modelo { get; protected set; } = string.Empty;

    /// <summary>Ano inicial de aplicação (opcional).</summary>
    public int? AnoInicio { get; protected set; }

    /// <summary>Ano final de aplicação (opcional). Vazio = "em diante".</summary>
    public int? AnoFim { get; protected set; }

    /// <summary>Motorização do veículo (texto livre: "1.0", "1.6 FIRE", "2.0 16V"). Opcional.</summary>
    public string? Motorizacao { get; protected set; }

    /// <summary>Combustível do veículo. NaoAplica quando não importa para esta peça.</summary>
    public Combustivel Combustivel { get; protected set; }

    /// <summary>Detalhe livre (ex: "exceto ar condicionado", "câmbio manual").</summary>
    public string? Observacao { get; protected set; }
}
