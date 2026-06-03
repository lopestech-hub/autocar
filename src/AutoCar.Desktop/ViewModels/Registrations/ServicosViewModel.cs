using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AutoCar.Application.Modules.Registrations.Servicos;
using AutoCar.Application.Modules.Registrations.Servicos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Listagem de serviços (catálogo de mão de obra): busca por descrição e abre o formulário
/// (novo/edição). Cadastro mestre auxiliar da Ordem de Serviço. Mesmo padrão da listagem de Marca.
/// </summary>
public partial class ServicosViewModel : ViewModelBase
{
    private readonly IServicoService _servicos;
    private readonly ServicoFormViewModel _form;
    private readonly ILogger<ServicosViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public ServicosViewModel(IServicoService servicos, ServicoFormViewModel form, ILogger<ServicosViewModel> logger)
    {
        _servicos = servicos;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

    public ObservableCollection<ServicoDto> Servicos { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public string TextoContador => Servicos.Count switch
    {
        0 => "Nenhum serviço",
        1 => "1 serviço",
        var n => $"{n} serviços",
    };

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

    [ObservableProperty]
    private ServicoFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(ServicoFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var lista = await _servicos.ListarAsync(Filtro);
            Servicos.Clear();
            foreach (var s in lista)
                Servicos.Add(s);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (System.Exception ex)
        {
            MensagemErro = "Falha ao carregar serviços.";
            _logger.LogError(ex, "Erro ao listar serviços.");
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
    private async Task AbrirAsync(ServicoDto? servico)
    {
        if (servico is null)
            return;

        await _form.CarregarAsync(servico.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
