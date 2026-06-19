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
/// Listagem de posições da peça: busca por descrição e abre o formulário (novo/edição).
/// Cadastro mestre auxiliar do Produto. Mesmo padrão de listagem de Marca.
/// </summary>
public partial class PosicoesViewModel : ViewModelBase
{
    private readonly IPosicaoPecaService _posicoes;
    private readonly PosicaoFormViewModel _form;
    private readonly ILogger<PosicoesViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public PosicoesViewModel(IPosicaoPecaService posicoes, PosicaoFormViewModel form, ILogger<PosicoesViewModel> logger)
    {
        _posicoes = posicoes;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

    public ObservableCollection<PosicaoPecaDto> Posicoes { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public string TextoContador => Posicoes.Count.ToString();

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
    private PosicaoFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(PosicaoFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var lista = await _posicoes.ListarAsync(Filtro);
            Posicoes.Clear();
            foreach (var p in lista)
                Posicoes.Add(p);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (System.Exception ex)
        {
            MensagemErro = "Falha ao carregar posições.";
            _logger.LogError(ex, "Erro ao listar posições.");
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
    private async Task AbrirAsync(PosicaoPecaDto? posicao)
    {
        if (posicao is null)
            return;

        await _form.CarregarAsync(posicao.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
