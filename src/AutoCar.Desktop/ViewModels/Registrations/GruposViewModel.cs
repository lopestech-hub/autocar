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
    private readonly GrupoFormViewModel _form;
    private readonly ILogger<GruposViewModel> _logger;

    private CancellationTokenSource? _debounce;

    public GruposViewModel(IGrupoProdutoService grupos, GrupoFormViewModel form, ILogger<GruposViewModel> logger)
    {
        _grupos = grupos;
        _form = form;
        _logger = logger;
        _form.Salvo += async () => { FecharFormulario(); await CarregarAsync(); };
        _form.Cancelado += FecharFormulario;
    }

    public ObservableCollection<GrupoProdutoDto> Grupos { get; } = new();

    [ObservableProperty]
    private string _filtro = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    public string TextoContador => Grupos.Count switch
    {
        0 => "Nenhum grupo",
        1 => "1 grupo",
        var n => $"{n} grupos",
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
    private GrupoFormViewModel? _formularioAtivo;

    public bool MostrarFormulario => FormularioAtivo is not null;

    partial void OnFormularioAtivoChanged(GrupoFormViewModel? value) =>
        OnPropertyChanged(nameof(MostrarFormulario));

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
        await _form.PrepararNovoAsync();
        FormularioAtivo = _form;
    }

    [RelayCommand]
    private async Task AbrirAsync(GrupoProdutoDto? grupo)
    {
        if (grupo is null)
            return;

        await _form.CarregarAsync(grupo.Id);
        FormularioAtivo = _form;
    }

    private void FecharFormulario() => FormularioAtivo = null;
}
