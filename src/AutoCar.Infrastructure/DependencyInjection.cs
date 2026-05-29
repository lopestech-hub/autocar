using AutoCar.Domain.Interfaces;
using AutoCar.Infrastructure.Persistence;
using AutoCar.Infrastructure.Persistence.Repositories;
using AutoCar.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Infrastructure;

/// <summary>Registro dos serviços da camada de infraestrutura no contêiner de DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<IHashSenha, HashSenhaBCrypt>();
        services.AddScoped<DbInitializer>();

        return services;
    }
}
