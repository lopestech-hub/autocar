using System;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Servicos;
using AutoCar.Application.Modules.Registrations.Servicos.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Formulário de Serviço em dois modos (visualização/edição). Novo abre em edição.
/// Cadastro enxuto: descrição + valor padrão (mão de obra). Mesmo padrão do form de Marca.
/// </summary>
public partial class ServicoFormViewModel : ViewModelBase
{
    private readonly IServicoService _servicos;
    private readonly ILogger<ServicoFormViewModel> _logger;
    private Guid? _id;

    public ServicoFormViewModel(IServicoService servicos, ILogger<ServicoFormViewModel> logger)
    {
        _servicos = servicos;
        _logger = logger;
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    [ObservableProperty] private string _descricao = string.Empty;
    [ObservableProperty] private decimal _vlrPadrao;
    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string Titulo => _id is null ? "Novo Serviço" : $"Serviço {Descricao}";

    /// <summary>
    /// Valor padrão como texto editável (a View usa um TextBox — o NumericUpDown do Fluent
    /// dá problema de estilo). Parse tolerante (vírgula ou ponto), exibe em moeda BR. A fonte
    /// da verdade continua sendo VlrPadrao (decimal).
    /// </summary>
    public string VlrPadraoTexto
    {
        get => VlrPadrao.ToString("N2", new System.Globalization.CultureInfo("pt-BR"));
        set
        {
            var limpo = (value ?? string.Empty).Trim().Replace(".", "").Replace(",", ".");
            VlrPadrao = decimal.TryParse(limpo, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;
            OnPropertyChanged();
        }
    }

    partial void OnVlrPadraoChanged(decimal value) => OnPropertyChanged(nameof(VlrPadraoTexto));

    public void PrepararNovo()
    {
        _id = null;
        Descricao = string.Empty;
        VlrPadrao = 0;
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
            var resultado = await _servicos.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var s = resultado.Valor;
            _id = s.Id;
            Descricao = s.Descricao;
            VlrPadrao = s.VlrPadrao;
            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar o serviço.";
            _logger.LogError(ex, "Erro ao carregar serviço {Id}.", id);
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
            var dto = new SalvarServicoDto(Descricao, VlrPadrao);

            var resultado = _id is null
                ? await _servicos.CriarAsync(dto)
                : await _servicos.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar o serviço.";
            _logger.LogError(ex, "Erro ao salvar serviço.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();
}
