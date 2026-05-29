using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Usuário do sistema. Tabela mestre: id (UUID) + cod_usuario (INT autoincrement).
/// Login por nome de usuário (campo Login) + senha (hash BCrypt, nunca em texto claro).
/// </summary>
public class Usuario : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected Usuario() { }

    public Usuario(string nome, string login, string senhaHash, PerfilUsuario perfil)
    {
        Nome = nome.Trim();
        Login = NormalizarLogin(login);
        SenhaHash = senhaHash;
        Perfil = perfil;
        FlgAtivo = true;
    }

    /// <summary>Login sempre em minúsculas e sem espaços — chave natural de autenticação.</summary>
    private static string NormalizarLogin(string login) => login.Trim().ToLowerInvariant();

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodUsuario { get; protected set; }

    /// <summary>Nome de exibição do usuário (ex: "Julio Lopes").</summary>
    public string Nome { get; protected set; } = string.Empty;

    /// <summary>Nome de usuário usado para autenticar (ex: "julio").</summary>
    public string Login { get; protected set; } = string.Empty;

    /// <summary>Hash BCrypt da senha. Nunca expor em logs ou telas.</summary>
    public string SenhaHash { get; protected set; } = string.Empty;

    public PerfilUsuario Perfil { get; protected set; }

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(string nome, string login, PerfilUsuario perfil)
    {
        Nome = nome.Trim();
        Login = NormalizarLogin(login);
        Perfil = perfil;
        MarcarAtualizada();
    }

    public void DefinirSenha(string senhaHash)
    {
        SenhaHash = senhaHash;
        MarcarAtualizada();
    }

    public void Ativar()
    {
        FlgAtivo = true;
        MarcarAtualizada();
    }

    public void Desativar()
    {
        FlgAtivo = false;
        MarcarAtualizada();
    }
}
