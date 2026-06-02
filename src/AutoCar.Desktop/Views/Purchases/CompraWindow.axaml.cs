using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Fornecedores;
using AutoCar.Desktop.ViewModels.Catalogo;
using AutoCar.Desktop.ViewModels.Purchases;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Desktop.Views.Purchases;

/// <summary>
/// Janela separada (maximizada, não-modal) que hospeda o formulário de compra. O shell principal
/// continua aberto e acessível. F2 abre o Catálogo (seletor de peça) e F3 abre o seletor de
/// fornecedor sobre esta janela. A janela fecha sozinha quando o form é salvo ou cancelado.
/// </summary>
public partial class CompraWindow : Window
{
    private CompraFormViewModel? _vm;

    public CompraWindow()
    {
        InitializeComponent();
    }

    public CompraWindow(CompraFormViewModel viewModel) : this()
    {
        _vm = viewModel;
        DataContext = viewModel;
        viewModel.Salvo += Close;
        viewModel.Cancelado += Close;
        viewModel.AbrirCatalogoSolicitado += AbrirCatalogo;
        viewModel.AbrirSeletorFornecedorSolicitado += AbrirSeletorFornecedor;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Abre o Catálogo numa janela seletora (modal sobre a compra). Ao clicar numa peça, ela é
    /// adicionada à compra e a janela do catálogo fecha. CatalogoViewModel vem do DI (tela limpa).
    /// </summary>
    private void AbrirCatalogo()
    {
        if (_vm is null)
            return;

        var catalogoVm = App.Services.GetRequiredService<CatalogoViewModel>();
        var seletor = new Sales.CatalogoSeletorWindow(catalogoVm, peca => _vm.AdicionarPecaDoCatalogo(peca));
        seletor.ShowDialog(this);
    }

    /// <summary>
    /// Abre o seletor de fornecedor (modal sobre a compra). Busca por nome/código, setas,
    /// Enter/duplo-clique escolhe. IFornecedorService vem do DI.
    /// </summary>
    private void AbrirSeletorFornecedor()
    {
        if (_vm is null)
            return;

        var fornecedores = App.Services.GetRequiredService<IFornecedorService>();
        var seletor = new FornecedorSeletorWindow(fornecedores, f => _vm.DefinirFornecedor(f));
        seletor.ShowDialog(this);
    }
}
