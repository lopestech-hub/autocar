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
/// Listagem de marcas: busca por descrição e abre o formulário (novo/edição).
/// Cadastro mestre auxiliar do Produto. Mesmo padrão de listagem de Cliente/Fornecedor.
/// </summary>
public partial class MarcasViewModel : ViewModelBase
{
    private readonly IMarcaService _marcas;
    private readonly Func<MarcaFormViewModel> _formFactory;
    private readonly ILogger<MarcasViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public MarcasViewModel(IMarcaService marcas, Func<MarcaFormViewModel> formFactory, ILogger<MarcasViewModel> logger)
    {
        _marcas = marcas;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando a janela de marca deve abrir. A View (code-behind) escuta, abre a
    /// janela não-modal e recarrega a lista ao salvar (mesmo padrão do Produto).</summary>
    public event Action<MarcaFormViewModel>? AbrirFormularioSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela salvar).</summary>
    public Task RecarregarAsync() => CarregarAsync();

    public ObservableCollection<MarcaDto> Marcas { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public string TextoContador => Marcas.Count.ToString();

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
            var lista = await _marcas.ListarAsync(Filtro);
            Marcas.Clear();
            foreach (var m in lista)
                Marcas.Add(m);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (System.Exception ex)
        {
            MensagemErro = "Falha ao carregar marcas.";
            _logger.LogError(ex, "Erro ao listar marcas.");
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
    private async Task AbrirAsync(MarcaDto? marca)
    {
        if (marca is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(marca.Id);
        AbrirFormularioSolicitado?.Invoke(form);
    }
}
