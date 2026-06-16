using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AutoCar.Application.Modules.Registrations.Produtos;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Listagem de lados da peça: busca por descrição e abre o formulário (novo/edição).
/// Cadastro mestre auxiliar do Produto. Mesmo padrão de listagem de Marca.
/// </summary>
public partial class LadosViewModel : ViewModelBase
{
    private readonly ILadoPecaService _lados;
    private readonly LadoFormViewModel _form;
    private readonly ILogger<LadosViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public LadosViewModel(ILadoPecaService lados, LadoFormViewModel form, ILogger<LadosViewModel> logger)
    {
        _lados = lados;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

    public ObservableCollection<LadoPecaDto> Lados { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public string TextoContador => Lados.Count switch
    {
        0 => "Nenhum lado",
        1 => "1 lado",
        var n => $"{n} lados",
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
    private LadoFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(LadoFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var lista = await _lados.ListarAsync(Filtro);
            Lados.Clear();
            foreach (var l in lista)
                Lados.Add(l);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (System.Exception ex)
        {
            MensagemErro = "Falha ao carregar lados.";
            _logger.LogError(ex, "Erro ao listar lados.");
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
    private async Task AbrirAsync(LadoPecaDto? lado)
    {
        if (lado is null)
            return;

        await _form.CarregarAsync(lado.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
