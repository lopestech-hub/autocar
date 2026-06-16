using System;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Produtos;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Formulário de Posição da peça em dois modos (visualização/edição). Novo abre em edição.
/// Cadastro enxuto: só descrição. Mesmo padrão de form de Marca.
/// </summary>
public partial class PosicaoFormViewModel : ViewModelBase
{
    private readonly IPosicaoPecaService _posicoes;
    private readonly ILogger<PosicaoFormViewModel> _logger;
    private Guid? _id;

    public PosicaoFormViewModel(IPosicaoPecaService posicoes, ILogger<PosicaoFormViewModel> logger)
    {
        _posicoes = posicoes;
        _logger = logger;
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    [ObservableProperty] private string _descricao = string.Empty;
    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string Titulo => _id is null ? "Nova Posição" : $"Posição {Descricao}";

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
            var resultado = await _posicoes.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var p = resultado.Valor;
            _id = p.Id;
            Descricao = p.Descricao;
            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar a posição.";
            _logger.LogError(ex, "Erro ao carregar posição {Id}.", id);
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
            var dto = new SalvarPosicaoPecaDto(Descricao);

            var resultado = _id is null
                ? await _posicoes.CriarAsync(dto)
                : await _posicoes.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar a posição.";
            _logger.LogError(ex, "Erro ao salvar posição.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();
}
