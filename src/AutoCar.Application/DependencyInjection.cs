using AutoCar.Application.Modules.Registrations.Clientes;
using AutoCar.Application.Modules.Registrations.Clientes.DTOs;
using AutoCar.Application.Modules.Registrations.Clientes.Validators;
using AutoCar.Application.Modules.Registrations.Fornecedores;
using AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;
using AutoCar.Application.Modules.Registrations.Fornecedores.Validators;
using AutoCar.Application.Modules.Registrations.Produtos;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Application.Modules.Registrations.Produtos.Validators;
using AutoCar.Application.Modules.Registrations.Servicos;
using AutoCar.Application.Modules.Registrations.Servicos.DTOs;
using AutoCar.Application.Modules.Registrations.Servicos.Validators;
using AutoCar.Application.Modules.Sales.PreVendas;
using AutoCar.Application.Modules.Sales.PreVendas.DTOs;
using AutoCar.Application.Modules.Sales.PreVendas.Validators;
using AutoCar.Application.Modules.Service.OrdensServico;
using AutoCar.Application.Modules.Service.OrdensServico.DTOs;
using AutoCar.Application.Modules.Service.OrdensServico.Validators;
using AutoCar.Application.Modules.Sales.Devolucoes;
using AutoCar.Application.Modules.Sales.Devolucoes.DTOs;
using AutoCar.Application.Modules.Sales.Devolucoes.Validators;
using AutoCar.Application.Modules.Purchases.Compras;
using AutoCar.Application.Modules.Purchases.Compras.DTOs;
using AutoCar.Application.Modules.Purchases.Compras.Validators;
using AutoCar.Application.Modules.Estoque;
using AutoCar.Application.Modules.Estoque.DTOs;
using AutoCar.Application.Modules.Estoque.Validators;
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

        services.AddScoped<IFornecedorService, FornecedorService>();
        services.AddScoped<IValidator<SalvarFornecedorDto>, SalvarFornecedorValidator>();

        services.AddScoped<IMarcaService, MarcaService>();
        services.AddScoped<IValidator<SalvarMarcaDto>, SalvarMarcaValidator>();

        services.AddScoped<ICategoriaProdutoService, CategoriaProdutoService>();
        services.AddScoped<IValidator<SalvarCategoriaProdutoDto>, SalvarCategoriaProdutoValidator>();

        services.AddScoped<IProdutoService, ProdutoService>();
        services.AddScoped<IValidator<SalvarProdutoDto>, SalvarProdutoValidator>();

        services.AddScoped<IServicoService, ServicoService>();
        services.AddScoped<IValidator<SalvarServicoDto>, SalvarServicoValidator>();

        // Vendas
        services.AddScoped<IPreVendaService, PreVendaService>();
        services.AddScoped<IValidator<SalvarPreVendaDto>, SalvarPreVendaValidator>();

        services.AddScoped<IDevolucaoService, DevolucaoService>();
        services.AddScoped<IValidator<CriarDevolucaoDto>, CriarDevolucaoValidator>();

        services.AddScoped<ICompraService, CompraService>();
        services.AddScoped<IValidator<CriarCompraDto>, CriarCompraValidator>();

        // Ordem de Serviço (oficina)
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();
        services.AddScoped<IValidator<SalvarOrdemServicoDto>, SalvarOrdemServicoValidator>();

        // Estoque
        services.AddScoped<IEstoqueService, EstoqueService>();
        services.AddScoped<IValidator<MovimentarEstoqueDto>, MovimentarEstoqueValidator>();

        return services;
    }
}
