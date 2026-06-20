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
/// Listagem de grupos de produto: busca por descrição e abre o formulário (novo/edição).
/// Cadastro mestre auxiliar do Produto (nível Categoria → Grupo → Produto).
/// </summary>
public partial class GruposViewModel : ViewModelBase
{
    private readonly IGrupoProdutoService _grupos;
    private readonly Func<GrupoFormViewModel> _formFactory;
    private readonly ILogger<GruposViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public GruposViewModel(IGrupoProdutoService grupos, Func<GrupoFormViewModel> formFactory, ILogger<GruposViewModel> logger)
    {
        _grupos = grupos;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando a janela de grupo deve abrir. A View (code-behind) escuta, abre a
    /// janela não-modal e recarrega a lista ao salvar (mesmo padrão do Produto).</summary>
    public event Action<GrupoFormViewModel>? AbrirFormularioSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela salvar).</summary>
    public Task RecarregarAsync() => CarregarAsync();

    public ObservableCollection<GrupoProdutoDto> Grupos { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public string TextoContador => Grupos.Count.ToString();

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
            var lista = await _grupos.ListarAsync(Filtro);
            Grupos.Clear();
            foreach (var g in lista)
                Grupos.Add(g);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (System.Exception ex)
        {
            MensagemErro = "Falha ao carregar grupos.";
            _logger.LogError(ex, "Erro ao listar grupos.");
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
    private async Task AbrirAsync(GrupoProdutoDto? grupo)
    {
        if (grupo is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(grupo.Id);
        AbrirFormularioSolicitado?.Invoke(form);
    }
}
