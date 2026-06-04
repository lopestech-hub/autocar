using System;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Mecanicos;
using AutoCar.Application.Modules.Registrations.Mecanicos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Formulário de Mecânico em dois modos (visualização/edição). Novo abre em edição.
/// Cadastro enxuto: nome + telefone. Mesmo padrão do form de Serviço.
/// </summary>
public partial class MecanicoFormViewModel : ViewModelBase
{
    private readonly IMecanicoService _mecanicos;
    private readonly ILogger<MecanicoFormViewModel> _logger;
    private Guid? _id;

    public MecanicoFormViewModel(IMecanicoService mecanicos, ILogger<MecanicoFormViewModel> logger)
    {
        _mecanicos = mecanicos;
        _logger = logger;
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    [ObservableProperty] private string _nome = string.Empty;
    [ObservableProperty] private string? _telefone;
    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string Titulo => _id is null ? "Novo Mecânico" : $"Mecânico {Nome}";

    public void PrepararNovo()
    {
        _id = null;
        Nome = string.Empty;
        Telefone = null;
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
            var resultado = await _mecanicos.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var m = resultado.Valor;
            _id = m.Id;
            Nome = m.Nome;
            Telefone = m.Telefone;
            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar o mecânico.";
            _logger.LogError(ex, "Erro ao carregar mecânico {Id}.", id);
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
            var dto = new SalvarMecanicoDto(Nome, Telefone);

            var resultado = _id is null
                ? await _mecanicos.CriarAsync(dto)
                : await _mecanicos.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar o mecânico.";
            _logger.LogError(ex, "Erro ao salvar mecânico.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();
}
