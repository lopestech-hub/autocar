using System;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Produtos;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Formulário de Categoria de produto em dois modos (visualização/edição). Novo abre
/// em edição. Cadastro enxuto: só descrição. Mesmo padrão de Cliente/Fornecedor.
/// </summary>
public partial class CategoriaFormViewModel : ViewModelBase
{
    private readonly ICategoriaProdutoService _categorias;
    private readonly ILogger<CategoriaFormViewModel> _logger;
    private Guid? _id;

    public CategoriaFormViewModel(ICategoriaProdutoService categorias, ILogger<CategoriaFormViewModel> logger)
    {
        _categorias = categorias;
        _logger = logger;
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    [ObservableProperty] private string _descricao = string.Empty;
    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string Titulo => _id is null ? "Nova Categoria" : $"Categoria {Descricao}";

    public void PrepararNovo()
    {
        _id = null;
        Descricao = string.Empty;
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
            var resultado = await _categorias.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var c = resultado.Valor;
            _id = c.Id;
            Descricao = c.Descricao;
            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar a categoria.";
            _logger.LogError(ex, "Erro ao carregar categoria {Id}.", id);
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
            var dto = new SalvarCategoriaProdutoDto(Descricao);

            var resultado = _id is null
                ? await _categorias.CriarAsync(dto)
                : await _categorias.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar a categoria.";
            _logger.LogError(ex, "Erro ao salvar categoria.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();
}
