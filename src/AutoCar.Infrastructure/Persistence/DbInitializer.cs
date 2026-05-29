using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AutoCar.Infrastructure.Persistence;

/// <summary>
/// Aplica migrations pendentes e garante o seed inicial de forma idempotente.
/// Roda no startup da aplicação. Cria um usuário admin padrão na primeira execução
/// para permitir o primeiro login (senha provisória — trocar depois).
/// </summary>
public class DbInitializer
{
    private readonly AppDbContext _db;
    private readonly IHashSenha _hash;
    private readonly ILogger<DbInitializer> _logger;

    // Credenciais do usuário administrador padrão. Documentadas no CLAUDE.md.
    private const string AdminLogin = "julio";
    private const string AdminNome = "Julio";
    private const string AdminSenhaProvisoria = "123";

    public DbInitializer(AppDbContext db, IHashSenha hash, ILogger<DbInitializer> logger)
    {
        _db = db;
        _hash = hash;
        _logger = logger;
    }

    /// <summary>
    /// Aplica migrations e garante o seed. Síncrono de propósito: é chamado no
    /// Program.Main, ANTES de iniciar o lifetime do Avalonia, fora do contexto STA
    /// de UI — evita o deadlock de bloquear o thread de UI esperando trabalho async.
    /// </summary>
    public void Inicializar()
    {
        _db.Database.Migrate();
        GarantirAdmin();
    }

    private void GarantirAdmin()
    {
        var jaExiste = _db.Usuarios.Any(u => u.Perfil == PerfilUsuario.Admin);
        if (jaExiste)
            return;

        var admin = new Usuario(
            nome: AdminNome,
            login: AdminLogin,
            senhaHash: _hash.Gerar(AdminSenhaProvisoria),
            perfil: PerfilUsuario.Admin);

        _db.Usuarios.Add(admin);
        _db.SaveChanges();

        _logger.LogInformation(
            "Usuário admin padrão criado (login: {Login}).", AdminLogin);
    }
}
