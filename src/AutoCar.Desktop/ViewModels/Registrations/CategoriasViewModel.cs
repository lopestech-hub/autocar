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
    private readonly CategoriaFormViewModel _form;
    private readonly ILogger<CategoriasViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public CategoriasViewModel(ICategoriaProdutoService categorias, CategoriaFormViewModel form, ILogger<CategoriasViewModel> logger)
    {
        _categorias = categorias;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

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

    [ObservableProperty]
    private CategoriaFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(CategoriaFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

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
        _form.PrepararNovo();
        FormularioAtivo = _form;
    }

    [RelayCommand]
    private async Task AbrirAsync(CategoriaProdutoDto? categoria)
    {
        if (categoria is null)
            return;

        await _form.CarregarAsync(categoria.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
