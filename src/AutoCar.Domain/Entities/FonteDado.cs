using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Fonte do dado de catálogo: de qual catálogo e por qual método uma peça de referência foi extraída
/// (COFAP, MOBENSANI, PERFECT, SAMPEL, SYL, HIPPER FREIOS...). Cadastro mestre auxiliar: id + cod_fonte.
/// NÃO confundir com <see cref="Marca"/> — marca é o fabricante da peça; fonte é a origem/procedência do
/// dado. Referenciada por <see cref="Produto"/> via FK opcional, para rastreabilidade de origem e para a
/// futura busca por IA no AutoCar Expert. Populada pelo carregador do automacao_catalogo.
/// </summary>
public class FonteDado : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected FonteDado() { }

    public FonteDado(string descricao, string sistema, string? observacao = null)
    {
        Descricao = descricao.Trim().ToUpperInvariant();
        Sistema = sistema.Trim().ToUpperInvariant();
        Observacao = Normalizar(observacao);
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodFonte { get; protected set; }

    /// <summary>Nome da fonte/catálogo (ex: "COFAP", "HIPPER FREIOS").</summary>
    public string Descricao { get; protected set; } = string.Empty;

    /// <summary>Motor/método da extração (ex: "CATALOGO EXPRESSO (IDEIA2001)", "PROPRIO").</summary>
    public string Sistema { get; protected set; } = string.Empty;

    /// <summary>Notas livres de extração (opcional).</summary>
    public string? Observacao { get; protected set; }

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(string descricao, string sistema, string? observacao = null)
    {
        Descricao = descricao.Trim().ToUpperInvariant();
        Sistema = sistema.Trim().ToUpperInvariant();
        Observacao = Normalizar(observacao);
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

    // Observação livre: só remove espaços; nula quando vazia. Não força CAIXA ALTA (é nota livre).
    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
