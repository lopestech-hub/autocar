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
    private readonly Func<ProdutoFormViewModel> _formFactory;
    private readonly ILogger<ProdutosViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public ProdutosViewModel(IProdutoService produtos, Func<ProdutoFormViewModel> formFactory, ILogger<ProdutosViewModel> logger)
    {
        _produtos = produtos;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando a janela de produto deve abrir. A View (code-behind) escuta, abre a
    /// janela não-modal e recarrega a lista ao salvar (mesmo padrão do estoque/pré-venda).</summary>
    public event Action<ProdutoFormViewModel>? AbrirFormularioSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela salvar).</summary>
    public Task RecarregarAsync() => CarregarAsync();

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
        var form = _formFactory();
        await form.PrepararNovoAsync();
        AbrirFormularioSolicitado?.Invoke(form);
    }

    [RelayCommand]
    private async Task AbrirAsync(ProdutoListaDto? produto)
    {
        if (produto is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(produto.Id);
        AbrirFormularioSolicitado?.Invoke(form);
    }
}
