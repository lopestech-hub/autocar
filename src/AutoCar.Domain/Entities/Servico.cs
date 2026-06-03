using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Serviço / mão de obra do catálogo da oficina (alinhamento, troca de óleo, revisão...).
/// Cadastro mestre auxiliar: id + cod_servico. Referenciado por uma linha de Ordem de Serviço
/// do tipo Serviço, que copia descrição e <see cref="VlrPadrao"/> como snapshot editável.
/// </summary>
public class Servico : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected Servico() { }

    public Servico(string descricao, decimal vlrPadrao)
    {
        AlterarDados(descricao, vlrPadrao);
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodServico { get; protected set; }

    public string Descricao { get; protected set; } = string.Empty;

    /// <summary>Valor sugerido da mão de obra (R$). Editável na Ordem de Serviço. Não-negativo.</summary>
    public decimal VlrPadrao { get; protected set; }

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(string descricao, decimal vlrPadrao)
    {
        if (vlrPadrao < 0)
            throw new ArgumentException("O valor padrão do serviço não pode ser negativo.", nameof(vlrPadrao));

        Descricao = descricao.Trim().ToUpperInvariant();
        VlrPadrao = vlrPadrao;
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
