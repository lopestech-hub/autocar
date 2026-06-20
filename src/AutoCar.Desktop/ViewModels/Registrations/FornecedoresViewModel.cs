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
    private readonly Func<FornecedorFormViewModel> _formFactory;
    private readonly ILogger<FornecedoresViewModel> _logger;

    // Debounce da busca automática enquanto o usuário digita.
    private CancellationTokenSource? _debounce;

    public FornecedoresViewModel(IFornecedorService fornecedores, Func<FornecedorFormViewModel> formFactory, ILogger<FornecedoresViewModel> logger)
    {
        _fornecedores = fornecedores;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando a janela de fornecedor deve abrir. A View (code-behind) escuta, abre a
    /// janela não-modal e recarrega a lista ao salvar (mesmo padrão do Produto).</summary>
    public event Action<FornecedorFormViewModel>? AbrirFormularioSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela salvar).</summary>
    public Task RecarregarAsync() => CarregarAsync();

    public ObservableCollection<FornecedorListaDto> Fornecedores { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    /// <summary>Contador de registros (só o número de itens).</summary>
    public string TextoContador => Fornecedores.Count.ToString();

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
        var form = _formFactory();
        form.PrepararNovo();
        AbrirFormularioSolicitado?.Invoke(form);
    }

    [RelayCommand]
    private async Task AbrirAsync(FornecedorListaDto? fornecedor)
    {
        if (fornecedor is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(fornecedor.Id);
        AbrirFormularioSolicitado?.Invoke(form);
    }
}
