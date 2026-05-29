using AutoCar.Domain.Enums;

namespace AutoCar.Application.Modules.Security.DTOs;

/// <summary>
/// Dados do usuário autenticado mantidos na sessão. Nunca carrega a senha/hash.
/// </summary>
public sealed record UsuarioLogado(Guid Id, int CodUsuario, string Nome, string Login, PerfilUsuario Perfil);
