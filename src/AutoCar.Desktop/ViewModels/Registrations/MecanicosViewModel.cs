using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AutoCar.Application.Modules.Registrations.Mecanicos;
using AutoCar.Application.Modules.Registrations.Mecanicos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Listagem de mecânicos: busca por nome e abre o formulário (novo/edição). Cadastro mestre
/// auxiliar da Ordem de Serviço (o mecânico não é usuário). Mesmo padrão da listagem de Serviço.
/// </summary>
public partial class MecanicosViewModel : ViewModelBase
{
    private readonly IMecanicoService _mecanicos;
    private readonly MecanicoFormViewModel _form;
    private readonly ILogger<MecanicosViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public MecanicosViewModel(IMecanicoService mecanicos, MecanicoFormViewModel form, ILogger<MecanicosViewModel> logger)
    {
        _mecanicos = mecanicos;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

    public ObservableCollection<MecanicoDto> Mecanicos { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public string TextoContador => Mecanicos.Count.ToString();

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
    private MecanicoFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(MecanicoFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var lista = await _mecanicos.ListarAsync(Filtro);
            Mecanicos.Clear();
            foreach (var m in lista)
                Mecanicos.Add(m);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (System.Exception ex)
        {
            MensagemErro = "Falha ao carregar mecânicos.";
            _logger.LogError(ex, "Erro ao listar mecânicos.");
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
    private async Task AbrirAsync(MecanicoDto? mecanico)
    {
        if (mecanico is null)
            return;

        await _form.CarregarAsync(mecanico.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
