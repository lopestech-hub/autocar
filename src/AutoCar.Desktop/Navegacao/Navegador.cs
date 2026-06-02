using System;
using AutoCar.Application.Modules.Estoque;
using AutoCar.Application.Modules.Purchases.Compras;
using AutoCar.Application.Modules.Sales.PreVendas;
using AutoCar.Application.Modules.Security.DTOs;
using AutoCar.Desktop.ViewModels;
using AutoCar.Desktop.ViewModels.Catalogo;
using AutoCar.Desktop.ViewModels.Inventory;
using AutoCar.Desktop.ViewModels.Purchases;
using AutoCar.Desktop.ViewModels.Registrations;
using AutoCar.Desktop.ViewModels.Sales;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.Navegacao;

/// <summary>
/// Mapa rota → ViewModel. Resolve cada tela via DI (sempre uma instância nova, para
/// a tela abrir limpa). Rota desconhecida/nula retorna null (shell mostra placeholder).
/// </summary>
public sealed class Navegador : INavegador
{
    private readonly IServiceProvider _sp;
    private UsuarioLogado? _usuario;

    public Navegador(IServiceProvider sp) => _sp = sp;

    public void DefinirUsuario(UsuarioLogado usuario) => _usuario = usuario;

    public ViewModelBase? Resolver(string? rota) => rota switch
    {
        "clientes" => _sp.GetRequiredService<ClientesViewModel>(),
        "fornecedor" => _sp.GetRequiredService<FornecedoresViewModel>(),
        "produtos" => _sp.GetRequiredService<ProdutosViewModel>(),
        "catalogo" => _sp.GetRequiredService<CatalogoViewModel>(),
        "marcas" => _sp.GetRequiredService<MarcasViewModel>(),
        "categorias" => _sp.GetRequiredService<CategoriasViewModel>(),
        "pre-vendas" => CriarPreVendas(),
        "estoque" => CriarEstoque(),
        "compras" => CriarCompras(),
        _ => null,
    };

    // PreVendasViewModel depende do usuário logado (registra quem abriu a pré-venda),
    // que só existe em runtime após o login — por isso é montado à mão, não via DI puro.
    private PreVendasViewModel? CriarPreVendas()
    {
        if (_usuario is null)
            return null;

        // Factory de form: cada janela de pré-venda recebe um ViewModel novo (limpo) — permite
        // abrir várias janelas sem compartilhar estado entre elas.
        return new PreVendasViewModel(
            _usuario,
            _sp.GetRequiredService<IPreVendaService>(),
            () => _sp.GetRequiredService<PreVendaFormViewModel>(),
            _sp.GetRequiredService<ILogger<PreVendasViewModel>>());
    }

    // EstoqueViewModel também depende do usuário logado (registra quem movimentou) — montado à mão.
    private EstoqueViewModel? CriarEstoque()
    {
        if (_usuario is null)
            return null;

        return new EstoqueViewModel(
            _usuario,
            _sp.GetRequiredService<IEstoqueService>(),
            () => _sp.GetRequiredService<MovimentoEstoqueFormViewModel>(),
            _sp.GetRequiredService<ILogger<EstoqueViewModel>>());
    }

    // ComprasViewModel também depende do usuário logado (registra quem fez a compra) — montado à mão.
    private ComprasViewModel? CriarCompras()
    {
        if (_usuario is null)
            return null;

        return new ComprasViewModel(
            _usuario,
            _sp.GetRequiredService<ICompraService>(),
            () => _sp.GetRequiredService<CompraFormViewModel>(),
            _sp.GetRequiredService<ILogger<ComprasViewModel>>());
    }
}
