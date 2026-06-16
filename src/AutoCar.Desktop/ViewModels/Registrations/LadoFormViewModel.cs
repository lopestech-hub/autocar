using System;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Produtos;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Formulário de Lado da peça em dois modos (visualização/edição). Novo abre em edição.
/// Cadastro enxuto: só descrição. Mesmo padrão de form de Marca.
/// </summary>
public partial class LadoFormViewModel : ViewModelBase
{
    private readonly ILadoPecaService _lados;
    private readonly ILogger<LadoFormViewModel> _logger;
    private Guid? _id;

    public LadoFormViewModel(ILadoPecaService lados, ILogger<LadoFormViewModel> logger)
    {
        _lados = lados;
        _logger = logger;
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    [ObservableProperty] private string _descricao = string.Empty;
    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string Titulo => _id is null ? "Novo Lado" : $"Lado {Descricao}";

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
            var resultado = await _lados.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var l = resultado.Valor;
            _id = l.Id;
            Descricao = l.Descricao;
            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar o lado.";
            _logger.LogError(ex, "Erro ao carregar lado {Id}.", id);
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
            var dto = new SalvarLadoPecaDto(Descricao);

            var resultado = _id is null
                ? await _lados.CriarAsync(dto)
                : await _lados.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar o lado.";
            _logger.LogError(ex, "Erro ao salvar lado.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();
}
