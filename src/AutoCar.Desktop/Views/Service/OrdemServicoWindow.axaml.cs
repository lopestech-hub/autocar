using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoCar.Application.Modules.Registrations.Clientes;
using AutoCar.Application.Modules.Registrations.Mecanicos;
using AutoCar.Application.Modules.Registrations.Servicos;
using AutoCar.Desktop.ViewModels.Catalogo;
using AutoCar.Desktop.ViewModels.Service;
using AutoCar.Desktop.Views;
using AutoCar.Desktop.Views.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCar.Desktop.Views.Service;

/// <summary>
/// Janela separada (maximizada, não-modal) que hospeda o formulário da Ordem de Serviço. O shell
/// principal continua acessível. F2 abre o Catálogo (peça), F3 o seletor de cliente, F4 o seletor de
/// serviço — todos modais sobre esta janela. A janela fecha quando o form é salvo ou cancelado.
/// </summary>
public partial class OrdemServicoWindow : Window
{
    private OrdemServicoFormViewModel? _vm;

    public OrdemServicoWindow()
    {
        InitializeComponent();
    }

    public OrdemServicoWindow(OrdemServicoFormViewModel viewModel) : this()
    {
        _vm = viewModel;
        DataContext = viewModel;
        viewModel.Salvo += Close;
        viewModel.Cancelado += Close;
        viewModel.AbrirCatalogoSolicitado += AbrirCatalogo;
        viewModel.AbrirSeletorClienteSolicitado += AbrirSeletorCliente;
        viewModel.AbrirSeletorServicoSolicitado += AbrirSeletorServico;
        viewModel.AbrirSeletorMecanicoSolicitado += AbrirSeletorMecanico;
        viewModel.ConfirmacaoCancelamentoSolicitada += ConfirmarCancelamento;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Abre o Catálogo numa janela seletora (modal sobre a OS). Ao escolher uma peça, ela é
    /// adicionada à OS e a janela do catálogo fecha. CatalogoViewModel vem do DI (instância nova).
    /// </summary>
    private void AbrirCatalogo()
    {
        if (_vm is null)
            return;

        var catalogoVm = App.Services.GetRequiredService<CatalogoViewModel>();
        var seletor = new CatalogoSeletorWindow(catalogoVm, peca => _vm.AdicionarPecaDoCatalogo(peca));
        seletor.ShowDialog(this);
    }

    /// <summary>
    /// Abre o seletor de cliente (modal sobre a OS). Busca por nome/código; "Consumidor" devolve null.
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
    /// Abre o seletor de serviço (modal sobre a OS). Ao escolher, o serviço entra como uma linha de
    /// mão de obra na OS. IServicoService vem do DI.
    /// </summary>
    private void AbrirSeletorServico()
    {
        if (_vm is null)
            return;

        var servicos = App.Services.GetRequiredService<IServicoService>();
        var seletor = new ServicoSeletorWindow(servicos, srv => _vm.AdicionarServico(srv));
        seletor.ShowDialog(this);
    }

    /// <summary>
    /// Abre o seletor de mecânico (modal sobre a OS), mesmo padrão do seletor de cliente. Busca por
    /// nome/código; "Sem mecânico" devolve null. IMecanicoService vem do DI.
    /// </summary>
    private void AbrirSeletorMecanico()
    {
        if (_vm is null)
            return;

        var mecanicos = App.Services.GetRequiredService<IMecanicoService>();
        var seletor = new MecanicoSeletorWindow(mecanicos, mec => _vm.DefinirMecanico(mec));
        seletor.ShowDialog(this);
    }

    /// <summary>
    /// Confirmação antes de cancelar a OS (ação irreversível: a OS é encerrada e não pode ser
    /// reaberta). Se o usuário confirmar, o ViewModel efetiva o cancelamento e a tela recarrega.
    /// </summary>
    private async void ConfirmarCancelamento()
    {
        if (_vm is null)
            return;

        var confirmado = await ConfirmacaoWindow.MostrarAsync(
            this,
            "Cancelar ordem de serviço",
            "Ao cancelar, a OS será encerrada e não poderá mais ser alterada nem faturada. Deseja continuar?",
            textoConfirmar: "Cancelar OS");

        if (confirmado)
            await _vm.ConfirmarCancelamentoAsync();
    }
}
