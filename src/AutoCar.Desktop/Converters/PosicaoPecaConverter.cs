using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AutoCar.Domain.Enums;

namespace AutoCar.Desktop.Converters;

/// <summary>
/// Converte <see cref="PosicaoPeca"/> em um rótulo amigável para exibição no combo e na
/// listagem. NaoAplica vira "—" (peça sem distinção de eixo), igual ao padrão dos campos
/// opcionais do projeto. Só conversão de saída (enum → texto).
/// </summary>
public sealed class PosicaoPecaConverter : IValueConverter
{
    public static readonly PosicaoPecaConverter Instancia = new();

    public static string Rotular(PosicaoPeca posicao) => posicao switch
    {
        PosicaoPeca.Dianteira => "Dianteira",
        PosicaoPeca.Traseira => "Traseira",
        _ => "—",
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is PosicaoPeca p ? Rotular(p) : "—";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
