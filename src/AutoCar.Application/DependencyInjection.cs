using AutoCar.Application.Modules.Security;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Application;

/// <summary>Registro dos serviços da camada de aplicação no contêiner de DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();

        return services;
    }
}
