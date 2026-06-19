using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AutoCar.Application.Modules.Sales.PreVendas;
using AutoCar.Application.Modules.Sales.PreVendas.DTOs;
using AutoCar.Application.Modules.Security.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Sales;

/// <summary>
/// Listagem de pré-vendas: busca por cliente/número e abre o formulário (nova/edição) numa
/// JANELA SEPARADA maximizada. A listagem não embute mais o form — dispara
/// <see cref="AbrirJanelaSolicitado"/> e a View (code-behind) abre a PreVendaWindow não-modal,
/// mantendo o shell principal acessível. Depende do usuário logado (vendedor) para registrar
/// quem abriu a pré-venda — por isso recebe o UsuarioLogado e não vai no DI puro.
/// </summary>
public partial class PreVendasViewModel : ViewModelBase
{
    private readonly IPreVendaService _preVendas;
    private readonly Func<PreVendaFormViewModel> _formFactory;
    private readonly ILogger<PreVendasViewModel> _logger;
    private readonly Guid _idUsuario;
    private readonly string _nomeVendedor;

    private CancellationTokenSource? _debounce;

    public PreVendasViewModel(
        UsuarioLogado usuario,
        IPreVendaService preVendas,
        Func<PreVendaFormViewModel> formFactory,
        ILogger<PreVendasViewModel> logger)
    {
        _idUsuario = usuario.Id;
        _nomeVendedor = usuario.Nome;
        _preVendas = preVendas;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando o form da pré-venda deve abrir numa janela separada. A View
    /// (code-behind) escuta, abre a PreVendaWindow não-modal e recarrega a lista ao salvar.</summary>
    public event Action<PreVendaFormViewModel>? AbrirJanelaSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela fechar com sucesso).</summary>
    public Task RecarregarAsync() => CarregarAsync();

    public ObservableCollection<PreVendaListaDto> PreVendas { get; } = new();

    [ObservableProperty] private string _filtro = string.Empty;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string TextoContador => PreVendas.Count.ToString();

    partial void OnFiltroChanged(string value) => AgendarBusca();

    private void AgendarBusca()
    {
        _debounce?.Cancel();
        _debounce = new CancellationTokenSource();
        var token = _debounce.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(350, token);
                if (!token.IsCancellationRequested)
                    await Dispatcher.UIThread.InvokeAsync(CarregarAsync);
            }
            catch (TaskCanceledException) { /* nova tecla cancelou: ignora */ }
        });
    }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var lista = await _preVendas.ListarAsync(Filtro);
            PreVendas.Clear();
            foreach (var p in lista)
                PreVendas.Add(p);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar pré-vendas.";
            _logger.LogError(ex, "Erro ao listar pré-vendas.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private async Task BuscarAsync() => await CarregarAsync();

    [RelayCommand]
    private async Task NovoAsync()
    {
        var form = _formFactory();
        await form.PrepararNovaAsync(_idUsuario, _nomeVendedor);
        AbrirJanelaSolicitado?.Invoke(form);
    }

    [RelayCommand]
    private async Task AbrirAsync(PreVendaListaDto? preVenda)
    {
        if (preVenda is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(preVenda.Id, _idUsuario, _nomeVendedor);
        AbrirJanelaSolicitado?.Invoke(form);
    }
}
