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
/// Listagem de categorias de produto: busca por descrição e abre o formulário.
/// Cadastro mestre auxiliar do Produto. Mesmo padrão de Cliente/Fornecedor.
/// </summary>
public partial class CategoriasViewModel : ViewModelBase
{
    private readonly ICategoriaProdutoService _categorias;
    private readonly Func<CategoriaFormViewModel> _formFactory;
    private readonly ILogger<CategoriasViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public CategoriasViewModel(ICategoriaProdutoService categorias, Func<CategoriaFormViewModel> formFactory, ILogger<CategoriasViewModel> logger)
    {
        _categorias = categorias;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando a janela de categoria deve abrir. A View (code-behind) escuta, abre a
    /// janela não-modal e recarrega a lista ao salvar (mesmo padrão do Produto).</summary>
    public event Action<CategoriaFormViewModel>? AbrirFormularioSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela salvar).</summary>
    public Task RecarregarAsync() => CarregarAsync();

    public ObservableCollection<CategoriaProdutoDto> Categorias { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public string TextoContador => Categorias.Count.ToString();

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
            var lista = await _categorias.ListarAsync(Filtro);
            Categorias.Clear();
            foreach (var c in lista)
                Categorias.Add(c);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (System.Exception ex)
        {
            MensagemErro = "Falha ao carregar categorias.";
            _logger.LogError(ex, "Erro ao listar categorias.");
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
    private async Task AbrirAsync(CategoriaProdutoDto? categoria)
    {
        if (categoria is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(categoria.Id);
        AbrirFormularioSolicitado?.Invoke(form);
    }
}
