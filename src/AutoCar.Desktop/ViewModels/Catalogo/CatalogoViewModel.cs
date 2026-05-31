using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AutoCar.Application.Modules.Registrations.Produtos;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Catalogo;

/// <summary>
/// Catálogo automotivo: busca de peças filtrando por veículo (montadora/modelo/ano).
/// Responde "quais peças servem nesse carro?". Todos os filtros são opcionais e vão
/// estreitando o resultado. Consulta (não cadastro) — sem formulário.
/// </summary>
public partial class CatalogoViewModel : ViewModelBase
{
    private readonly IProdutoService _produtos;
    private readonly ILogger<CatalogoViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public CatalogoViewModel(IProdutoService produtos, ILogger<CatalogoViewModel> logger)
    {
        _produtos = produtos;
        _logger = logger;
    }

    /// <summary>Resultado da busca (peças que servem no veículo filtrado).</summary>
    public ObservableCollection<CatalogoItemDto> Resultados { get; } = new();

    /// <summary>Montadoras já cadastradas (autocomplete).</summary>
    public ObservableCollection<string> Montadoras { get; } = new();

    /// <summary>Modelos já cadastrados, filtrados pela montadora escolhida (autocomplete).</summary>
    public ObservableCollection<string> Modelos { get; } = new();

    [ObservableProperty] private string? _termo;
    [ObservableProperty] private string? _montadora;
    [ObservableProperty] private string? _modelo;
    [ObservableProperty] private string? _anoTexto;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string TextoContador => Resultados.Count switch
    {
        0 => "Nenhuma peça",
        1 => "1 peça",
        var n => $"{n} peças",
    };

    // Cada filtro dispara uma nova busca (com debounce, para não bater a cada tecla).
    partial void OnTermoChanged(string? value) => AgendarBusca();
    partial void OnModeloChanged(string? value) => AgendarBusca();
    partial void OnAnoTextoChanged(string? value) => AgendarBusca();

    partial void OnMontadoraChanged(string? value)
    {
        // Trocar a montadora recarrega os modelos disponíveis e refaz a busca.
        _ = CarregarModelosAsync();
        AgendarBusca();
    }

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
                    await Dispatcher.UIThread.InvokeAsync(BuscarAsync);
            }
            catch (TaskCanceledException) { /* nova tecla cancelou: ignora */ }
        });
    }

    /// <summary>Carrega as montadoras para o autocomplete (chamado ao abrir a tela).</summary>
    [RelayCommand]
    private async Task InicializarAsync()
    {
        try
        {
            var montadoras = await _produtos.ListarMontadorasAsync();
            Montadoras.Clear();
            foreach (var m in montadoras) Montadoras.Add(m);
            await CarregarModelosAsync();
            await BuscarAsync();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar o catálogo.";
            _logger.LogError(ex, "Erro ao inicializar o catálogo.");
        }
    }

    [RelayCommand]
    private async Task BuscarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            int? ano = int.TryParse((AnoTexto ?? string.Empty).Trim(),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out var a) ? a : null;

            var filtro = new BuscaCatalogoDto(Termo, Montadora, Modelo, ano);
            var lista = await _produtos.BuscarCatalogoAsync(filtro);

            Resultados.Clear();
            foreach (var item in lista) Resultados.Add(item);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao buscar no catálogo.";
            _logger.LogError(ex, "Erro ao buscar no catálogo.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Limpar()
    {
        Termo = Montadora = Modelo = AnoTexto = null;
        // Os setters disparam a busca; garante recarregar modelos sem montadora.
    }

    private async Task CarregarModelosAsync()
    {
        try
        {
            var modelos = await _produtos.ListarModelosAsync(Montadora);
            Modelos.Clear();
            foreach (var m in modelos) Modelos.Add(m);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar modelos do catálogo.");
        }
    }
}
