using System.Reflection;
using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence;

/// <summary>
/// Contexto EF Core do AutoCar. Aplica todas as configurações Fluent API
/// do assembly e mantém os timestamps (CriadoEm/AtualizadoEm) coerentes.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override int SaveChanges()
    {
        AtualizarTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AtualizarTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Mantém AtualizadoEm sempre em UTC ao alterar entidades (exibição converte p/ Brasília).</summary>
    private void AtualizarTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<EntidadeBase>())
        {
            if (entry.State == EntityState.Modified)
                entry.Property(nameof(EntidadeBase.AtualizadoEm)).CurrentValue = DataHora.AgoraUtc();
        }
    }
}
