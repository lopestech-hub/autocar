using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoCar.Infrastructure.Persistence;

/// <summary>
/// Factory usada APENAS em tempo de design pelo "dotnet ef" para gerar e aplicar
/// migrations sem subir a aplicação. A string de conexão de runtime real vem do
/// appsettings.json do Desktop — esta aqui aponta para o banco de desenvolvimento.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("AUTOCAR_DB")
            ?? "Host=localhost;Port=5432;Database=autocar;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
