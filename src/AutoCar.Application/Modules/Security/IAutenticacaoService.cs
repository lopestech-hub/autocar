using AutoCar.Application.Modules.Security.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Security;

/// <summary>Serviço de autenticação de usuários.</summary>
public interface IAutenticacaoService
{
    /// <summary>
    /// Valida nome de usuário e senha. Retorna o usuário logado em caso de
    /// sucesso, ou um erro de não-autorizado quando credenciais são inválidas
    /// ou o usuário está inativo.
    /// </summary>
    Task<Result<UsuarioLogado>> AutenticarAsync(string login, string senha, CancellationToken ct = default);
}
