using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoCar.Application.Modules.Security.DTOs;
using AutoCar.Desktop.Navegacao;
using AutoCar.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AutoCar.Desktop.ViewModels;

/// <summary>
/// ViewModel do shell principal (layout ERP clássico: menu de texto + toolbar de
/// ícones + área central + barra de status). Monta menu e toolbar conforme o perfil
/// do usuário logado (itens sem permissão ficam ocultos) e controla o módulo ativo.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly PerfilUsuario[] TodosPerfis =
        { PerfilUsuario.Vendedor, PerfilUsuario.Mecanico, PerfilUsuario.Financeiro };

    // Catálogo completo de módulos: título, ícone FontAwesome, categoria (menu de texto),
    // se aparece na toolbar e perfis que atendem. Admin vê todos (ItemMenu.VisivelPara).
    // Categoria vazia ("") = não entra no menu de texto (ex: ação de busca).
    private static readonly IReadOnlyList<ItemMenu> TodosOsItens = new[]
    {
        // --- Atalhos da toolbar ---
        new ItemMenu("Buscar", "fa-solid fa-magnifying-glass", "", flgToolbar: true, TodosPerfis),
        new ItemMenu("Clientes", "fa-solid fa-user-group", "Cadastros", flgToolbar: true, new[] { PerfilUsuario.Vendedor, PerfilUsuario.Mecanico }, rota: "clientes"),
        new ItemMenu("Fornecedor", "fa-solid fa-truck-field", "Cadastros", flgToolbar: true, new[] { PerfilUsuario.Vendedor }, rota: "fornecedor"),
        new ItemMenu("Produtos", "fa-solid fa-box", "Cadastros", flgToolbar: true, new[] { PerfilUsuario.Vendedor, PerfilUsuario.Mecanico }, rota: "produtos"),
        new ItemMenu("Catálogo", "fa-solid fa-magnifying-glass", "Cadastros", flgToolbar: true, new[] { PerfilUsuario.Vendedor, PerfilUsuario.Mecanico }, rota: "catalogo"),
        new ItemMenu("Orçamento", "fa-solid fa-file-lines", "Movimentos", flgToolbar: true, new[] { PerfilUsuario.Vendedor }),
        new ItemMenu("Pré-venda", "fa-solid fa-cart-shopping", "Movimentos", flgToolbar: true, new[] { PerfilUsuario.Vendedor }, rota: "pre-vendas"),
        new ItemMenu("OS", "fa-solid fa-screwdriver-wrench", "Movimentos", flgToolbar: true, new[] { PerfilUsuario.Mecanico }),

        // --- Só no menu de texto ---
        new ItemMenu("Marcas", "fa-solid fa-tags", "Cadastros", flgToolbar: false, new[] { PerfilUsuario.Vendedor }, rota: "marcas"),
        new ItemMenu("Categorias", "fa-solid fa-layer-group", "Cadastros", flgToolbar: false, new[] { PerfilUsuario.Vendedor }, rota: "categorias"),
        new ItemMenu("Usuários", "fa-solid fa-users", "Cadastros", flgToolbar: false, Array.Empty<PerfilUsuario>()), // só Admin
        new ItemMenu("Estoque", "fa-solid fa-boxes-stacked", "Movimentos", flgToolbar: false, new[] { PerfilUsuario.Vendedor }),
        new ItemMenu("Caixa", "fa-solid fa-money-bill-wave", "Financeiro", flgToolbar: false, new[] { PerfilUsuario.Financeiro }),
        new ItemMenu("Contas a Receber", "fa-solid fa-arrow-down", "Financeiro", flgToolbar: false, new[] { PerfilUsuario.Financeiro }),
        new ItemMenu("Contas a Pagar", "fa-solid fa-arrow-up", "Financeiro", flgToolbar: false, new[] { PerfilUsuario.Financeiro }),
        new ItemMenu("Fiscal", "fa-solid fa-file-invoice-dollar", "Financeiro", flgToolbar: false, new[] { PerfilUsuario.Financeiro }),
    };

    // Ordem das categorias no menu superior de texto.
    private static readonly string[] OrdemCategorias =
        { "Cadastros", "Movimentos", "Financeiro" };

    private readonly INavegador _navegador;

    public MainWindowViewModel(UsuarioLogado usuario, INavegador navegador)
    {
        Usuario = usuario;
        _navegador = navegador;

        var visiveis = TodosOsItens.Where(i => i.VisivelPara(usuario.Perfil)).ToList();

        // Toolbar: só itens marcados com FlgToolbar.
        Itens = new ObservableCollection<ItemMenu>(visiveis.Where(i => i.FlgToolbar));

        // Menu de texto: agrupado por categoria (ignora itens sem categoria, ex: Buscar),
        // só categorias que têm pelo menos um item visível.
        Categorias = new ObservableCollection<CategoriaMenu>(
            OrdemCategorias
                .Select(cat => new CategoriaMenu(
                    cat, visiveis.Where(i => i.Categoria == cat).ToList()))
                .Where(c => c.Itens.Count > 0));
    }

    public UsuarioLogado Usuario { get; }

    public string NomeUsuario => Usuario.Nome;

    public string PerfilUsuarioTexto => Usuario.Perfil.ToString();

    /// <summary>Data atual no rodapé (formato brasileiro).</summary>
    public string DataAtual => DateTime.Now.ToString("dd/MM/yyyy");

    /// <summary>Itens para a toolbar de ícones.</summary>
    public ObservableCollection<ItemMenu> Itens { get; }

    /// <summary>Categorias para o menu superior de texto.</summary>
    public ObservableCollection<CategoriaMenu> Categorias { get; }

    /// <summary>Módulo atualmente aberto. Null = tela inicial (marca).</summary>
    [ObservableProperty]
    private ItemMenu? _itemAtivo;

    /// <summary>ViewModel da tela do módulo aberto. Null quando o módulo ainda não tem tela.</summary>
    [ObservableProperty]
    private ViewModelBase? _conteudoAtivo;

    /// <summary>Verdadeiro quando nenhum módulo está aberto — mostra a marca central.</summary>
    public bool MostrarBoasVindas => ItemAtivo is null;

    /// <summary>Módulo aberto mas sem tela implementada — mostra o placeholder "em construção".</summary>
    public bool MostrarPlaceholder => ItemAtivo is not null && ConteudoAtivo is null;

    partial void OnItemAtivoChanged(ItemMenu? value)
    {
        OnPropertyChanged(nameof(MostrarBoasVindas));
        OnPropertyChanged(nameof(MostrarPlaceholder));
    }

    partial void OnConteudoAtivoChanged(ViewModelBase? value) =>
        OnPropertyChanged(nameof(MostrarPlaceholder));

    /// <summary>Disparado ao clicar em Sair. A janela trata fechando e reabrindo o login.</summary>
    public event Action? SairSolicitado;

    [RelayCommand]
    private void Selecionar(ItemMenu item)
    {
        ItemAtivo = item;
        ConteudoAtivo = _navegador.Resolver(item.Rota);
    }

    [RelayCommand]
    private void Sair() => SairSolicitado?.Invoke();
}
