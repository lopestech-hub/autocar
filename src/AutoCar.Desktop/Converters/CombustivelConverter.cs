using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AutoCar.Domain.Enums;

namespace AutoCar.Desktop.Converters;

/// <summary>
/// Converte <see cref="Combustivel"/> em rótulo amigável para o combo e a exibição.
/// NaoAplica fica em branco (sem traço na UI), igual ao padrão dos campos opcionais do projeto.
/// Só conversão de saída (enum → texto).
/// </summary>
public sealed class CombustivelConverter : IValueConverter
{
    public static readonly CombustivelConverter Instancia = new();

    public static string Rotular(Combustivel combustivel) => combustivel switch
    {
        Combustivel.Flex => "Flex",
        Combustivel.Gasolina => "Gasolina",
        Combustivel.Diesel => "Diesel",
        Combustivel.Etanol => "Etanol",
        Combustivel.GNV => "GNV",
        _ => "",
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Combustivel c ? Rotular(c) : "";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
