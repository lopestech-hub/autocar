using AutoCar.Domain.Common;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Mecânico da oficina — quem executa o serviço numa Ordem de Serviço. É um cadastro mestre próprio
/// (id + cod_mecanico), <b>não</b> um usuário do sistema: o mecânico não loga, não tem perfil nem
/// senha. Serve para identificar o responsável pelo trabalho na OS (base de produtividade/comissão
/// futura). Referenciado por OrdemServico via FK.
/// </summary>
public class Mecanico : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected Mecanico() { }

    public Mecanico(string nome, string? telefone)
    {
        AlterarDados(nome, telefone);
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodMecanico { get; protected set; }

    public string Nome { get; protected set; } = string.Empty;

    /// <summary>Telefone de contato (opcional, texto livre).</summary>
    public string? Telefone { get; protected set; }

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(string nome, string? telefone)
    {
        Nome = nome.Trim().ToUpperInvariant();
        Telefone = string.IsNullOrWhiteSpace(telefone) ? null : telefone.Trim();
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
