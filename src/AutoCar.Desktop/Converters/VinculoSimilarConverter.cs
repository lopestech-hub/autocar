using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AutoCar.Desktop.Converters;

/// <summary>
/// Converte o booleano <c>Vinculado</c> de uma equivalência (a marca equivalente já é produto da
/// loja?) em texto e cor para a coluna "Estoque" do mini-grid de equivalências. Vinculado = "Sim"
/// (verde); só referência externa = em branco. Só conversão de saída.
/// </summary>
public sealed class VinculoSimilarConverter : IValueConverter
{
    /// <summary>"Sim" quando vinculado a um produto; em branco quando é só referência externa.</summary>
    public static readonly VinculoSimilarConverter Texto = new(modoTexto: true);

    /// <summary>Verde quando vinculado; cinza-mudo quando só referência.</summary>
    public static readonly VinculoSimilarConverter Cor = new(modoTexto: false);

    private static readonly IBrush Verde = new SolidColorBrush(Color.Parse("#166534"));
    private static readonly IBrush Mudo = new SolidColorBrush(Color.Parse("#94A3B8"));

    private readonly bool _modoTexto;

    private VinculoSimilarConverter(bool modoTexto) => _modoTexto = modoTexto;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var vinculado = value is true;
        if (_modoTexto)
            return vinculado ? "Sim" : "";
        return vinculado ? Verde : Mudo;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
