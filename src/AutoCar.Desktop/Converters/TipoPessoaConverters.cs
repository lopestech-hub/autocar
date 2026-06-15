using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AutoCar.Domain.Enums;

namespace AutoCar.Desktop.Converters;

/// <summary>Rótulo do badge de tipo de pessoa: "PF" (Física) / "PJ" (Jurídica).</summary>
public sealed class TipoPessoaTextoConverter : IValueConverter
{
    public static readonly TipoPessoaTextoConverter Instancia = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TipoPessoa.Fisica ? "PF" : "PJ";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Cor de FUNDO do badge de tipo: azul-claro (PF) / roxo-claro (PJ).</summary>
public sealed class TipoPessoaFundoConverter : IValueConverter
{
    public static readonly TipoPessoaFundoConverter Instancia = new();

    private static readonly IBrush Fisica = new SolidColorBrush(Color.Parse("#E0F2FE"));
    private static readonly IBrush Juridica = new SolidColorBrush(Color.Parse("#EDE9FE"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TipoPessoa.Fisica ? Fisica : Juridica;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Cor de TEXTO do badge de tipo: azul-escuro (PF) / roxo-escuro (PJ).</summary>
public sealed class TipoPessoaTextoCorConverter : IValueConverter
{
    public static readonly TipoPessoaTextoCorConverter Instancia = new();

    private static readonly IBrush Fisica = new SolidColorBrush(Color.Parse("#075985"));
    private static readonly IBrush Juridica = new SolidColorBrush(Color.Parse("#5B21B6"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TipoPessoa.Fisica ? Fisica : Juridica;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// Máscara de exibição do documento (o banco guarda só dígitos): CPF 000.000.000-00 (PF) ou
/// CNPJ 00.000.000/0000-00 (PJ). MultiValue: [0]=Documento (string), [1]=TipoPessoa (enum).
/// </summary>
public sealed class DocumentoMascaraConverter : IMultiValueConverter
{
    public static readonly DocumentoMascaraConverter Instancia = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not string doc)
            return string.Empty;

        var tipo = values[1] is TipoPessoa t ? t : TipoPessoa.Fisica;

        if (tipo == TipoPessoa.Fisica && doc.Length == 11)
            return $"{doc[..3]}.{doc.Substring(3, 3)}.{doc.Substring(6, 3)}-{doc.Substring(9, 2)}";
        if (tipo == TipoPessoa.Juridica && doc.Length == 14)
            return $"{doc[..2]}.{doc.Substring(2, 3)}.{doc.Substring(5, 3)}/{doc.Substring(8, 4)}-{doc.Substring(12, 2)}";
        return doc;
    }
}
