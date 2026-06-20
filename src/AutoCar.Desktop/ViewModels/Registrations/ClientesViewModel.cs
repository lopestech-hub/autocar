using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AutoCar.Application.Modules.Registrations.Clientes;
using AutoCar.Application.Modules.Registrations.Clientes.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Listagem de clientes: busca por nome/documento e abre o formulário (novo/edição).
/// A View (Grid tabular) é construída pela Luna.
/// </summary>
public partial class ClientesViewModel : ViewModelBase
{
    private readonly IClienteService _clientes;
    private readonly Func<ClienteFormViewModel> _formFactory;
    private readonly ILogger<ClientesViewModel> _logger;

    // Debounce da busca automática enquanto o usuário digita.
    private CancellationTokenSource? _debounce;

    public ClientesViewModel(IClienteService clientes, Func<ClienteFormViewModel> formFactory, ILogger<ClientesViewModel> logger)
    {
        _clientes = clientes;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando a janela de cliente deve abrir. A View (code-behind) escuta, abre a
    /// janela não-modal e recarrega a lista ao salvar (mesmo padrão do Produto).</summary>
    public event Action<ClienteFormViewModel>? AbrirFormularioSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela salvar).</summary>
    public Task RecarregarAsync() => CarregarAsync();

    public ObservableCollection<ClienteListaDto> Clientes { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    /// <summary>Contador de registros (só o número de itens).</summary>
    public string TextoContador => Clientes.Count.ToString();

    // Busca automática: ao digitar no filtro, agenda a busca após 350ms de pausa.
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
            var lista = await _clientes.ListarAsync(Filtro);
            Clientes.Clear();
            foreach (var c in lista)
                Clientes.Add(c);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (System.Exception ex)
        {
            MensagemErro = "Falha ao carregar clientes.";
            _logger.LogError(ex, "Erro ao listar clientes.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private async Task BuscarAsync() => await CarregarAsync();

    [RelayCommand]
    private void Novo()
    {
        var form = _formFactory();
        form.PrepararNovo();
        AbrirFormularioSolicitado?.Invoke(form);
    }

    [RelayCommand]
    private async Task AbrirAsync(ClienteListaDto? cliente)
    {
        if (cliente is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(cliente.Id);
        AbrirFormularioSolicitado?.Invoke(form);
    }
}
