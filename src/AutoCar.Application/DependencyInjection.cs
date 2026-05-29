using AutoCar.Application.Modules.Registrations.Clientes;
using AutoCar.Application.Modules.Registrations.Clientes.DTOs;
using AutoCar.Application.Modules.Registrations.Clientes.Validators;
using AutoCar.Application.Modules.Security;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Application;

/// <summary>Registro dos serviços da camada de aplicação no contêiner de DI.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAutenticacaoService, AutenticacaoService>();

        // Cadastros
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IValidator<SalvarClienteDto>, SalvarClienteValidator>();

        return services;
    }
}
