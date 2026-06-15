using System;
using System.Globalization;
using Avalonia.Data.Converters;
using AutoCar.Desktop.Services;

namespace AutoCar.Desktop.Converters;

/// <summary>
/// Converte o NOME do arquivo de imagem do produto (ex: "27022.jpg") no Bitmap carregado da
/// pasta-base (Imagens:PastaBase). Usado no ToolTip do Catálogo: o Avalonia só materializa o
/// conteúdo do tooltip quando ele vai abrir (hover), então a imagem carrega sob demanda — não
/// todas de uma vez. Retorna null quando não há imagem ou o arquivo não existe (tooltip vazio).
/// </summary>
public sealed class ImagemProdutoConverter : IValueConverter
{
    public static readonly ImagemProdutoConverter Instancia = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        ImagemProduto.Carregar(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>True quando o produto tem imagem existente na pasta — controla se o ToolTip aparece.</summary>
public sealed class TemImagemProdutoConverter : IValueConverter
{
    public static readonly TemImagemProdutoConverter Instancia = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        ImagemProduto.Existe(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
