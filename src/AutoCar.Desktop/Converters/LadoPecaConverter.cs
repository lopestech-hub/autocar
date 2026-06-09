using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AutoCar.Domain.Enums;

namespace AutoCar.Desktop.Converters;

/// <summary>
/// Converte <see cref="LadoPeca"/> em um rótulo amigável para o combo e a listagem.
/// NaoAplica vira "—" (peça sem distinção de lado), igual ao padrão dos campos opcionais
/// do projeto. Só conversão de saída (enum → texto).
/// </summary>
public sealed class LadoPecaConverter : IValueConverter
{
    public static readonly LadoPecaConverter Instancia = new();

    public static string Rotular(LadoPeca lado) => lado switch
    {
        LadoPeca.Esquerdo => "Esquerdo",
        LadoPeca.Direito => "Direito",
        _ => "—",
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is LadoPeca l ? Rotular(l) : "—";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
