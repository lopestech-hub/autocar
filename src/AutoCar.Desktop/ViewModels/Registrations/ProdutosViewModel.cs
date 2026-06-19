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
/// Listagem de produtos: busca por descrição/código e abre o formulário (novo/edição).
/// Mesmo padrão de listagem de Cliente/Fornecedor/Marca.
/// </summary>
public partial class ProdutosViewModel : ViewModelBase
{
    private readonly IProdutoService _produtos;
    private readonly ProdutoFormViewModel _form;
    private readonly ILogger<ProdutosViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public ProdutosViewModel(IProdutoService produtos, ProdutoFormViewModel form, ILogger<ProdutosViewModel> logger)
    {
        _produtos = produtos;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

    public ObservableCollection<ProdutoListaDto> Produtos { get; } = new();

    [ObservableProperty] private string _filtro = string.Empty;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string TextoContador => Produtos.Count.ToString();

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

    [ObservableProperty] private ProdutoFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(ProdutoFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var lista = await _produtos.ListarAsync(Filtro);
            Produtos.Clear();
            foreach (var p in lista)
                Produtos.Add(p);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar produtos.";
            _logger.LogError(ex, "Erro ao listar produtos.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private async Task BuscarAsync() => await CarregarAsync();

    [RelayCommand]
    private async Task NovoAsync()
    {
        await _form.PrepararNovoAsync();
        FormularioAtivo = _form;
    }

    [RelayCommand]
    private async Task AbrirAsync(ProdutoListaDto? produto)
    {
        if (produto is null)
            return;

        await _form.CarregarAsync(produto.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
