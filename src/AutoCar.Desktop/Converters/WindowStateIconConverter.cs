using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace AutoCar.Desktop.Converters;

/// <summary>
/// Converte o <see cref="WindowState"/> da janela no ícone do botão maximizar/restaurar da barra
/// de título customizada: maximizada mostra "restaurar", caso contrário "maximizar".
/// Só conversão de saída.
/// </summary>
public sealed class WindowStateIconConverter : IValueConverter
{
    public static readonly WindowStateIconConverter Instancia = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is WindowState.Maximized
            ? "fa-regular fa-window-restore"
            : "fa-regular fa-window-maximize";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
