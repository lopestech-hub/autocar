using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Produtos;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Formulário de Grupo de produto em dois modos (visualização/edição). Novo abre em edição.
/// Além da descrição, o grupo pertence a uma categoria (combo obrigatório).
/// </summary>
public partial class GrupoFormViewModel : ViewModelBase
{
    private readonly IGrupoProdutoService _grupos;
    private readonly ILogger<GrupoFormViewModel> _logger;
    private Guid? _id;

    public GrupoFormViewModel(IGrupoProdutoService grupos, ILogger<GrupoFormViewModel> logger)
    {
        _grupos = grupos;
        _logger = logger;
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    /// <summary>Categorias para o combo (obrigatório — o grupo pertence a uma categoria).</summary>
    public ObservableCollection<OpcaoDto> Categorias { get; } = new();

    [ObservableProperty] private string _descricao = string.Empty;
    [ObservableProperty] private OpcaoDto? _categoriaSelecionada;
    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string Titulo => _id is null ? "Novo Grupo" : $"Grupo {Descricao}";

    public async Task PrepararNovoAsync()
    {
        await CarregarCategoriasAsync();
        _id = null;
        Descricao = string.Empty;
        CategoriaSelecionada = null;
        MensagemErro = null;
        ModoVisualizacao = false;
        OnPropertyChanged(nameof(Titulo));
    }

    public async Task CarregarAsync(Guid id)
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            await CarregarCategoriasAsync();

            var resultado = await _grupos.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var g = resultado.Valor;
            _id = g.Id;
            Descricao = g.Descricao;
            // Selecionar pelo Id dentro da coleção do combo (matching por referência do Avalonia).
            CategoriaSelecionada = Categorias.FirstOrDefault(c => c.Id == g.IdCategoria);
            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar o grupo.";
            _logger.LogError(ex, "Erro ao carregar grupo {Id}.", id);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void HabilitarEdicao() => ModoVisualizacao = false;

    [RelayCommand]
    private async Task SalvarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            if (CategoriaSelecionada is null)
            {
                MensagemErro = "Selecione a categoria do grupo.";
                return;
            }

            var dto = new SalvarGrupoProdutoDto(Descricao, CategoriaSelecionada.Id);

            var resultado = _id is null
                ? await _grupos.CriarAsync(dto)
                : await _grupos.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar o grupo.";
            _logger.LogError(ex, "Erro ao salvar grupo.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();

    private async Task CarregarCategoriasAsync()
    {
        var categorias = await _grupos.ListarCategoriasAsync();
        Categorias.Clear();
        foreach (var c in categorias)
            Categorias.Add(c);
    }
}
