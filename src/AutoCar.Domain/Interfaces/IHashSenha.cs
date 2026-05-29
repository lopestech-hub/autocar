namespace AutoCar.Domain.Interfaces;

/// <summary>
/// Abstração de hashing de senha. A implementação concreta (BCrypt) mora
/// na Infrastructure — o domínio/aplicação não conhece a lib usada.
/// </summary>
public interface IHashSenha
{
    /// <summary>Gera o hash de uma senha em texto claro.</summary>
    string Gerar(string senhaTextoClaro);

    /// <summary>Verifica se a senha em texto claro corresponde ao hash.</summary>
    bool Verificar(string senhaTextoClaro, string hash);
}
