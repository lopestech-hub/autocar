using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AutoCar.Desktop.Converters;

/// <summary>
/// Converte <see cref="bool"/> (flg_ativo) no rótulo do badge de status: "Ativo" / "Inativo".
/// Usado no badge das listagens (Produto e futuros). Só conversão de saída.
/// </summary>
public sealed class StatusAtivoTextoConverter : IValueConverter
{
    public static readonly StatusAtivoTextoConverter Instancia = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? "Ativo" : "Inativo";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Cor de FUNDO do badge de status: verde-claro (ativo) / vermelho-claro (inativo).
/// Mesmas cores do badge original montado em code-behind.
/// </summary>
public sealed class StatusAtivoFundoConverter : IValueConverter
{
    public static readonly StatusAtivoFundoConverter Instancia = new();

    private static readonly IBrush Ativo = new SolidColorBrush(Color.Parse("#DCFCE7"));
    private static readonly IBrush Inativo = new SolidColorBrush(Color.Parse("#FEE2E2"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Ativo : Inativo;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Cor de TEXTO do badge de status: verde-escuro (ativo) / vermelho-escuro (inativo).
/// </summary>
public sealed class StatusAtivoTextoCorConverter : IValueConverter
{
    public static readonly StatusAtivoTextoCorConverter Instancia = new();

    private static readonly IBrush Ativo = new SolidColorBrush(Color.Parse("#166534"));
    private static readonly IBrush Inativo = new SolidColorBrush(Color.Parse("#991B1B"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Ativo : Inativo;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
