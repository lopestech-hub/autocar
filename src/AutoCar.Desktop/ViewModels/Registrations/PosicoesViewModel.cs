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
    private readonly Func<PosicaoFormViewModel> _formFactory;
    private readonly ILogger<PosicoesViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public PosicoesViewModel(IPosicaoPecaService posicoes, Func<PosicaoFormViewModel> formFactory, ILogger<PosicoesViewModel> logger)
    {
        _posicoes = posicoes;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando a janela de posição deve abrir. A View (code-behind) escuta, abre a
    /// janela não-modal e recarrega a lista ao salvar (mesmo padrão do Produto).</summary>
    public event Action<PosicaoFormViewModel>? AbrirFormularioSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela salvar).</summary>
    public Task RecarregarAsync() => CarregarAsync();

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
        var form = _formFactory();
        form.PrepararNovo();
        AbrirFormularioSolicitado?.Invoke(form);
    }

    [RelayCommand]
    private async Task AbrirAsync(PosicaoPecaDto? posicao)
    {
        if (posicao is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(posicao.Id);
        AbrirFormularioSolicitado?.Invoke(form);
    }
}
