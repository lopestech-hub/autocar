using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Clientes;
using AutoCar.Desktop.ViewModels.Catalogo;
using AutoCar.Desktop.ViewModels.Sales;
using AutoCar.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Desktop.Views.Sales;

/// <summary>
/// Janela separada (maximizada, não-modal) que hospeda o formulário de pré-venda. O shell
/// principal continua aberto e acessível. F2 abre o Catálogo (janela seletora de peça) sobre
/// esta janela. A janela fecha sozinha quando o form é salvo ou cancelado.
/// </summary>
public partial class PreVendaWindow : Window
{
    private PreVendaFormViewModel? _vm;

    public PreVendaWindow()
    {
        InitializeComponent();
    }

    public PreVendaWindow(PreVendaFormViewModel viewModel) : this()
    {
        _vm = viewModel;
        DataContext = viewModel;
        viewModel.Salvo += Close;
        viewModel.Cancelado += Close;
        viewModel.Faturado += Close;
        viewModel.AbrirCatalogoSolicitado += AbrirCatalogo;
        viewModel.AbrirSeletorClienteSolicitado += AbrirSeletorCliente;
        viewModel.ConfirmacaoFaturamentoSolicitada += ConfirmarFaturamento;
        viewModel.AbrirDevolucaoSolicitada += AbrirDevolucao;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Abre o Catálogo numa janela seletora (modal sobre a pré-venda). Ao clicar numa peça,
    /// ela é adicionada à pré-venda e a janela do catálogo fecha. CatalogoViewModel vem do DI
    /// (instância nova por abertura, tela limpa).
    /// </summary>
    private void AbrirCatalogo()
    {
        if (_vm is null)
            return;

        var catalogoVm = App.Services.GetRequiredService<CatalogoViewModel>();
        var seletor = new CatalogoSeletorWindow(catalogoVm, peca => _vm.AdicionarPecaDoCatalogo(peca));
        seletor.ShowDialog(this); // modal sobre a pré-venda; o shell principal segue acessível por trás dela
    }

    /// <summary>
    /// Abre o seletor de cliente (modal sobre a pré-venda). Busca por nome/código, setas,
    /// Enter/duplo-clique escolhe; "Consumidor" devolve null. IClienteService vem do DI.
    /// </summary>
    private void AbrirSeletorCliente()
    {
        if (_vm is null)
            return;

        var clientes = App.Services.GetRequiredService<IClienteService>();
        var seletor = new ClienteSeletorWindow(clientes, cli => _vm.DefinirCliente(cli));
        seletor.ShowDialog(this);
    }

    /// <summary>
    /// Confirmação antes de faturar (ação irreversível que baixa o estoque). Se o usuário confirmar,
    /// o ViewModel efetiva o faturamento; em sucesso, ele dispara Faturado e a janela fecha.
    /// </summary>
    private async void ConfirmarFaturamento()
    {
        if (_vm is null)
            return;

        var confirmado = await ConfirmacaoWindow.MostrarAsync(
            this,
            "Faturar pré-venda",
            "Ao faturar, o estoque dos itens será baixado e o documento não poderá mais ser alterado. Deseja continuar?",
            textoConfirmar: "Faturar");

        if (confirmado)
            await _vm.ConfirmarFaturamentoAsync();
    }

    /// <summary>
    /// Abre a tela de devolução da venda faturada numa janela separada (não-modal). A pré-venda
    /// permanece aberta (a venda continua Faturada — a devolução é um documento à parte). A janela de
    /// devolução fecha sozinha ao concluir (ela mesma trata isso).
    /// </summary>
    private async void AbrirDevolucao()
    {
        if (_vm?.IdPreVenda is not Guid idPreVenda)
            return;

        var devolucaoVm = App.Services.GetRequiredService<DevolucaoFormViewModel>();
        await devolucaoVm.PrepararAsync(idPreVenda, _vm.CodPreVenda, _vm.IdUsuarioLogado);

        var janela = new DevolucaoWindow(devolucaoVm);
        janela.Show(this);
    }
}
