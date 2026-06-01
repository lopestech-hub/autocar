using System;
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

        // Factory para repositórios que precisam de um DbContext novo por operação (evita xmin
        // defasado do contexto de longa duração). Usado pelo ProdutoRepository; os demais repos
        // ainda usam o AppDbContext injetado. Veja IProdutoRepository.AtualizarAsync.
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(connectionString), lifetime: ServiceLifetime.Scoped);

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IFornecedorRepository, FornecedorRepository>();
        services.AddScoped<IMarcaRepository, MarcaRepository>();
        services.AddScoped<ICategoriaProdutoRepository, CategoriaProdutoRepository>();
        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IPreVendaRepository, PreVendaRepository>();
        services.AddSingleton<IHashSenha, HashSenhaBCrypt>();
        services.AddScoped<DbInitializer>();

        // Consulta de CNPJ via BrasilAPI (HttpClient tipado, timeout curto).
        services.AddHttpClient<IConsultaCnpj, ConsultaCnpjBrasilApi>(http =>
        {
            http.BaseAddress = new Uri("https://brasilapi.com.br/");
            http.Timeout = TimeSpan.FromSeconds(15);
            // BrasilAPI (atrás de CDN) rejeita requests sem User-Agent.
            http.DefaultRequestHeaders.Add("User-Agent", "AutoCar-ERP/1.0");
        });

        return services;
    }
}
