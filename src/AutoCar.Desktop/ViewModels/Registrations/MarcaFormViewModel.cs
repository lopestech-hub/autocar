using System;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Produtos;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Formulário de Marca em dois modos (visualização/edição). Novo abre em edição.
/// Cadastro enxuto: só descrição. Mesmo padrão de form de Cliente/Fornecedor.
/// </summary>
public partial class MarcaFormViewModel : ViewModelBase
{
    private readonly IMarcaService _marcas;
    private readonly ILogger<MarcaFormViewModel> _logger;
    private Guid? _id;

    public MarcaFormViewModel(IMarcaService marcas, ILogger<MarcaFormViewModel> logger)
    {
        _marcas = marcas;
        _logger = logger;
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    [ObservableProperty] private string _descricao = string.Empty;
    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string Titulo => _id is null ? "Nova Marca" : $"Marca {Descricao}";

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
            var resultado = await _marcas.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var m = resultado.Valor;
            _id = m.Id;
            Descricao = m.Descricao;
            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar a marca.";
            _logger.LogError(ex, "Erro ao carregar marca {Id}.", id);
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
            var dto = new SalvarMarcaDto(Descricao);

            var resultado = _id is null
                ? await _marcas.CriarAsync(dto)
                : await _marcas.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar a marca.";
            _logger.LogError(ex, "Erro ao salvar marca.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();
}
