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
    private readonly MarcaFormViewModel _form;
    private readonly ILogger<MarcasViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public MarcasViewModel(IMarcaService marcas, MarcaFormViewModel form, ILogger<MarcasViewModel> logger)
    {
        _marcas = marcas;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

    public ObservableCollection<MarcaDto> Marcas { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public string TextoContador => Marcas.Count switch
    {
        0 => "Nenhuma marca",
        1 => "1 marca",
        var n => $"{n} marcas",
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
    private MarcaFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(MarcaFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

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
        _form.PrepararNovo();
        FormularioAtivo = _form;
    }

    [RelayCommand]
    private async Task AbrirAsync(MarcaDto? marca)
    {
        if (marca is null)
            return;

        await _form.CarregarAsync(marca.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
