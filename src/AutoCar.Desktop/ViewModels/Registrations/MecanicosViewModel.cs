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
    private readonly Func<MecanicoFormViewModel> _formFactory;
    private readonly ILogger<MecanicosViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public MecanicosViewModel(IMecanicoService mecanicos, Func<MecanicoFormViewModel> formFactory, ILogger<MecanicosViewModel> logger)
    {
        _mecanicos = mecanicos;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando a janela de mecânico deve abrir. A View (code-behind) escuta, abre a
    /// janela não-modal e recarrega a lista ao salvar (mesmo padrão do Produto).</summary>
    public event Action<MecanicoFormViewModel>? AbrirFormularioSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela salvar).</summary>
    public Task RecarregarAsync() => CarregarAsync();

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
        var form = _formFactory();
        form.PrepararNovo();
        AbrirFormularioSolicitado?.Invoke(form);
    }

    [RelayCommand]
    private async Task AbrirAsync(MecanicoDto? mecanico)
    {
        if (mecanico is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(mecanico.Id);
        AbrirFormularioSolicitado?.Invoke(form);
    }
}
