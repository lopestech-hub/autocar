using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AutoCar.Application.Modules.Service.OrdensServico;
using AutoCar.Application.Modules.Service.OrdensServico.DTOs;
using AutoCar.Application.Modules.Security.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Service;

/// <summary>
/// Listagem de Ordens de Serviço: busca por cliente/placa/número e abre o formulário (nova/edição)
/// numa JANELA SEPARADA maximizada (mesmo padrão da Pré-venda). Dispara
/// <see cref="AbrirJanelaSolicitado"/> e a View (code-behind) abre a janela não-modal. Depende do
/// usuário logado (atendente) para registrar quem abriu a OS — por isso não vai no DI puro.
/// </summary>
public partial class OrdensServicoViewModel : ViewModelBase
{
    private readonly IOrdemServicoService _ordens;
    private readonly Func<OrdemServicoFormViewModel> _formFactory;
    private readonly ILogger<OrdensServicoViewModel> _logger;
    private readonly Guid _idUsuario;
    private readonly string _nomeAtendente;

    private CancellationTokenSource? _debounce;

    public OrdensServicoViewModel(
        UsuarioLogado usuario,
        IOrdemServicoService ordens,
        Func<OrdemServicoFormViewModel> formFactory,
        ILogger<OrdensServicoViewModel> logger)
    {
        _idUsuario = usuario.Id;
        _nomeAtendente = usuario.Nome;
        _ordens = ordens;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando o form deve abrir numa janela separada. A View abre a janela
    /// não-modal e recarrega a lista ao fechar com sucesso.</summary>
    public event Action<OrdemServicoFormViewModel>? AbrirJanelaSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela fechar com sucesso).</summary>
    public Task RecarregarAsync() => CarregarAsync();

    public ObservableCollection<OrdemServicoListaDto> Ordens { get; } = new();

    [ObservableProperty] private string _filtro = string.Empty;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string TextoContador => Ordens.Count.ToString();

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
            var lista = await _ordens.ListarAsync(Filtro);
            Ordens.Clear();
            foreach (var o in lista)
                Ordens.Add(o);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar as ordens de serviço.";
            _logger.LogError(ex, "Erro ao listar OS.");
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
        await form.PrepararNovaAsync(_idUsuario, _nomeAtendente);
        AbrirJanelaSolicitado?.Invoke(form);
    }

    [RelayCommand]
    private async Task AbrirAsync(OrdemServicoListaDto? os)
    {
        if (os is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(os.Id, _idUsuario, _nomeAtendente);
        AbrirJanelaSolicitado?.Invoke(form);
    }
}
