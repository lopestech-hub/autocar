using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Linha editável do mini-grid de aplicações por veículo (dentro do formulário de Produto).
/// Anos como texto para parse tolerante (vazio = sem ano). Salva junto com o produto.
/// </summary>
public partial class AplicacaoItemViewModel : ViewModelBase
{
    [ObservableProperty] private string _montadora = string.Empty;
    [ObservableProperty] private string _modelo = string.Empty;
    [ObservableProperty] private string? _anoInicioTexto;
    [ObservableProperty] private string? _anoFimTexto;
    [ObservableProperty] private string? _observacao;

    public AplicacaoItemViewModel() { }

    public AplicacaoItemViewModel(string montadora, string modelo, int? anoInicio, int? anoFim, string? observacao)
    {
        Montadora = montadora;
        Modelo = modelo;
        AnoInicioTexto = anoInicio?.ToString();
        AnoFimTexto = anoFim?.ToString();
        Observacao = observacao;
    }

    /// <summary>Linha "preenchível": tem ao menos montadora ou modelo. Linhas vazias são descartadas ao salvar.</summary>
    public bool TemConteudo => !string.IsNullOrWhiteSpace(Montadora) || !string.IsNullOrWhiteSpace(Modelo);

    public int? AnoInicio => ParseAno(AnoInicioTexto);
    public int? AnoFim => ParseAno(AnoFimTexto);

    private static int? ParseAno(string? texto) =>
        int.TryParse((texto ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)
            ? v
            : null;
}
