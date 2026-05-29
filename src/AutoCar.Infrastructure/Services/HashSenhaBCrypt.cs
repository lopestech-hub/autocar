using AutoCar.Domain.Interfaces;

namespace AutoCar.Infrastructure.Services;

/// <summary>Implementação de hashing de senha usando BCrypt.</summary>
public class HashSenhaBCrypt : IHashSenha
{
    public string Gerar(string senhaTextoClaro) =>
        BCrypt.Net.BCrypt.HashPassword(senhaTextoClaro);

    public bool Verificar(string senhaTextoClaro, string hash) =>
        BCrypt.Net.BCrypt.Verify(senhaTextoClaro, hash);
}
