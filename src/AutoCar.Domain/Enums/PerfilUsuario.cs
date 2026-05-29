namespace AutoCar.Domain.Enums;

/// <summary>
/// Perfis de acesso fixos do sistema. Define quais áreas o usuário
/// pode acessar. No MVP cada usuário tem exatamente um perfil.
/// </summary>
public enum PerfilUsuario
{
    /// <summary>Acesso total ao sistema, incluindo cadastro de usuários.</summary>
    Admin = 1,

    /// <summary>Balcão de vendas e consulta de catálogo/estoque.</summary>
    Vendedor = 2,

    /// <summary>Ordens de serviço da oficina.</summary>
    Mecanico = 3,

    /// <summary>Caixa, contas a pagar e a receber.</summary>
    Financeiro = 4,
}
