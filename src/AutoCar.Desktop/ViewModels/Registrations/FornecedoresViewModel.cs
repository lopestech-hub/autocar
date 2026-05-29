using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AutoCar.Application.Modules.Registrations.Fornecedores;
using AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Listagem de fornecedores: busca por nome/documento e abre o formulário (novo/edição).
/// A View (Grid tabular) segue o padrão de ClientesView.
/// </summary>
public partial class FornecedoresViewModel : ViewModelBase
{
    private readonly IFornecedorService _fornecedores;
    private readonly FornecedorFormViewModel _form;
    private readonly ILogger<FornecedoresViewModel> _logger;

    // Debounce da busca automática enquanto o usuário digita.
    private CancellationTokenSource? _debounce;

    public FornecedoresViewModel(IFornecedorService fornecedores, FornecedorFormViewModel form, ILogger<FornecedoresViewModel> logger)
    {
        _fornecedores = fornecedores;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

    public ObservableCollection<FornecedorListaDto> Fornecedores { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    /// <summary>Texto do contador de registros ("12 fornecedores" / "1 fornecedor" / "Nenhum fornecedor").</summary>
    public string TextoContador => Fornecedores.Count switch
    {
        0 => "Nenhum fornecedor",
        1 => "1 fornecedor",
        var n => $"{n} fornecedores",
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
    private FornecedorFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(FornecedorFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var lista = await _fornecedores.ListarAsync(Filtro);
            Fornecedores.Clear();
            foreach (var f in lista)
                Fornecedores.Add(f);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (System.Exception ex)
        {
            MensagemErro = "Falha ao carregar fornecedores.";
            _logger.LogError(ex, "Erro ao listar fornecedores.");
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
    private async Task AbrirAsync(FornecedorListaDto? fornecedor)
    {
        if (fornecedor is null)
            return;

        await _form.CarregarAsync(fornecedor.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
