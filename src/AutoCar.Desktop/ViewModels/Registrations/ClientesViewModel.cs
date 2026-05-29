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
    private readonly ClienteFormViewModel _form;
    private readonly ILogger<ClientesViewModel> _logger;

    // Debounce da busca automática enquanto o usuário digita.
    private CancellationTokenSource? _debounce;

    public ClientesViewModel(IClienteService clientes, ClienteFormViewModel form, ILogger<ClientesViewModel> logger)
    {
        _clientes = clientes;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

    public ObservableCollection<ClienteListaDto> Clientes { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    /// <summary>Texto do contador de registros ("12 clientes" / "1 cliente" / "Nenhum cliente").</summary>
    public string TextoContador => Clientes.Count switch
    {
        0 => "Nenhum cliente",
        1 => "1 cliente",
        var n => $"{n} clientes",
    };

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

    /// <summary>Formulário sobreposto à listagem. Null = listagem visível.</summary>
    [ObservableProperty]
    private ClienteFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(ClienteFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

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
        _form.PrepararNovo();
        FormularioAtivo = _form;
    }

    [RelayCommand]
    private async Task AbrirAsync(ClienteListaDto? cliente)
    {
        if (cliente is null)
            return;

        await _form.CarregarAsync(cliente.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
