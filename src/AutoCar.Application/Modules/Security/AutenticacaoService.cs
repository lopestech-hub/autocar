using AutoCar.Application.Modules.Security.DTOs;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Security;

/// <summary>
/// Autenticação por nome de usuário + senha com verificação de hash BCrypt.
/// Mensagem de erro genérica (não revela se o usuário existe) por segurança.
/// </summary>
public sealed class AutenticacaoService : IAutenticacaoService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IHashSenha _hash;

    public AutenticacaoService(IUsuarioRepository usuarios, IHashSenha hash)
    {
        _usuarios = usuarios;
        _hash = hash;
    }

    public async Task<Result<UsuarioLogado>> AutenticarAsync(
        string login, string senha, CancellationToken ct = default)
    {
        var credenciaisInvalidas = Error.NaoAutorizado("Usuário ou senha inválidos.");

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            return Result.Falhar<UsuarioLogado>(credenciaisInvalidas);

        var usuario = await _usuarios.ObterPorLoginAsync(login.Trim().ToLowerInvariant(), ct);
        if (usuario is null || !usuario.FlgAtivo)
            return Result.Falhar<UsuarioLogado>(credenciaisInvalidas);

        if (!_hash.Verificar(senha, usuario.SenhaHash))
            return Result.Falhar<UsuarioLogado>(credenciaisInvalidas);

        var logado = new UsuarioLogado(
            usuario.Id, usuario.CodUsuario, usuario.Nome, usuario.Login, usuario.Perfil);

        return Result.Ok(logado);
    }
}
