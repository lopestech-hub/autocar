using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AutoCar.Desktop.Converters;

/// <summary>
/// bool → Brush de FUNDO do realce de desconto: true = âmbar claro (#FEF3C7), false = transparente.
/// Sinaliza, sem quebrar a edição, que a linha do item tem desconto aplicado.
/// </summary>
public sealed class DescontoFundoConverter : IValueConverter
{
    public static readonly DescontoFundoConverter Instancia = new();
    private static readonly IBrush Ambar = new SolidColorBrush(Color.Parse("#FEF3C7"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Ambar : Brushes.Transparent;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// decimal → Brush do valor do TOTAL do documento: > 0 = azul primário (#3B82F6),
/// 0 = cinza mudo (#94A3B8). Deixa claro quando o documento ainda não tem valor.
/// </summary>
public sealed class TotalBrushConverter : IValueConverter
{
    public static readonly TotalBrushConverter Instancia = new();
    private static readonly IBrush Azul = new SolidColorBrush(Color.Parse("#3B82F6"));
    private static readonly IBrush Cinza = new SolidColorBrush(Color.Parse("#94A3B8"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is decimal v && v > 0 ? Azul : Cinza;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
