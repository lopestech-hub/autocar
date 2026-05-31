using System;
using AutoCar.Desktop.ViewModels;
using AutoCar.Desktop.ViewModels.Catalogo;
using AutoCar.Desktop.ViewModels.Registrations;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Desktop.Navegacao;

/// <summary>
/// Mapa rota → ViewModel. Resolve cada tela via DI (sempre uma instância nova, para
/// a tela abrir limpa). Rota desconhecida/nula retorna null (shell mostra placeholder).
/// </summary>
public sealed class Navegador : INavegador
{
    private readonly IServiceProvider _sp;

    public Navegador(IServiceProvider sp) => _sp = sp;

    public ViewModelBase? Resolver(string? rota) => rota switch
    {
        "clientes" => _sp.GetRequiredService<ClientesViewModel>(),
        "fornecedor" => _sp.GetRequiredService<FornecedoresViewModel>(),
        "produtos" => _sp.GetRequiredService<ProdutosViewModel>(),
        "catalogo" => _sp.GetRequiredService<CatalogoViewModel>(),
        "marcas" => _sp.GetRequiredService<MarcasViewModel>(),
        "categorias" => _sp.GetRequiredService<CategoriasViewModel>(),
        _ => null,
    };
}
